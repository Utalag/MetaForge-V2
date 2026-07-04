namespace MetaForge.Infrastructure.Caching;

/// <summary>
/// Záznam checkpointu — snapshot dokumentu po N commandech.
/// </summary>
public sealed record BusinessProjectionCheckpoint
{
    /// <summary>Po kolika commandech byl checkpoint vytvořen.</summary>
    public int CommandIndex { get; init; }

    /// <summary>Serializovaný dokument (JSON).</summary>
    public string DocumentJson { get; init; } = "{}";

    /// <summary>Časové razítko vytvoření checkpointu.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Verze schématu dokumentu v době checkpointu.</summary>
    public string SchemaVersion { get; init; } = "1.0";
}
