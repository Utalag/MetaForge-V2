namespace MetaForge.BusinessModel;

public sealed class BusinessBehaviorInputNode
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Type { get; init; } = "text";

    public bool Required { get; init; }

    public string? Summary { get; init; }
}