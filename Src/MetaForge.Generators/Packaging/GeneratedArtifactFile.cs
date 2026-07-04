namespace MetaForge.Generators.Packaging;

/// <summary>
/// Dodatečný soubor generovaný jako součást artifactu (např. .props, .csproj).
/// </summary>
public sealed record GeneratedArtifactFile(
    string RelativePath,
    string Content,
    GeneratedArtifactFileKind Kind = GeneratedArtifactFileKind.Source
);

/// <summary>
/// Druh generovaného souboru.
/// </summary>
public enum GeneratedArtifactFileKind
{
    Source,
    PackageManifest,
    ProjectFile,
    SolutionFile
}
