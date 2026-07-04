namespace MetaForge.BusinessModel;

/// <summary>
/// Provede replay commandu nad checkpointem a vrati <see cref="ProjectionReplayResult"/>.
/// Interni pipeline krok pro <see cref="ReplayProjectionQueryService"/>.
/// </summary>
internal static class ReplayStage
{
    /// <summary>
    /// Urci relevantni checkpoint, aplikuje replay delta a vrati vysledek.
    /// </summary>
    public static ProjectionReplayResult Execute(
        IReadOnlyList<CommandEnvelope> commands,
        string? effectiveStreamId,
        BusinessProjectionReducer reducer,
        BusinessProjectionCheckpointStore? checkpointStore,
        string commandLogPath)
    {
        var checkpoint = ResolveCheckpoint(commands, effectiveStreamId, checkpointStore);
        var replayResult = reducer.Replay(
            commands.Skip(checkpoint?.CommandCount ?? 0).ToArray(),
            checkpoint?.Document);

        return new ProjectionReplayResult
        {
            ReplayResult = replayResult,
            Checkpoint = checkpoint,
            TotalCommandCount = commands.Count,
            CommandLogPath = commandLogPath,
            CheckpointPath = checkpoint is null ? null : checkpointStore?.CheckpointPath,
        };
    }

    private static BusinessProjectionCheckpoint? ResolveCheckpoint(
        IReadOnlyList<CommandEnvelope> commands,
        string? effectiveStreamId,
        BusinessProjectionCheckpointStore? checkpointStore)
    {
        if (checkpointStore is null || commands.Count == 0)
            return null;

        var checkpoint = checkpointStore.TryLoad();
        if (checkpoint is null)
            return null;

        if (!string.IsNullOrWhiteSpace(effectiveStreamId)
            && !string.Equals(checkpoint.StreamId, effectiveStreamId, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return checkpoint.CommandCount > commands.Count
            ? null
            : checkpoint;
    }

    /// <summary>
    /// Streamingova varianta Execute: zpracovava commandy z IAsyncEnumerable v batchich po <paramref name="batchSize"/>
    /// bez nacitani vsech commandu do pameti najednou.
    /// Checkpointy jsou podporeny: commandy pokryte checkpointem se preskoci.
    /// </summary>
    public static async Task<ProjectionReplayResult> ExecuteStreamingAsync(
        IAsyncEnumerable<CommandEnvelope> commandStream,
        string? effectiveStreamId,
        BusinessProjectionReducer reducer,
        BusinessProjectionCheckpointStore? checkpointStore,
        string commandLogPath,
        int batchSize = 1_000,
        CancellationToken cancellationToken = default)
    {
        var checkpoint = LoadCheckpointForStream(checkpointStore, effectiveStreamId);
        var skipCount = checkpoint?.CommandCount ?? 0;
        var currentDoc = checkpoint?.Document;

        var totalSeen = 0;
        var totalApplied = 0;
        var processedBatchCount = 0;
        var batch = new List<CommandEnvelope>(batchSize);

        await foreach (var envelope in commandStream.WithCancellation(cancellationToken))
        {
            totalSeen++;

            if (totalSeen <= skipCount)
                continue;

            batch.Add(envelope);

            if (batch.Count >= batchSize)
            {
                var applyResult = reducer.Replay(batch, currentDoc);
                if (!applyResult.Success)
                {
                    return BuildStreamingFailureResult(applyResult, totalApplied, processedBatchCount, totalSeen, commandLogPath, checkpoint, checkpointStore);
                }

                currentDoc = applyResult.Document;
                totalApplied += batch.Count;
                processedBatchCount++;
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            var applyResult = reducer.Replay(batch, currentDoc);
            if (!applyResult.Success)
            {
                return BuildStreamingFailureResult(applyResult, totalApplied, processedBatchCount, totalSeen, commandLogPath, checkpoint, checkpointStore);
            }

            currentDoc = applyResult.Document;
            totalApplied += batch.Count;
            processedBatchCount++;
        }

        return new ProjectionReplayResult
        {
            ReplayResult = new BusinessProjectionReplayResult
            {
                Success = true,
                Document = currentDoc ?? new BusinessAuthoringDocument(),
                AppliedCommandCount = totalApplied,
                ProcessedBatchCount = processedBatchCount,
                TotalCommandCount = totalSeen,
            },
            Checkpoint = checkpoint,
            TotalCommandCount = totalSeen,
            CommandLogPath = commandLogPath,
            CheckpointPath = checkpoint is null ? null : checkpointStore?.CheckpointPath,
        };
    }

    private static BusinessProjectionCheckpoint? LoadCheckpointForStream(
        BusinessProjectionCheckpointStore? checkpointStore,
        string? effectiveStreamId)
    {
        if (checkpointStore is null)
            return null;

        var checkpoint = checkpointStore.TryLoad();
        if (checkpoint is null)
            return null;

        if (!string.IsNullOrWhiteSpace(effectiveStreamId)
            && !string.Equals(checkpoint.StreamId, effectiveStreamId, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return checkpoint;
    }

    private static ProjectionReplayResult BuildStreamingFailureResult(
        BusinessProjectionReplayResult applyResult,
        int totalApplied,
        int processedBatchCount,
        int totalSeen,
        string commandLogPath,
        BusinessProjectionCheckpoint? checkpoint,
        BusinessProjectionCheckpointStore? checkpointStore)
    {
        return new ProjectionReplayResult
        {
            ReplayResult = new BusinessProjectionReplayResult
            {
                Success = false,
                Document = applyResult.Document,
                Issues = applyResult.Issues,
                AppliedCommandCount = totalApplied,
                ProcessedBatchCount = processedBatchCount,
                TotalCommandCount = totalSeen,
                FailedBatchStartIndex = applyResult.FailedBatchStartIndex,
                FailedBatchCommandCount = applyResult.FailedBatchCommandCount,
                FailedBatchMutationId = applyResult.FailedBatchMutationId,
                FailedBatchCorrelationId = applyResult.FailedBatchCorrelationId,
            },
            Checkpoint = checkpoint,
            TotalCommandCount = totalSeen,
            CommandLogPath = commandLogPath,
            CheckpointPath = checkpoint is null ? null : checkpointStore?.CheckpointPath,
        };
    }
}
