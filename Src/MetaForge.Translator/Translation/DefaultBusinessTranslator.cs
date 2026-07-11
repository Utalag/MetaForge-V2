using MetaForge.BusinessModel.Models;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Catalog;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;
using MetaForge.Translator.Prompting;
using MetaForge.Translator.Prompting.ModelPrompts;

namespace MetaForge.Translator.Translation;

/// <summary>
/// Výchozí deterministický překladač — business atribut → TypeModel.
/// Používá CatalogManager pro resolvování typů.
/// Podporuje volitelný AI enrichment přes IAiTranslator.
/// </summary>
public sealed class DefaultBusinessTranslator : IBusinessTranslator
{
    private readonly CatalogManager _catalog;
    private readonly IAiTranslator? _aiTranslator;

    /// <summary>
    /// Vytvoří překladač bez AI (pouze deterministický).
    /// </summary>
    public DefaultBusinessTranslator(CatalogManager catalog)
    {
        _catalog = catalog;
        _aiTranslator = null;
    }

    /// <summary>
    /// Vytvoří překladač s volitelným AI enrichmentem.
    /// </summary>
    /// <param name="catalog">Katalog typů.</param>
    /// <param name="aiTranslator">AI překladač — může být null (fallback na deterministický).</param>
    public DefaultBusinessTranslator(CatalogManager catalog, IAiTranslator? aiTranslator)
    {
        _catalog = catalog;
        _aiTranslator = aiTranslator;
    }

    /// <summary>
    /// Přeloží business atribut na TypeModel.
    /// 1. Zkusí najít v katalogu.
    /// 2. Pokud nenajde, použije fallback na základě názvu typu.
    /// </summary>
    public TypeModel Translate(BusinessAttributeNode attribute)
    {
        // 1. Katalog
        var preset = _catalog.ResolveType(attribute.Type);
        if (preset is not null)
            return preset.Type;

        // 2. Fallback podle názvu typu
        var type = attribute.Type.ToLowerInvariant() switch
        {
            "string" or "text" => TypeModel.String,
            "int" or "integer" or "int32" => TypeModel.Int32,
            "long" or "int64" => TypeModel.Of(DataType.Int64),
            "decimal" or "money" or "price" => TypeModel.Decimal,
            "double" or "float" => TypeModel.Of(DataType.Double),
            "bool" or "boolean" => TypeModel.Bool,
            "datetime" => TypeModel.DateTime,
            "date" => TypeModel.Of(DataType.DateOnly),
            "guid" or "uuid" => TypeModel.Guid,
            "email" => TypeModel.String,
            "phone" => TypeModel.String,
            "url" or "uri" => TypeModel.Of(DataType.Uri),
            _ => TypeModel.Object,
        };

        // 3. Aplikuj IsRequired
        if (attribute.IsRequired && type.IsNullable)
            type = type with { IsNullable = false };

        return type;
    }

    /// <summary>
    /// Deterministický enrichment — odvodí dodatečné informace.
    /// Nepoužívá AI — jen pravidla.
    /// </summary>
    public EnrichmentResult? TryEnrich(BusinessAttributeNode attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        // Jen pro string atributy bez omezení
        if (attribute.Type is "string" or "text" or "email" or "phone")
        {
            var rules = new List<string>();
            int? maxLength = null;

            if (attribute.IsRequired)
                rules.Add("not_empty");

            switch (attribute.Type)
            {
                case "email":
                    rules.Add("email_format");
                    maxLength = 254;
                    break;
                case "phone":
                    rules.Add("phone_format");
                    maxLength = 20;
                    break;
                case "string" or "text":
                    maxLength = attribute.MaxLength ?? 200;
                    break;
            }

            if (rules.Count > 0 || maxLength is not null)
            {
                return new EnrichmentResult(
                    AttributeId: attribute.Id,
                    SuggestedCSharpType: "string",
                    ValidationRules: rules,
                    MaxLength: maxLength
                );
            }
        }

        return null;
    }

