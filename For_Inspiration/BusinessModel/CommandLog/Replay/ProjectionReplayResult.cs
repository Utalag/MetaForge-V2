namespace MetaForge.BusinessModel;

/// <summary>
/// Interni datovy prenosovy objekt vysledku ReplayStage.
/// </summary>
internal sealed class ProjectionReplayResult
{
    public required BusinessProjectionReplayResult ReplayResult { get; init; }

    public BusinessProjectionCheckpoint? Checkpoint { get; init; }

    public int TotalCommandCount { get; init; }

    public string CommandLogPath { get; init; } = string.Empty;

    public string? CheckpointPath { get; init; }
}
