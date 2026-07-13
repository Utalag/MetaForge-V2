namespace MetaForge.Generators;

/// <summary>
/// NuGet balíček vyžadovaný vygenerovaným kódem.
/// </summary>
public sealed record CodePackageDependency(
    string PackageId,
    string Version
);

/// <summary>
/// Výstup generátoru — vygenerovaný soubor s kódem.
/// PROP-017: Rozšířeno o RequiredPackages a RequiredUsings pro ForgeBlock integraci.
/// </summary>
public sealed record GeneratedCodeArtifact(
    string FileName,
    string SourceCode,
    IReadOnlyList<DiagnosticInfo>? Diagnostics = null,
    IReadOnlyList<CodePackageDependency>? RequiredPackages = null,
    IReadOnlyList<string>? RequiredUsings = null
);
