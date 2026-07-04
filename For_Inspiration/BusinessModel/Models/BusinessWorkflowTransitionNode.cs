namespace MetaForge.BusinessModel;

public sealed class BusinessWorkflowTransitionNode
{
    public string Id { get; init; } = string.Empty;

    public string FromStepId { get; init; } = string.Empty;

    public string ToStepId { get; init; } = string.Empty;

    public string? Label { get; init; }

    public string? Condition { get; init; }

    public bool IsDefault { get; init; }
}