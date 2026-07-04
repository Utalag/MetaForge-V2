namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Nezodpovězená otázka k business modelu — např. "Jaký je formát fakturační adresy?"
/// Slouží pro iterativní upřesňování modelu s uživatelem nebo AI.
/// </summary>
public sealed class PendingQuestionNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Text otázky.</summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>Kontext — ke které entitě/atributu se otázka vztahuje.</summary>
    public string? ContextEntityId { get; set; }

    /// <summary>Datum vytvoření.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
