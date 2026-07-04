namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Workflow krok — jeden krok v business workflow.
/// </summary>
public sealed record BusinessWorkflowStepNode
{
    /// <summary>Unikátní identifikátor kroku.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název kroku (např. "Schválení objednávky").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Popis kroku.</summary>
    public string? Description { get; init; }

    /// <summary>Druh kroku (manuální, automatický, rozhodovací, čekací).</summary>
    public BusinessWorkflowStepKind Kind { get; init; } = BusinessWorkflowStepKind.Manual;

    /// <summary>Pořadí kroku ve workflow (0-based).</summary>
    public int Order { get; init; }

    /// <summary>Binding detail — na jakou entitu/behavior je krok navázán.</summary>
    public BusinessWorkflowStepBindingDetail? Binding { get; init; }
}
