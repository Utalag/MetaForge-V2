namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Workflow přechod — spojnice mezi dvěma kroky workflow.
/// </summary>
public sealed record BusinessWorkflowTransitionNode
{
    /// <summary>Unikátní identifikátor přechodu.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>ID zdrojového kroku.</summary>
    public string FromStepId { get; init; } = string.Empty;

    /// <summary>ID cílového kroku.</summary>
    public string ToStepId { get; init; } = string.Empty;

    /// <summary>Podmínka přechodu (např. "objednávka.Schválena == true").</summary>
    public string? Condition { get; init; }

    /// <summary>Popisek přechodu (např. "Schváleno", "Zamítnuto").</summary>
    public string? Label { get; init; }
}
