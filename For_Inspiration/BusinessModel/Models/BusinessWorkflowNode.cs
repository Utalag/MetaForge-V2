namespace MetaForge.BusinessModel;

public sealed class BusinessWorkflowNode
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Summary { get; init; }

    public string? Trigger { get; init; }

    public string? PresetId { get; init; }

    public IReadOnlyList<BusinessWorkflowStepNode> Steps { get; init; } = [];

    public IReadOnlyList<BusinessWorkflowTransitionNode> Transitions { get; init; } = [];

    public IReadOnlyList<BusinessNoteNode> Notes { get; init; } = [];
}