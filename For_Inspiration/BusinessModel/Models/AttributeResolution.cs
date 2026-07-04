namespace MetaForge.BusinessModel;

/// <summary>
/// Výsledek resolvování typu atributu — kombinace CatalogManager.ResolveType() + SuggestPresets().
/// Používá se v enrichment-before-save flow pro zobrazení 🟢 obohacených údajů v editoru.
/// </summary>
public sealed record AttributeResolution
{
    /// <summary>Výsledný typ po resolvování (např. "decimal", "Money").</summary>
    public string ResolvedType { get; init; } = "text";

    /// <summary>Podkladový primitivní typ, pokud je StrongType (např. "decimal").</summary>
    public string? UnderlyingType { get; init; }

    /// <summary>Je atribut StrongType (má value object preset)?</summary>
    public bool IsStrongType { get; init; }

    /// <summary>Název value objectu, pokud je StrongType (např. "Money").</summary>
    public string? ValueObjectName { get; init; }

    /// <summary>ID katalogové položky, pokud type odpovídá presetu.</summary>
    public string? CatalogId { get; init; }

    /// <summary>Validační pravidla navržená z presetu/CustomType.</summary>
    public IReadOnlyList<string> CandidateValidationRules { get; init; } = [];

    /// <summary>Seznam navržených presetů (ID + DisplayName).</summary>
    public IReadOnlyList<string> SuggestedPresets { get; init; } = [];
}
