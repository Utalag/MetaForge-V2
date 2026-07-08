namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Deskriptor registrovatelneho ForgeBlock package.
/// </summary>
public sealed record ForgeBlockPackageDescriptor
{
    public required string Id { get; init; }

    public string? DistributionPackageId { get; init; }

    public required string DisplayName { get; init; }

    public required string Version { get; init; }

    public string? Description { get; init; }

    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<string> DependsOn { get; init; } = Array.Empty<string>();
}