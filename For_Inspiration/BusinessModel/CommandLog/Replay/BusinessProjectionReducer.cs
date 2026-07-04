namespace MetaForge.BusinessModel;

public sealed class BusinessProjectionReducer
{
    private readonly BusinessPatchEngine _patchEngine;
    private readonly CommandEnvelopeToBusinessPatchMapper _commandMapper;

    public BusinessProjectionReducer(
        BusinessPatchEngine? patchEngine = null,
        CommandEnvelopeToBusinessPatchMapper? commandMapper = null)
    {
        _patchEngine = patchEngine ?? new BusinessPatchEngine();
        _commandMapper = commandMapper ?? new CommandEnvelopeToBusinessPatchMapper();
    }

    public BusinessProjectionReplayResult Replay(
        IReadOnlyList<CommandEnvelope> envelopes,
        BusinessAuthoringDocument? seed = null)
    {
        ArgumentNullException.ThrowIfNull(envelopes);

        var currentDocument = seed ?? new BusinessAuthoringDocument();
        var appliedCommandCount = 0;
        var processedBatchCount = 0;
        var batchStartIndex = 0;
        var batchEnvelopes = new List<CommandEnvelope>();

        for (var index = 0; index < envelopes.Count; index++)
        {
            var envelope = envelopes[index];

            if (batchEnvelopes.Count > 0 && !BelongsToSameReplayBatch(batchEnvelopes[^1], envelope))
            {
                var applyResult = ApplyBatch(currentDocument, batchEnvelopes);
                if (!applyResult.Success)
                {
                    return CreateFailureResult(applyResult, currentDocument, appliedCommandCount, processedBatchCount, batchStartIndex, batchEnvelopes, envelopes.Count, envelopes);
                }

                currentDocument = applyResult.Document;
                appliedCommandCount += batchEnvelopes.Count;
                processedBatchCount++;
                batchStartIndex = index;
                batchEnvelopes.Clear();
            }

            batchEnvelopes.Add(envelope);
        }

        if (batchEnvelopes.Count > 0)
        {
            var applyResult = ApplyBatch(currentDocument, batchEnvelopes);
            if (!applyResult.Success)
            {
                return CreateFailureResult(applyResult, currentDocument, appliedCommandCount, processedBatchCount, batchStartIndex, batchEnvelopes, envelopes.Count, envelopes);
            }

            currentDocument = applyResult.Document;
            appliedCommandCount += batchEnvelopes.Count;
            processedBatchCount++;
        }

        return new BusinessProjectionReplayResult
        {
            Success = true,
            Document = currentDocument,
            AppliedCommandCount = appliedCommandCount,
            ProcessedBatchCount = processedBatchCount,
            TotalCommandCount = envelopes.Count,
            AttributeSyncStates = ComputeAttributeSyncStates(envelopes),
            WorkflowBindingSyncStates = ComputeWorkflowBindingSyncStates(envelopes),
        };
    }

    private BusinessPatchApplyResult ApplyBatch(BusinessAuthoringDocument currentDocument, IReadOnlyList<CommandEnvelope> batchEnvelopes)
    {
        var operations = batchEnvelopes.Select(_commandMapper.Map).ToArray();
        return _patchEngine.Apply(currentDocument, operations);
    }

    private static bool BelongsToSameReplayBatch(CommandEnvelope left, CommandEnvelope right)
    {
        if (!string.IsNullOrWhiteSpace(left.MutationId) || !string.IsNullOrWhiteSpace(right.MutationId))
        {
            return !string.IsNullOrWhiteSpace(left.MutationId)
                && !string.IsNullOrWhiteSpace(right.MutationId)
                && string.Equals(left.MutationId, right.MutationId, StringComparison.OrdinalIgnoreCase);
        }

        return !string.IsNullOrWhiteSpace(left.CorrelationId)
            && !string.IsNullOrWhiteSpace(right.CorrelationId)
            && string.Equals(left.CorrelationId, right.CorrelationId, StringComparison.OrdinalIgnoreCase)
            && left.Source == right.Source
            && string.Equals(left.CausationId, right.CausationId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.IssuedBy.ActorType, right.IssuedBy.ActorType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.IssuedBy.ActorId, right.IssuedBy.ActorId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.IssuedBy.DisplayName, right.IssuedBy.DisplayName, StringComparison.OrdinalIgnoreCase);
    }

