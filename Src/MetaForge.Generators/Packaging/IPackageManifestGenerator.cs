namespace MetaForge.Generators.Packaging;

/// <summary>
/// Generátor package manifestu pro konkrétní package manager.
/// </summary>
public interface IPackageManifestGenerator
{
    /// <summary>
    /// Vygeneruje soubory manifestu pro dané balíčky.
    /// </summary>
    IReadOnlyCollection<GeneratedArtifactFile> GenerateFiles(IReadOnlyCollection<CodePackageDependency> packages);
}
