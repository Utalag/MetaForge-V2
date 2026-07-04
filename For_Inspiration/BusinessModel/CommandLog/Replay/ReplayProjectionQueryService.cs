namespace MetaForge.BusinessModel;

public sealed class ReplayProjectionQueryService : IProjectionQueryService
{
    private readonly JsonlShadowCommandReader _commandReader;
    private readonly BusinessProjectionReducer _projectionReducer;
    private readonly BusinessProjectionCheckpointStore? _checkpointStore;
    private readonly StreamingThresholdOptions _streamingThreshold;

    public ReplayProjectionQueryService(
        JsonlShadowCommandReader? commandReader = null,
        BusinessProjectionReducer? projectionReducer = null,
        BusinessProjectionCheckpointStore? checkpointStore = null,
        StreamingThresholdOptions? streamingThreshold = null)
    {
        _commandReader = commandReader ?? new JsonlShadowCommandReader();
        _projectionReducer = projectionReducer ?? new BusinessProjectionReducer();
        _checkpointStore = checkpointStore;
        _streamingThreshold = streamingThreshold ?? StreamingThresholdOptions.Default;
    }

    public async Task<BusinessProjectionView> GetProjectionAsync(string? streamId = null, CancellationToken cancellationToken = default)
    {
        ProjectionReplayResult result;

        if (SourceStage.ShouldUseStreaming(_commandReader.FilePath, _streamingThreshold))
        {
            var effectiveStreamId = await SourceStage.ResolveStreamIdAsync(
                _commandReader.FilePath, streamId, cancellationToken);

            var commandStream = SourceStage.StreamAsync(_commandReader, effectiveStreamId, cancellationToken);
            result = await ReplayStage.ExecuteStreamingAsync(
                commandStream,
                effectiveStreamId,
                _projectionReducer,
                _checkpointStore,
                _commandReader.FilePath,
                _streamingThreshold.StreamingBatchSize,
                cancellationToken);

            return BuildView(result, effectiveStreamId);
        }

        var (commands, syncEffectiveStreamId) = SourceStage.Load(_commandReader, streamId);
        result = ReplayStage.Execute(commands, syncEffectiveStreamId, _projectionReducer, _checkpointStore, _commandReader.FilePath);
        return BuildView(result, syncEffectiveStreamId);
    }

    private static BusinessProjectionView BuildView(ProjectionReplayResult result, string? effectiveStreamId)
        => new()
        {
            Success = result.ReplayResult.Success,
            StreamId = effectiveStreamId,
            Document = result.ReplayResult.Document,
            Issues = result.ReplayResult.Issues,
            TotalCommandCount = result.TotalCommandCount,
            ReplayedCommandCount = result.ReplayResult.AppliedCommandCount,
            CheckpointCommandCount = result.Checkpoint?.CommandCount ?? 0,
            UsedCheckpoint = result.Checkpoint is not null,
            CommandLogPath = result.CommandLogPath,
            CheckpointPath = result.CheckpointPath,
            WorkflowBindingSyncStates = result.ReplayResult.WorkflowBindingSyncStates,
        };

    public BusinessProjectionCheckpoint SaveCheckpoint(string? streamId = null)
    {
        if (_checkpointStore is null)
            throw new InvalidOperationException("Projection checkpoint store neni nakonfigurovany.");

        var projection = GetProjectionAsync(streamId).GetAwaiter().GetResult();
        if (!projection.Success)
            throw new InvalidOperationException("Projection checkpoint nelze ulozit, protoze replay business projekce selhal.");

        var checkpoint = new BusinessProjectionCheckpoint
        {
            StreamId = projection.StreamId ?? string.Empty,
            CommandCount = projection.TotalCommandCount,
            SavedAt = DateTimeOffset.UtcNow,
            Document = projection.Document,
        };

        return _checkpointStore.Save(checkpoint);
    }

}
