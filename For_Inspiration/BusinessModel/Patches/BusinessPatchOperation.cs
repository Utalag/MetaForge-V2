namespace MetaForge.BusinessModel;

public sealed class BusinessPatchOperation
{
    public string Op { get; init; } = string.Empty;

    public string? EntityId { get; init; }

    public string? AttributeId { get; init; }

    public string? BehaviorId { get; init; }

    public string? RelationId { get; init; }

    public string? QuestionId { get; init; }

    public string? WorkflowId { get; init; }

    public string? WorkflowStepId { get; set; }

    public string? WorkflowTransitionId { get; init; }

    public int? NewIndex { get; init; }

    public Dictionary<string, object?> Data { get; init; } = [];
}