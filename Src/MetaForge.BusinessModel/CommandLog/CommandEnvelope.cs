using MetaForge.BusinessModel.Models;

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
    public string SchemaVersion { get; init; } = BusinessAuthoringDocument.CurrentSchemaVersion;

    // --- Nová pole dle PROP-020 ---

    /// <summary>Identifikátor streamu — pro multi-tenant / multi-project log.</summary>
    public string StreamId { get; init; } = "default";

    /// <summary>Odkud command přišel (CLI, MCP, Chat, ...).</summary>
    public CommandSource Source { get; init; } = CommandSource.Unknown;

    /// <summary>Původ informace — ruční, generovaná, hybridní.</summary>
    public CoreInfoSource InfoSource { get; init; } = CoreInfoSource.Manual;

    /// <summary>Kdo command vydal.</summary>
    public CommandIssuedBy IssuedBy { get; init; } = new();

    /// <summary>Provenience — metadata o vzniku commandu (model, confidence, ...).</summary>
    public CommandProvenance Provenance { get; init; } = new();

    /// <summary>Korelační ID — pro sledování souvisejících commandů napříč službami.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Kauzační ID — ID commandu, který tento command vyvolal.</summary>
    public string? CausationId { get; init; }

    /// <summary>
    /// ID mutace pro idempotenci.
    /// Pokud je nastaveno, CommandLogStore ignoruje duplicitní commandy se stejným MutationId.
    /// null = idempotence se nekontroluje (zpětná kompatibilita).
    /// </summary>
    public string? MutationId { get; init; }
}