    private static BusinessProjectionReplayResult CreateFailureResult(
        BusinessPatchApplyResult applyResult,
        BusinessAuthoringDocument fallbackDocument,
        int appliedCommandCount,
        int processedBatchCount,
        int batchStartIndex,
        IReadOnlyList<CommandEnvelope> batchEnvelopes,
        int totalCommandCount,
        IReadOnlyList<CommandEnvelope> allEnvelopes)
    {
        var firstEnvelope = batchEnvelopes[0];

        return new BusinessProjectionReplayResult
        {
            Success = false,
            Document = applyResult.Document ?? fallbackDocument,
            Issues = applyResult.Issues,
            AppliedCommandCount = appliedCommandCount,
            ProcessedBatchCount = processedBatchCount,
            TotalCommandCount = totalCommandCount,
            FailedBatchStartIndex = batchStartIndex,
            FailedBatchCommandCount = batchEnvelopes.Count,
            FailedBatchMutationId = firstEnvelope.MutationId,
            FailedBatchCorrelationId = firstEnvelope.CorrelationId,
            AttributeSyncStates = ComputeAttributeSyncStates(allEnvelopes),
            WorkflowBindingSyncStates = ComputeWorkflowBindingSyncStates(allEnvelopes),
        };
    }

    /// <summary>
    /// Odvozi AttributeSyncState z historie commandu: pro kazdy atribut (klic entityId/attributeId)
    /// vraci stav synchronizace CoreDetail.
    /// Pravidla: add_attribute → New, apply_preset/enrich_attribute → Synced,
    /// update_attribute (business metadata) po Synced → BusinessEdited,
    /// update_core_detail → Synced.
    /// Pro attribute.remove je zaznam odstranen.
    /// </summary>
    private static IReadOnlyDictionary<string, AttributeSyncState> ComputeAttributeSyncStates(
        IReadOnlyList<CommandEnvelope> envelopes)
    {
        var states = new Dictionary<string, AttributeSyncState>(StringComparer.OrdinalIgnoreCase);

        foreach (var envelope in envelopes)
        {
            var kind = envelope.Kind;
            if (!IsAttributeRelevantKind(kind))
                continue;

            var entityId = GetPayloadString(envelope, "entityId");
            if (string.IsNullOrWhiteSpace(entityId))
                continue;

            var attributeId = GetPayloadString(envelope, "attributeId");
            var key = string.IsNullOrWhiteSpace(attributeId)
                ? $"{entityId}/~{GetPayloadString(envelope, "name")}"
                : $"{entityId}/{attributeId}";

            if (IsRemoveAttributeKind(kind))
            {
                states.Remove(key);
                continue;
            }

            if (IsEnrichmentKind(kind))
            {
                // preset.apply / attribute.enrich → always Synced (resolves any conflict)
                states[key] = AttributeSyncState.Synced;
            }
            else if (IsCoreDetailUpdateKind(kind))
            {
                // Synced → CoreEdited, BusinessEdited → Conflict, New/unset → Synced
                var current = states.GetValueOrDefault(key, AttributeSyncState.New);
                states[key] = current switch
                {
                    AttributeSyncState.Synced => AttributeSyncState.CoreEdited,
                    AttributeSyncState.BusinessEdited => AttributeSyncState.Conflict,
                    _ => AttributeSyncState.Synced,
                };
            }
            else if (IsBusinessMetadataUpdateKind(kind))
            {
                // Synced → BusinessEdited, CoreEdited → Conflict, others unchanged / New if absent
                if (states.TryGetValue(key, out var current))
                {
                    states[key] = current switch
                    {
                        AttributeSyncState.Synced => AttributeSyncState.BusinessEdited,
                        AttributeSyncState.CoreEdited => AttributeSyncState.Conflict,
                        _ => current,
                    };
                }
                else
                {
                    states[key] = AttributeSyncState.New;
                }
            }
            else
            {
                // attribute.add or other attribute commands
                states.TryAdd(key, AttributeSyncState.New);
            }
        }

        return states;
    }

