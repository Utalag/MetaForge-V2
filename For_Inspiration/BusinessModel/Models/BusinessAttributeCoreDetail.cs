using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

public sealed class BusinessAttributeCoreDetail
{
    public CoreInfoSource Source { get; init; }

    public string? ResolvedPresetId { get; init; }

    public string? ValueObjectName { get; init; }

    public bool IsStrongType { get; init; }

    public DateTimeOffset? LastSyncedAt { get; init; }

    [JsonIgnore]
    public AttributeSyncState SyncState { get; set; } = AttributeSyncState.New;
}
