using System.Globalization;
using MetaForge.BusinessModel;

namespace MetaForge.Translator;

/// <summary>
/// Projekcni pohled na workflow sekci <see cref="BusinessAuthoringDocument.Workflows"/>.
/// </summary>
public sealed class WorkflowProjectionView
{
    /// <summary>Seznam workflow definic v dokumentu.</summary>
    public IReadOnlyList<WorkflowProjection> Workflows { get; init; } = [];

    /// <summary>Celkovy pocet kroku napric vsemi workflow.</summary>
    public int TotalSteps { get; init; }

    /// <summary>Celkovy pocet prechodu napric vsemi workflow.</summary>
    public int TotalTransitions { get; init; }
}

/// <summary>
/// Projekce jedne workflow definice.
/// </summary>
public sealed class WorkflowProjection
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Trigger { get; init; }

    public IReadOnlyList<WorkflowStepProjection> Steps { get; init; } = [];

    public IReadOnlyList<WorkflowTransitionProjection> Transitions { get; init; } = [];
}

/// <summary>
/// Projekce jednoho workflow kroku.
/// </summary>
public sealed class WorkflowStepProjection
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Kind { get; init; } = string.Empty;

    public string? RelatedEntityId { get; init; }

    public string? RelatedEntityName { get; init; }

    public string? RelatedBehaviorId { get; init; }

    public string? Actor { get; init; }

    public bool HasBindingDetail { get; init; }

    public string? BindingSummary { get; init; }

    /// <summary>
    /// Computed sync stav workflow bindingu — doplni se v WWB1-S4.
    /// </summary>
    public string? SyncState { get; init; }
}

/// <summary>
/// Projekce prechodu mezi workflow kroky.
/// </summary>
public sealed class WorkflowTransitionProjection
{
    public string Id { get; init; } = string.Empty;

    public string FromStepId { get; init; } = string.Empty;

    public string ToStepId { get; init; } = string.Empty;

    public string? Label { get; init; }

    public bool IsDefault { get; init; }
}

/// <summary>
/// Builder pro <see cref="WorkflowProjectionView"/> z <see cref="BusinessAuthoringDocument"/>.
/// </summary>
internal static class WorkflowProjectionBuilder
{
    public static WorkflowProjectionView Build(
        BusinessAuthoringDocument document,
        IReadOnlyDictionary<string, WorkflowBindingSyncState>? syncStates = null)
    {
        var entityNameMap = document.Entities.ToDictionary(
            e => e.Id,
            e => e.Name,
            StringComparer.OrdinalIgnoreCase);

        var projections = document.Workflows
            .Select(w => BuildWorkflow(w, entityNameMap, syncStates))
            .ToArray();

        return new WorkflowProjectionView
        {
            Workflows = projections,
            TotalSteps = projections.Sum(w => w.Steps.Count),
            TotalTransitions = projections.Sum(w => w.Transitions.Count),
        };
    }

    private static WorkflowProjection BuildWorkflow(
        BusinessWorkflowNode workflow,
        Dictionary<string, string> entityNameMap,
        IReadOnlyDictionary<string, WorkflowBindingSyncState>? syncStates)
    {
        return new WorkflowProjection
        {
            Id = workflow.Id,
            Name = workflow.Name,
            Trigger = workflow.Trigger,
            Steps = workflow.Steps
                .Select(s => BuildStep(workflow.Id, s, entityNameMap, syncStates))
                .ToArray(),
            Transitions = workflow.Transitions
                .Select(t => new WorkflowTransitionProjection
                {
                    Id = t.Id,
                    FromStepId = t.FromStepId,
                    ToStepId = t.ToStepId,
                    Label = t.Label,
                    IsDefault = t.IsDefault,
                })
                .ToArray(),
        };
    }

    private static WorkflowStepProjection BuildStep(
        string workflowId,
        BusinessWorkflowStepNode step,
        Dictionary<string, string> entityNameMap,
        IReadOnlyDictionary<string, WorkflowBindingSyncState>? syncStates)
    {
        var key = $"{workflowId}/{step.Id}";
        var syncState = syncStates?.TryGetValue(key, out var state) == true
            ? state.ToString()
            : null;

        return new WorkflowStepProjection
        {
            Id = step.Id,
            Name = step.Name,
            Kind = step.Kind.ToString(),
            RelatedEntityId = step.RelatedEntityId,
            RelatedEntityName = step.RelatedEntityId is not null && entityNameMap.TryGetValue(step.RelatedEntityId, out var name)
                ? name
                : null,
            RelatedBehaviorId = step.RelatedBehaviorId,
            Actor = step.Actor,
            HasBindingDetail = step.BindingDetail is not null,
            BindingSummary = step.BindingDetail?.BindingSummary,
            SyncState = syncState,
        };
    }
}
