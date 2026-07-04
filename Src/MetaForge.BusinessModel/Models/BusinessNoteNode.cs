namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Poznámka k entitě — volný textový komentář.
/// </summary>
public sealed class BusinessNoteNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Text poznámky.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Datum vytvoření.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
