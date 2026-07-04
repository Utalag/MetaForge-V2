using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

public sealed class BusinessWorkflowStepBindingDetail
{
    public CoreInfoSource Source { get; init; }

    public string? CapabilityId { get; init; }

    public string? ToolHandle { get; init; }

    public string? BindingKind { get; init; }

    public string? BindingSummary { get; init; }

    public DateTimeOffset? LastSyncedAt { get; init; }

    [JsonIgnore]
    public WorkflowBindingSyncState SyncState { get; set; } = WorkflowBindingSyncState.New;
}