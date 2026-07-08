namespace MetaForge.Core.Abstractions;

/// <summary>
/// Typ doprovodneho exportniho souboru generovaneho vedle hlavniho zdrojoveho kodu.
/// </summary>
public enum GeneratedArtifactFileKind
{
    Supporting,
    PackageManifest,
    Project,
    Configuration
}

/// <summary>
/// Doporuceny doprovodny soubor pro exportovany artifact.
/// </summary>
public sealed record GeneratedArtifactFile
{
    /// <summary>
    /// Relativni cesta nebo nazev doprovodneho souboru.
    /// </summary>
    public string RelativePath { get; init; } = string.Empty;

    /// <summary>
    /// Obsah doprovodneho souboru.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Kategorie doprovodneho souboru.
    /// </summary>
    public GeneratedArtifactFileKind Kind { get; init; } = GeneratedArtifactFileKind.Supporting;
}