    private static bool IsAttributeRelevantKind(string kind)
        => kind.StartsWith("attribute.", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("preset.apply", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("attribute.enrich", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("coredetail.update", StringComparison.OrdinalIgnoreCase);

    private static bool IsRemoveAttributeKind(string kind)
        => kind.Equals("attribute.remove", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("attribute.delete", StringComparison.OrdinalIgnoreCase);

    private static bool IsEnrichmentKind(string kind)
        => kind.Equals("preset.apply", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("attribute.enrich", StringComparison.OrdinalIgnoreCase);

    private static bool IsCoreDetailUpdateKind(string kind)
        => kind.Equals("coredetail.update", StringComparison.OrdinalIgnoreCase);

    private static bool IsBusinessMetadataUpdateKind(string kind)
        => kind.Equals("attribute.update", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Odvozi WorkflowBindingSyncState z historie commandu: pro kazdy workflow step (klic workflowId/stepId)
    /// vraci stav synchronizace binding detailu.
    /// </summary>
    private static IReadOnlyDictionary<string, WorkflowBindingSyncState> ComputeWorkflowBindingSyncStates(
        IReadOnlyList<CommandEnvelope> envelopes)
    {
        var states = new Dictionary<string, WorkflowBindingSyncState>(StringComparer.OrdinalIgnoreCase);

        foreach (var envelope in envelopes)
        {
            var kind = envelope.Kind;
            if (!IsWorkflowStepRelevantKind(kind))
                continue;

            var workflowId = GetPayloadString(envelope, "workflowId");

            if (kind.Equals("workflow.delete", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(workflowId))
                {
                    var prefix = $"{workflowId}/";
                    foreach (var stateKey in states.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())
                        states.Remove(stateKey);
                }
                continue;
            }

            var stepId = GetPayloadString(envelope, "workflowStepId");
            if (string.IsNullOrWhiteSpace(workflowId) || string.IsNullOrWhiteSpace(stepId))
                continue;

            var key = $"{workflowId}/{stepId}";

            if (IsRemoveWorkflowStepKind(kind))
            {
                states.Remove(key);
                continue;
            }

            if (IsBindWorkflowStepKind(kind))
            {
                // bind_workflow_step → always Synced (resolves any conflict)
                states[key] = WorkflowBindingSyncState.Synced;
            }
            else if (IsUpdateWorkflowBindingKind(kind))
            {
                var current = states.GetValueOrDefault(key, WorkflowBindingSyncState.New);
                states[key] = current switch
                {
                    WorkflowBindingSyncState.Synced => WorkflowBindingSyncState.BindingEdited,
                    WorkflowBindingSyncState.BusinessEdited => WorkflowBindingSyncState.Conflict,
                    _ => current,
                };
            }
            else if (IsUpdateWorkflowStepKind(kind))
            {
                // Binding-relevant business edit only if payload touches kind, relatedEntityId, relatedBehaviorId, actor, inputs, outputs
                if (!HasPayloadBindingRelevantKey(envelope))
                    continue;

                var current = states.GetValueOrDefault(key, WorkflowBindingSyncState.New);
                states[key] = current switch
                {
                    WorkflowBindingSyncState.Synced => WorkflowBindingSyncState.BusinessEdited,
                    WorkflowBindingSyncState.BindingEdited => WorkflowBindingSyncState.Conflict,
                    _ => current,
                };
            }
            else
            {
                // workflow_step.add or other workflow step commands
                states.TryAdd(key, WorkflowBindingSyncState.New);
            }
        }

        return states;
    }

    private static bool IsWorkflowStepRelevantKind(string kind)
        => kind.StartsWith("workflow_step.", StringComparison.OrdinalIgnoreCase)
            || kind.StartsWith("workflow_binding.", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("workflow.delete", StringComparison.OrdinalIgnoreCase);

    private static bool IsRemoveWorkflowStepKind(string kind)
        => kind.Equals("workflow_step.remove", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("workflow_step.delete", StringComparison.OrdinalIgnoreCase);

    private static bool IsBindWorkflowStepKind(string kind)
        => kind.Equals("workflow_step.bind", StringComparison.OrdinalIgnoreCase);

    private static bool IsUpdateWorkflowBindingKind(string kind)
        => kind.Equals("workflow_binding.update", StringComparison.OrdinalIgnoreCase);

    private static bool IsUpdateWorkflowStepKind(string kind)
        => kind.Equals("workflow_step.update", StringComparison.OrdinalIgnoreCase);

    private static bool HasPayloadBindingRelevantKey(CommandEnvelope envelope)
    {
        var relevantKeys = new[] { "kind", "relatedEntityId", "relatedBehaviorId", "actor", "inputs", "outputs" };
        foreach (var pair in envelope.Payload)
        {
            foreach (var key in relevantKeys)
            {
                if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }

    private static string? GetPayloadString(CommandEnvelope envelope, string key)
    {
        foreach (var pair in envelope.Payload)
        {
            if (!string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                continue;

            if (pair.Value is null)
                return null;

            if (pair.Value is System.Text.Json.Nodes.JsonValue jsonValue)
            {
                if (jsonValue.TryGetValue<string>(out var text))
                    return text;
                if (jsonValue.TryGetValue<int>(out var intValue))
                    return intValue.ToString();
                if (jsonValue.TryGetValue<long>(out var longValue))
                    return longValue.ToString();
            }

            return pair.Value.ToJsonString();
        }

        return null;
    }
}