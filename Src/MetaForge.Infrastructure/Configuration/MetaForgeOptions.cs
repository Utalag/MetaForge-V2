namespace MetaForge.Infrastructure.Configuration;

/// <summary>
/// Hlavní konfigurační model pro MetaForge platformu.
/// Mapuje se na sekci "MetaForge" v appsettings.json.
/// </summary>
public sealed class MetaForgeOptions
{
    /// <summary>Název konfigurační sekce.</summary>
    public const string SectionName = "MetaForge";

    /// <summary>Konfigurace úložiště.</summary>
    public StorageOptions Storage { get; init; } = new();

    /// <summary>Konfigurace AI.</summary>
    public AiOptions Ai { get; init; } = new();
}
