using MetaForge.BusinessModel.Models;
using MetaForge.Core.Catalog;
using MetaForge.Core.DataTypes;

namespace MetaForge.Translator.Translation;

/// <summary>
/// Výchozí deterministický překladač — business atribut → TypeModel.
/// Používá CatalogManager pro resolvování typů.
/// </summary>
public sealed class DefaultBusinessTranslator : IBusinessTranslator
{
    private readonly CatalogManager _catalog;

    public DefaultBusinessTranslator(CatalogManager catalog)
    {
        _catalog = catalog;
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
}
