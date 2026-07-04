namespace MetaForge.BusinessModel;

public sealed class BusinessProjectionView
{
    public bool Success { get; init; }

    public string? StreamId { get; init; }

    public BusinessAuthoringDocument Document { get; init; } = new();

    public IReadOnlyList<BusinessValidationIssue> Issues { get; init; } = [];

    public int TotalCommandCount { get; init; }

    public int ReplayedCommandCount { get; init; }

    public int CheckpointCommandCount { get; init; }

    public bool UsedCheckpoint { get; init; }

    public string CommandLogPath { get; init; } = string.Empty;

    public string? CheckpointPath { get; init; }

    public IReadOnlyDictionary<string, WorkflowBindingSyncState> WorkflowBindingSyncStates { get; init; }
        = new Dictionary<string, WorkflowBindingSyncState>();
}
