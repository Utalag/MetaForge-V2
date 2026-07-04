using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

public sealed class CommandProvenance
{
    [JsonPropertyName("mode")]
    public string Mode { get; init; } = "direct";

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    [JsonPropertyName("producer")]
    public string? Producer { get; init; }

    [JsonPropertyName("provider")]
    public string? Provider { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("promptVersion")]
    public string? PromptVersion { get; init; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; init; }
}