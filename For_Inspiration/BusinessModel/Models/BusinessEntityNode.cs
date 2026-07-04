namespace MetaForge.BusinessModel;

public sealed class BusinessEntityNode
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Summary { get; init; }

    public string? Icon { get; init; }

    public string? PresetId { get; init; }

    public IReadOnlyList<BusinessAttributeNode> Attributes { get; init; } = [];

    public IReadOnlyList<BusinessBehaviorNode> Behaviors { get; init; } = [];

    public IReadOnlyList<BusinessNoteNode> Notes { get; init; } = [];
}