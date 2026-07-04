namespace MetaForge.Generators.Packaging;

/// <summary>
/// Registr generátorů package manifestů.
/// </summary>
public static class PackageManifestRegistry
{
    private static readonly Lock SyncRoot = new();
    private static readonly List<IPackageManifestGenerator> Generators = new();

    /// <summary>
    /// Zaregistruje generátor package manifestu.
    /// </summary>
    public static void Register(IPackageManifestGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);

        lock (SyncRoot)
        {
            Generators.Add(generator);
        }
    }

    /// <summary>
    /// Vygeneruje soubory manifestu pro dané balíčky ze všech registrovaných generátorů.
    /// </summary>
    public static IReadOnlyCollection<GeneratedArtifactFile> GenerateFiles(IReadOnlyCollection<CodePackageDependency> packages)
    {
        ArgumentNullException.ThrowIfNull(packages);

        List<IPackageManifestGenerator> snapshot;
        lock (SyncRoot)
        {
            snapshot = Generators.ToList();
        }

        var results = new List<GeneratedArtifactFile>();
        foreach (var generator in snapshot)
        {
            results.AddRange(generator.GenerateFiles(packages));
        }

        return results.AsReadOnly();
    }
}
