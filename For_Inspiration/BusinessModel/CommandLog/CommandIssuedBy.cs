using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

public sealed class CommandIssuedBy
{
    [JsonPropertyName("actorType")]
    public string ActorType { get; init; } = "system";

    [JsonPropertyName("actorId")]
    public string? ActorId { get; init; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }
}