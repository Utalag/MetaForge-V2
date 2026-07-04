namespace MetaForge.BusinessModel;

public sealed class BusinessWorkflowStepNode
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public BusinessWorkflowStepKind Kind { get; init; } = BusinessWorkflowStepKind.Task;

    public string? Summary { get; init; }

    public string? RelatedEntityId { get; init; }

    public string? RelatedBehaviorId { get; init; }

    public string? Actor { get; init; }

    public IReadOnlyList<string> Inputs { get; init; } = [];

    public IReadOnlyList<string> Outputs { get; init; } = [];

    public BusinessWorkflowStepBindingDetail? BindingDetail { get; init; }

    public IReadOnlyList<BusinessNoteNode> Notes { get; init; } = [];
}