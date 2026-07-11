using MetaForge.BusinessModel.Models;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Translator.Translation;

/// <summary>
/// Překládá business atributy na Core TypeModel.
/// </summary>
public interface IBusinessTranslator
{
    /// <summary>Přeloží BusinessAttributeNode na TypeModel.</summary>
    TypeModel Translate(BusinessAttributeNode attribute);

    /// <summary>Pokusí se o enrichment (AI/deterministický). Vrací null pokud nic.</summary>
    EnrichmentResult? TryEnrich(BusinessAttributeNode attribute);

    /// <summary>Přeloží celý BusinessAuthoringDocument na Core elementy.</summary>
    IReadOnlyList<RootElement> TranslateDocument(BusinessAuthoringDocument document);
}

/// <summary>
/// Výsledek enrichmentu — dodatečné informace o atributu.
/// </summary>
public sealed record EnrichmentResult(
    string AttributeId,
    string? SuggestedCSharpType = null,
    IReadOnlyList<string>? ValidationRules = null,
    string? DefaultValue = null,
    int? MaxLength = null
);
