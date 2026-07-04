namespace MetaForge.BusinessModel;

public sealed class PendingQuestionNode
{
    public string Id { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;

    public PendingQuestionStatus Status { get; init; } = PendingQuestionStatus.Open;

    public PendingQuestionScope Scope { get; init; } = PendingQuestionScope.Project;

    public string? RelatedEntityId { get; init; }

    public string? RelatedAttributeId { get; init; }

    public string? RelatedBehaviorId { get; init; }

    public string? RelatedRelationId { get; init; }
}