namespace MetaForge.BusinessModel;

public sealed class BusinessAttributeNode
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Type { get; init; } = "text";

    public string? CustomType { get; init; }

    public bool Required { get; init; }

    public string? Summary { get; init; }

    public string? DefaultValue { get; init; }

    public IReadOnlyList<string> Constraints { get; init; } = [];

    public string? Computed { get; init; }

    public string? PresetId { get; init; }

    public BusinessAttributeCoreDetail? CoreDetail { get; init; }
}