namespace MetaForge.Core.Common;

/// <summary>
/// Jazykově-neutrální deklarace externí balíčkové závislosti potřebné pro vygenerovaný kód.
/// </summary>
public sealed record CodePackageDependency
{
    /// <summary>
    /// Identifikátor balíčku v cílovém package manageru.
    /// </summary>
    public required string PackageId { get; init; }

    /// <summary>
    /// Doporučená nebo minimální verze balíčku.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Název package manageru, např. NuGet, npm, pip, Maven nebo GoModule.
    /// </summary>
    public string PackageManager { get; init; } = string.Empty;
}