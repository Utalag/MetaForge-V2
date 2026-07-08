namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Neutralni metadata jednoho parametru capability.
/// Type je neutralni string ("number", "string", "boolean", "object").
/// </summary>
public sealed record ParameterMetadata
{
    public required string Name { get; init; }

    public required string Type { get; init; }

    public string Description { get; init; } = string.Empty;

    public bool Required { get; init; } = true;

    public string? DefaultValue { get; init; }
}
