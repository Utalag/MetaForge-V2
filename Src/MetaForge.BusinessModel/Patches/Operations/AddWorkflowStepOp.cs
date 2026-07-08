using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Přidá krok do existujícího workflow (immutable).
/// Ověřuje že workflow existuje. Dopočítá Order = počet stávajících kroků.
/// </summary>
public sealed class AddWorkflowStepOp : IPatchOperation
{
    public string CommandType => "AddWorkflowStep";

    public string WorkflowId { get; }
    public string StepId { get; }
    public string StepName { get; }
    public BusinessWorkflowStepKind Kind { get; }

    public AddWorkflowStepOp(string workflowId, string stepName, BusinessWorkflowStepKind kind, string? stepId = null)
    {
        if (string.IsNullOrWhiteSpace(workflowId))
            throw new ArgumentException("WorkflowId cannot be empty.", nameof(workflowId));
        if (string.IsNullOrWhiteSpace(stepName))
            throw new ArgumentException("Step name cannot be empty.", nameof(stepName));

        WorkflowId = workflowId;
        StepId = stepId ?? Guid.NewGuid().ToString("N")[..8];
        StepName = stepName;
        Kind = kind;
    }

    public BusinessAuthoringDocument Apply(BusinessAuthoringDocument document)
    {
        var workflow = document.Workflows.FirstOrDefault(w => w.Id == WorkflowId)
            ?? throw new InvalidOperationException($"Workflow '{WorkflowId}' not found.");

        var step = new BusinessWorkflowStepNode
        {
            Id = StepId,
            Name = StepName,
            Kind = Kind,
            Order = workflow.Steps.Count
        };

        return document with
        {
            Workflows = document.Workflows
                .Select(w => w.Id == WorkflowId
                    ? w with { Steps = w.Steps.Append(step).ToList().AsReadOnly() }
                    : w)
                .ToList()
                .AsReadOnly()
        };
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = WorkflowId,
        TargetAttributeId = StepId,
        Payload = $"{StepName}|{Kind}",
    };
}
