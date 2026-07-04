namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Immutable záznam jednoho commandu v CommandLog.
/// APPEND-ONLY — nikdy se nemění, nemaže, nepřepisuje.
/// </summary>
public sealed record CommandEnvelope
{
    /// <summary>Unikátní identifikátor commandu.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Časové razítko vytvoření commandu.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>Typ commandu (např. "AddEntity", "UpdateAttribute", "DeleteEntity").</summary>
    public string CommandType { get; init; } = string.Empty;

    /// <summary>ID entity, které se command týká (pokud relevantní).</summary>
    public string? TargetEntityId { get; init; }

    /// <summary>ID atributu, kterého se command týká (pokud relevantní).</summary>
    public string? TargetAttributeId { get; init; }

    /// <summary>JSON payload s daty commandu.</summary>
    public string Payload { get; init; } = "{}";

    /// <summary>Verze schématu commandu (pro budoucí migrace).</summary>
    public string SchemaVersion { get; init; } = "1.0";
}
