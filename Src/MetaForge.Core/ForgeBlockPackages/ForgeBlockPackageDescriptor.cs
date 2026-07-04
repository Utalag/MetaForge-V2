namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Popisovač ForgeBlock balíku pro registraci.
/// </summary>
public sealed record ForgeBlockPackageDescriptor(
    string Handle,
    string Version,
    string DisplayName,
    string Description
);

/// <summary>
/// Popisovač jedné catalog entry z ForgeBlocku.
/// </summary>
public sealed record ForgeBlockCatalogEntryDescriptor(
    string Name,
    string TypeName,
    string? Description = null,
    IReadOnlyList<string>? Tags = null
);
