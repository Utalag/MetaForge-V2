using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Přidá nové workflow do dokumentu (immutable).
/// </summary>
public sealed class AddWorkflowOp : IPatchOperation
{
    public string CommandType => "AddWorkflow";

    public string WorkflowId { get; }
    public string Name { get; }
    public string? Description { get; }

    public AddWorkflowOp(string name, string? description = null, string? workflowId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workflow name cannot be empty.", nameof(name));

        WorkflowId = workflowId ?? Guid.NewGuid().ToString("N")[..8];
        Name = name;
        Description = description;
    }

    public BusinessAuthoringDocument Apply(BusinessAuthoringDocument document)
    {
        var workflow = new BusinessWorkflowNode
        {
            Id = WorkflowId,
            Name = Name,
            Description = Description
        };

        return document with
        {
            Workflows = document.Workflows.Append(workflow).ToList().AsReadOnly()
        };
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = WorkflowId,
        Payload = Name,
    };
}
