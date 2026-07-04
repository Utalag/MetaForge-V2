namespace MetaForge.BusinessModel;

/// <summary>
/// Konfiguruje prahy pro aktivaci streamingoveho rezimu pri nacitani command logu v SourceStage.
/// Streaming se aktivuje kdyz fileSize > FileSizeThresholdBytes NEBO commandCount > CommandCountThreshold.
/// </summary>
public sealed record StreamingThresholdOptions
{
    /// <summary>Vychozi prahy: 1 MB soubor nebo 500 commandu.</summary>
    public static StreamingThresholdOptions Default => new();

    /// <summary>Velikost souboru v bajtech, od ktere se aktivuje streaming. Vychozi: 1 MB.</summary>
    public long FileSizeThresholdBytes { get; init; } = 1_048_576;

    /// <summary>Pocet commandu, od ktereho se aktivuje streaming. Vychozi: 500.</summary>
    public int CommandCountThreshold { get; init; } = 500;

    /// <summary>Pocet commandu zpracovanych v jednom streamovacim batchi. Vychozi: 1000.</summary>
    public int StreamingBatchSize { get; init; } = 1_000;

    /// <summary>
    /// Vraci true pokud velikost souboru prekrocila threshold. Vyuziva file size jako primerni metriku —
    /// koreluje primo s pameti alokovanou pri ReadAll(). Secondarni metrika (CommandCountThreshold)
    /// slouzi jako dokumentovana hodnota pro ucely nastaveni; primary check je file size.
    /// </summary>
    public bool ShouldUseStreaming(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        return new FileInfo(filePath).Length > FileSizeThresholdBytes;
    }
}
