namespace MetaForge.BusinessModel;

public sealed class BusinessProjectionReplayResult
{
    public bool Success { get; init; }

    public BusinessAuthoringDocument Document { get; init; } = new();

    public IReadOnlyList<BusinessValidationIssue> Issues { get; init; } = [];

    public int AppliedCommandCount { get; init; }

    public int ProcessedBatchCount { get; init; }

    public int TotalCommandCount { get; init; }

    public int? FailedBatchStartIndex { get; init; }

    public int FailedBatchCommandCount { get; init; }

    public string? FailedBatchMutationId { get; init; }

    public string? FailedBatchCorrelationId { get; init; }

    /// <summary>
    /// Computed (not stored) sync state per attribute. Klic = "{entityId}/{attributeId}" nebo "{entityId}/~{name}".
    /// Hodnota = AttributeSyncState odvozeny z historie commandu.
    /// </summary>
    public IReadOnlyDictionary<string, AttributeSyncState> AttributeSyncStates { get; init; }
        = new Dictionary<string, AttributeSyncState>();

    /// <summary>
    /// Computed (not stored) sync state per workflow step binding. Klic = "{workflowId}/{stepId}".
    /// Hodnota = WorkflowBindingSyncState odvozeny z historie commandu.
    /// </summary>
    public IReadOnlyDictionary<string, WorkflowBindingSyncState> WorkflowBindingSyncStates { get; init; }
        = new Dictionary<string, WorkflowBindingSyncState>();
}
