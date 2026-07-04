namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Jedna capability (schopnost) ForgeBlocku — co umí poskytnout.
/// </summary>
public sealed record ForgeBlockCapability(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<string>? Tags = null
);
