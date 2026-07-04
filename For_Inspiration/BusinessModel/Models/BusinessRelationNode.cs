namespace MetaForge.BusinessModel;

public sealed class BusinessRelationNode
{
    public string Id { get; init; } = string.Empty;

    public string SourceEntityId { get; init; } = string.Empty;

    public string TargetEntityId { get; init; } = string.Empty;

    public BusinessRelationKind Kind { get; init; } = BusinessRelationKind.BelongsTo;

    public string? SourceNavigationName { get; init; }

    public string? TargetNavigationName { get; init; }

    public IReadOnlyList<BusinessNoteNode> Notes { get; init; } = [];
}