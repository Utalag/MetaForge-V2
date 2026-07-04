namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Poznámka k entitě — volný textový komentář.
/// </summary>
public sealed record BusinessNoteNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Text poznámky.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Datum vytvoření.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
