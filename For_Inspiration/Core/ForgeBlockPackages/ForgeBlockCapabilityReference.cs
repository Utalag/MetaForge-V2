namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Typed reference na jinou ForgeBlock capability.
/// </summary>
public sealed record ForgeBlockCapabilityReference
{
    public required string PackageId { get; init; }

    public required string CapabilityId { get; init; }

    public override string ToString() => $"{PackageId}/{CapabilityId}";
}