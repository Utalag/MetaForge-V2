namespace MetaForge.BusinessModel;

public sealed class BusinessBehaviorNode
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public BusinessBehaviorKind Kind { get; init; } = BusinessBehaviorKind.Query;

    public string? Summary { get; init; }

    public IReadOnlyList<BusinessBehaviorInputNode> Inputs { get; init; } = [];

    public string? Returns { get; init; }

    public IReadOnlyList<BusinessNoteNode> Notes { get; init; } = [];
}