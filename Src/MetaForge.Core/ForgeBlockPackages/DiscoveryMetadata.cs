namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Federovaná discovery metadata — každý ForgeBlock nese vlastní.
/// </summary>
public sealed record DiscoveryMetadata(
    string DisplayName,
    string Description,
    string? Author = null,
    string? Website = null,
    IReadOnlyList<string>? Tags = null,
    IReadOnlyList<string>? Categories = null
);