    /// <summary>
    /// AI-assisted enrichment — použije IAiTranslator pro hlubší analýzu.
    /// Pokud AI není dostupná, vrátí deterministický výsledek.
    /// </summary>
    /// <param name="attribute">Atribut k obohacení.</param>
    /// <param name="siblingAttributes">Názvy ostatních atributů entity (pro kontext).</param>
    /// <param name="entityName">Název entity (pro kontext).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Výsledek enrichmentu, nebo null pokud ani AI ani deterministická cesta nic nenašly.</returns>
    public async Task<EnrichmentResult?> TryEnrichAsync(
        BusinessAttributeNode attribute,
        IEnumerable<string> siblingAttributes,
        string? entityName = null,
        CancellationToken ct = default)
    {
        // 1. Zkus AI enrichment (pokud je dostupný)
        if (_aiTranslator is not null)
        {
            try
            {
                var isAvailable = await _aiTranslator.IsAvailableAsync(ct);
                if (isAvailable)
                {
                    var userPrompt = AuthoringTranslationModelPrompt.BuildUserPrompt(
                        attribute.Name,
                        attribute.Type,
                        siblingAttributes,
                        entityName);

                    var aiResult = await _aiTranslator.CompleteStructuredAsync<SemanticBriefJson>(
                        AuthoringTranslationModelPrompt.SystemPrompt,
                        userPrompt,
                        ct);

                    if (aiResult is not null && aiResult.Confidence > 0.5)
                    {
                        return new EnrichmentResult(
                            AttributeId: attribute.Id,
                            SuggestedCSharpType: aiResult.SuggestedType,
                            ValidationRules: aiResult.ValidationRules,
                            DefaultValue: aiResult.DefaultValue,
                            MaxLength: aiResult.MaxLength
                        );
                    }
                }
            }
            catch
            {
                // Graceful fallback — pokračuj na deterministickou cestu
            }
        }

        // 2. Fallback na deterministický enrichment
        return TryEnrich(attribute);
    }

    // === Strong Type Mapping (PROP-047) ===

    /// <summary>
    /// Přeloží celý BusinessAuthoringDocument na Core elementy.
    /// Detekuje strong types (ValueObjectElement) přes CoreDetail.IsStrongType.
    /// </summary>
    public IReadOnlyList<RootElement> TranslateDocument(BusinessAuthoringDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var result = new List<RootElement>();
        var translationSource = DetermineTranslationSource(document);

        foreach (var entity in document.Entities)
        {
            var classElement = new ClassElement { Name = entity.Name };

            // Translation source metadata
            if (translationSource != null)
                classElement.Metadata.Set("Generation.TranslationSource", translationSource);

            foreach (var attr in entity.Attributes)
            {
                // Strong type detekce přes CoreDetail
                if (attr.CoreDetail?.IsStrongType == true && attr.CoreDetail?.ValueObjectName != null)
                {
                    var ctd = document.CustomTypes
                        .FirstOrDefault(ct => ct.Name == attr.CoreDetail.ValueObjectName);

                    if (ctd != null)
                    {
                        // Vytvořit ValueObjectElement (Vogen-annotated target)
                        var vo = new ValueObjectElement
                        {
                            Name = ctd.Name,
                            IsReadOnly = true,
                        };

                        // Translation source metadata na value object
                        if (translationSource != null)
                            vo.Metadata.Set("Generation.TranslationSource", translationSource);

                        // Property s odkazem na strong type
                        classElement.InlineStrongTypes.Add(vo);
                        classElement.Properties.Add(PropertyElement.GetSet(attr.Name,
                            TypeModel.Of(DataType.Struct).WithCustomName(ctd.Name)));
                        continue;
                    }
                }

                // Fallback na primitivum
                classElement.Properties.Add(PropertyElement.GetSet(attr.Name, Translate(attr)));
            }

            result.Add(classElement);
        }

        return result;
    }

    /// <summary>
    /// Určí zdroj překladu — AI nebo deterministický.
    /// </summary>
    private static string? DetermineTranslationSource(BusinessAuthoringDocument document)
    {
        // Default: Deterministic, dokud AI vrstva nezapíše metadata
        return "Deterministic";
    }

    /// <summary>
    /// Převede base type string z CustomTypeDefinition na TypeModel.
    /// </summary>
    private static TypeModel MapBaseType(string baseType) => baseType.ToLowerInvariant() switch
    {
        "string" or "text" => TypeModel.String,
        "int" or "integer" or "int32" => TypeModel.Int32,
        "long" or "int64" => TypeModel.Of(DataType.Int64),
        "decimal" or "money" => TypeModel.Decimal,
        "double" => TypeModel.Of(DataType.Double),
        "bool" or "boolean" => TypeModel.Bool,
        "datetime" => TypeModel.DateTime,
        "guid" => TypeModel.Guid,
        _ => TypeModel.Object,
    };
}
