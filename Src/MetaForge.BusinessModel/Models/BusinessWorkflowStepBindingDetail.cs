namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Binding detail — jak je workflow krok navázán na business entitu/behavior.
/// </summary>
public sealed record BusinessWorkflowStepBindingDetail
{
    /// <summary>ID entity, ke které je krok navázán.</summary>
    public string? EntityId { get; init; }

    /// <summary>ID behavioru (metody), ke kterému je krok navázán.</summary>
    public string? BehaviorId { get; init; }

    /// <summary>Název bindingu (např. "ValidateOrder", "SendEmail").</summary>
    public string? BindingName { get; init; }

    /// <summary>Stav synchronizace s Core vrstvou.</summary>
    public WorkflowBindingSyncState SyncState { get; init; } = WorkflowBindingSyncState.New;
}
