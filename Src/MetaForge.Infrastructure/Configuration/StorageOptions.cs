namespace MetaForge.Infrastructure.Configuration;

/// <summary>
/// Konfigurace úložiště — cesty k souborům, auto-save.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>Cesta k JSONL souboru s command logem (relativní nebo absolutní).</summary>
    public string CommandLogPath { get; init; } = "data/commands.jsonl";

    /// <summary>Cesta k JSON snapshotu dokumentu.</summary>
    public string DocumentPath { get; init; } = "data/document.json";

    /// <summary>Cesta k adresáři s checkpointy.</summary>
    public string CheckpointPath { get; init; } = "data/checkpoints/";

    /// <summary>Automaticky ukládat dokument po každém commandu?</summary>
    public bool AutoSave { get; init; } = true;

    /// <summary>Interval auto-save v milisekundách (0 = po každém commandu).</summary>
    public int AutoSaveIntervalMs { get; init; } = 5000;
}
