namespace MetaForge.Generators.Packaging;

/// <summary>
/// Závislost na NuGet balíčku.
/// </summary>
public sealed record CodePackageDependency(
    string PackageId,
    string Version,
    string? PackageManager = "NuGet"
);
