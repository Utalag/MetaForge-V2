namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Neutralni metadata navratoveho typu capability.
/// </summary>
public sealed record ReturnMetadata
{
    public required string Type { get; init; }

    public string Description { get; init; } = string.Empty;
}
