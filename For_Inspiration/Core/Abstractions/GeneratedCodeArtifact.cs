using MetaForge.Core.Common;

namespace MetaForge.Core.Abstractions;

/// <summary>
/// Výsledek generování jednoho jazykového artefaktu včetně importů a balíčkových závislostí.
/// </summary>
public sealed record GeneratedCodeArtifact
{
    /// <summary>
    /// Vygenerovaný zdrojový kód.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Importy nebo using direktivy potřebné pro vygenerovaný artefakt.
    /// </summary>
    public IReadOnlyCollection<string> RequiredImports { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Externí balíčky potřebné pro úspěšný build artefaktu.
    /// </summary>
    public IReadOnlyCollection<CodePackageDependency> RequiredPackages { get; init; } = Array.Empty<CodePackageDependency>();

    /// <summary>
    /// Doporucene doprovodne soubory jako package manifesty nebo projektove fragmenty.
    /// </summary>
    public IReadOnlyCollection<GeneratedArtifactFile> AdditionalFiles { get; init; } = Array.Empty<GeneratedArtifactFile>();
}