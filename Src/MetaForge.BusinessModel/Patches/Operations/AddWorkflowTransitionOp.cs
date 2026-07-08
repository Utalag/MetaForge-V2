using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Přidá přechod mezi dvěma kroky workflow (immutable).
/// Ověřuje že workflow i oba kroky existují.
/// </summary>
public sealed class AddWorkflowTransitionOp : IPatchOperation
{
    public string CommandType => "AddWorkflowTransition";

    public string WorkflowId { get; }
    public string TransitionId { get; }
    public string FromStepId { get; }
    public string ToStepId { get; }
    public string? Condition { get; }
    public string? Label { get; }

    public AddWorkflowTransitionOp(
        string workflowId,
        string fromStepId,
        string toStepId,
        string? condition = null,
        string? label = null,
        string? transitionId = null)
    {
        if (string.IsNullOrWhiteSpace(workflowId))
            throw new ArgumentException("WorkflowId cannot be empty.", nameof(workflowId));
        if (string.IsNullOrWhiteSpace(fromStepId))
            throw new ArgumentException("FromStepId cannot be empty.", nameof(fromStepId));
        if (string.IsNullOrWhiteSpace(toStepId))
            throw new ArgumentException("ToStepId cannot be empty.", nameof(toStepId));

        WorkflowId = workflowId;
        TransitionId = transitionId ?? Guid.NewGuid().ToString("N")[..8];
        FromStepId = fromStepId;
        ToStepId = toStepId;
        Condition = condition;
        Label = label;
    }

    public BusinessAuthoringDocument Apply(BusinessAuthoringDocument document)
    {
        var workflow = document.Workflows.FirstOrDefault(w => w.Id == WorkflowId)
            ?? throw new InvalidOperationException($"Workflow '{WorkflowId}' not found.");

        // Ověření že oba kroky existují (pro konzistenci při replayi se tato kontrola přeskočí)
        if (workflow.Steps.All(s => s.Id != FromStepId))
            throw new InvalidOperationException($"Step '{FromStepId}' not found in workflow '{WorkflowId}'.");
        if (workflow.Steps.All(s => s.Id != ToStepId))
            throw new InvalidOperationException($"Step '{ToStepId}' not found in workflow '{WorkflowId}'.");

        var transition = new BusinessWorkflowTransitionNode
        {
            Id = TransitionId,
            FromStepId = FromStepId,
            ToStepId = ToStepId,
            Condition = Condition,
            Label = Label
        };

        return document with
        {
            Workflows = document.Workflows
                .Select(w => w.Id == WorkflowId
                    ? w with { Transitions = w.Transitions.Append(transition).ToList().AsReadOnly() }
                    : w)
                .ToList()
                .AsReadOnly()
        };
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = WorkflowId,
        TargetAttributeId = TransitionId,
        Payload = $"{FromStepId}|{ToStepId}|{Condition}|{Label}",
    };
}
