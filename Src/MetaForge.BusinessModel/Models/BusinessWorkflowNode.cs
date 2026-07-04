namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Business workflow — sekvence kroků a přechodů modelující business proces.
/// Např. "Schvalování objednávky", "Onboarding zaměstnance".
/// </summary>
public sealed record BusinessWorkflowNode
{
    /// <summary>Unikátní identifikátor workflow.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název workflow (např. "Schvalování objednávky").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Popis workflow.</summary>
    public string? Description { get; init; }

    /// <summary>Kroky workflow.</summary>
    public IReadOnlyList<BusinessWorkflowStepNode> Steps { get; init; } = [];

    /// <summary>Přechody mezi kroky.</summary>
    public IReadOnlyList<BusinessWorkflowTransitionNode> Transitions { get; init; } = [];
}
