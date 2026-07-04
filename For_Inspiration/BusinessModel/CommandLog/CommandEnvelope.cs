using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

public sealed class CommandEnvelope
{
    public const string CurrentSchemaVersion = "command-envelope/v1";

    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = CurrentSchemaVersion;

    [JsonPropertyName("streamId")]
    public string StreamId { get; init; } = string.Empty;

    [JsonPropertyName("expectedVersion")]
    public long? ExpectedVersion { get; init; }

    [JsonPropertyName("issuedAt")]
    public DateTimeOffset IssuedAt { get; init; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("issuedBy")]
    public CommandIssuedBy IssuedBy { get; init; } = new();

    [JsonPropertyName("source")]
    public CommandSource Source { get; init; } = CommandSource.Unknown;

    /// <summary>
    /// Puvod informace v commandu — rucne zadana (Manual), vygenerovana AI (Generated) nebo kombinace (Hybrid).
    /// Vychozi hodnota Manual zajistuje zpetnou kompatibilitu se starsimi JSONL logy bez tohoto pole.
    /// </summary>
    [JsonPropertyName("infoSource")]
    public CoreInfoSource InfoSource { get; init; } = CoreInfoSource.Manual;

    [JsonPropertyName("kind")]
    public string Kind { get; init; } = string.Empty;

    [JsonPropertyName("payload")]
    public JsonObject Payload { get; init; } = new();

    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; init; }

    [JsonPropertyName("causationId")]
    public string? CausationId { get; init; }

    [JsonPropertyName("mutationId")]
    public string? MutationId { get; init; }

    [JsonPropertyName("provenance")]
    public CommandProvenance Provenance { get; init; } = new();
}