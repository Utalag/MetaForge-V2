using System.Text.Json;

namespace MetaForge.BusinessModel;

public sealed partial class BusinessPatchEngine
{
    private BusinessAuthoringDocument ApplyAddWorkflow(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var name = RequireString(operation, "name", "operations.data.name");
        if (document.Workflows.Any(workflow => string.Equals(workflow.Name, name, StringComparison.OrdinalIgnoreCase)))
            return document;

        var workflow = new BusinessWorkflowNode
        {
            Id = operation.WorkflowId ?? GetString(operation, "id") ?? _idAllocator.CreateWorkflowId(name, document),
            Name = name,
            Summary = GetString(operation, "summary"),
            Trigger = GetString(operation, "trigger"),
            PresetId = GetString(operation, "presetId"),
            Notes = ParseNoteList(operation, "notes"),
        };

        var workflows = document.Workflows.ToList();
        InsertAtOrAppend(workflows, workflow, operation.NewIndex ?? GetInt(operation, "index"));
        return CopyDocument(document, workflows: workflows);
    }

    private BusinessAuthoringDocument ApplyUpdateWorkflow(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var workflow = RequireWorkflow(document, operation.WorkflowId, "operations.workflowId");
        var workflows = document.Workflows.ToList();
        var index = workflows.FindIndex(item => string.Equals(item.Id, workflow.Id, StringComparison.OrdinalIgnoreCase));

        workflows[index] = new BusinessWorkflowNode
        {
            Id = workflow.Id,
            Name = GetString(operation, "name") ?? workflow.Name,
            Summary = HasValue(operation, "summary") ? GetString(operation, "summary") : workflow.Summary,
            Trigger = HasValue(operation, "trigger") ? GetString(operation, "trigger") : workflow.Trigger,
            PresetId = HasValue(operation, "presetId") ? GetString(operation, "presetId") : workflow.PresetId,
            Steps = workflow.Steps,
            Transitions = workflow.Transitions,
            Notes = HasValue(operation, "notes") ? ParseNoteList(operation, "notes") : workflow.Notes,
        };

        return CopyDocument(document, workflows: workflows);
    }

    private BusinessAuthoringDocument ApplyDeleteWorkflow(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.WorkflowId))
            return document;

        var workflows = document.Workflows.Where(workflow => !string.Equals(workflow.Id, operation.WorkflowId, StringComparison.OrdinalIgnoreCase)).ToList();
        if (workflows.Count == document.Workflows.Count)
            return document;

        return CopyDocument(document, workflows: workflows);
    }

    private BusinessAuthoringDocument ApplyAddWorkflowStep(
        BusinessAuthoringDocument document,
        BusinessPatchOperation operation,
        ICollection<PendingQuestionNode> generatedQuestions)
    {
        var workflow = RequireWorkflow(document, operation.WorkflowId, "operations.workflowId");
        var name = RequireString(operation, "name", "operations.data.name");
        if (workflow.Steps.Any(step => string.Equals(step.Name, name, StringComparison.OrdinalIgnoreCase)))
            return document;

        var step = new BusinessWorkflowStepNode
        {
            Id = operation.WorkflowStepId ?? GetString(operation, "id") ?? _idAllocator.CreateWorkflowStepId(name, workflow),
            Name = name,
            Kind = GetEnum<BusinessWorkflowStepKind>(operation, "kind") ?? BusinessWorkflowStepKind.Task,
            Summary = GetString(operation, "summary"),
            RelatedEntityId = GetString(operation, "relatedEntityId"),
            RelatedBehaviorId = GetString(operation, "relatedBehaviorId"),
            Actor = GetString(operation, "actor"),
            Inputs = GetStringList(operation, "inputs"),
            Outputs = GetStringList(operation, "outputs"),
            Notes = ParseNoteList(operation, "notes"),
        };

        if (string.IsNullOrWhiteSpace(operation.WorkflowStepId))
            operation.WorkflowStepId = step.Id;

        MaybeAddWorkflowReferenceQuestions(document, workflow, step, generatedQuestions);

        var steps = workflow.Steps.ToList();
        InsertAtOrAppend(steps, step, operation.NewIndex ?? GetInt(operation, "index"));
        return ReplaceWorkflow(document, workflow, CopyWorkflow(workflow, steps: steps));
    }

    private BusinessAuthoringDocument ApplyUpdateWorkflowStep(
        BusinessAuthoringDocument document,
        BusinessPatchOperation operation,
        ICollection<PendingQuestionNode> generatedQuestions)
    {
        var workflow = RequireWorkflow(document, operation.WorkflowId, "operations.workflowId");
        var step = RequireWorkflowStep(workflow, operation.WorkflowStepId, "operations.workflowStepId");
        var steps = workflow.Steps.ToList();
        var index = steps.FindIndex(item => string.Equals(item.Id, step.Id, StringComparison.OrdinalIgnoreCase));

        var updatedStep = new BusinessWorkflowStepNode
        {
            Id = step.Id,
            Name = GetString(operation, "name") ?? step.Name,
            Kind = GetEnum<BusinessWorkflowStepKind>(operation, "kind") ?? step.Kind,
            Summary = HasValue(operation, "summary") ? GetString(operation, "summary") : step.Summary,
            RelatedEntityId = HasValue(operation, "relatedEntityId") ? GetString(operation, "relatedEntityId") : step.RelatedEntityId,
            RelatedBehaviorId = HasValue(operation, "relatedBehaviorId") ? GetString(operation, "relatedBehaviorId") : step.RelatedBehaviorId,
            Actor = HasValue(operation, "actor") ? GetString(operation, "actor") : step.Actor,
            Inputs = HasValue(operation, "inputs") ? GetStringList(operation, "inputs") : step.Inputs,
            Outputs = HasValue(operation, "outputs") ? GetStringList(operation, "outputs") : step.Outputs,
            BindingDetail = step.BindingDetail,
            Notes = HasValue(operation, "notes") ? ParseNoteList(operation, "notes") : step.Notes,
        };

        MaybeAddWorkflowReferenceQuestions(document, workflow, updatedStep, generatedQuestions);

        steps[index] = updatedStep;
        return ReplaceWorkflow(document, workflow, CopyWorkflow(workflow, steps: steps));
    }

    private BusinessAuthoringDocument ApplyMoveWorkflowStep(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var workflow = RequireWorkflow(document, operation.WorkflowId, "operations.workflowId");
        var step = RequireWorkflowStep(workflow, operation.WorkflowStepId, "operations.workflowStepId");
        var targetIndex = operation.NewIndex ?? GetInt(operation, "index");
        if (!targetIndex.HasValue)
            return document;

        var steps = workflow.Steps.ToList();
        var currentIndex = steps.FindIndex(item => string.Equals(item.Id, step.Id, StringComparison.OrdinalIgnoreCase));
        if (currentIndex < 0 || currentIndex == targetIndex.Value)
            return document;

        steps.RemoveAt(currentIndex);
        InsertAtOrAppend(steps, step, targetIndex.Value);
        return ReplaceWorkflow(document, workflow, CopyWorkflow(workflow, steps: steps));
    }

    private BusinessAuthoringDocument ApplyDeleteWorkflowStep(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.WorkflowId) || string.IsNullOrWhiteSpace(operation.WorkflowStepId))
            return document;

        var workflow = RequireWorkflow(document, operation.WorkflowId, "operations.workflowId");
        var steps = workflow.Steps.Where(step => !string.Equals(step.Id, operation.WorkflowStepId, StringComparison.OrdinalIgnoreCase)).ToList();
        if (steps.Count == workflow.Steps.Count)
            return document;

        var transitions = workflow.Transitions
            .Where(transition => !string.Equals(transition.FromStepId, operation.WorkflowStepId, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(transition.ToStepId, operation.WorkflowStepId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return ReplaceWorkflow(document, workflow, CopyWorkflow(workflow, steps: steps, transitions: transitions));
    }

    private BusinessAuthoringDocument ApplyAddWorkflowTransition(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var workflow = RequireWorkflow(document, operation.WorkflowId, "operations.workflowId");
        var fromStepId = RequireString(operation, "fromStepId", "operations.data.fromStepId");
        var toStepId = RequireString(operation, "toStepId", "operations.data.toStepId");

        var transition = new BusinessWorkflowTransitionNode
        {
            Id = operation.WorkflowTransitionId ?? GetString(operation, "id") ?? _idAllocator.CreateWorkflowTransitionId(fromStepId, toStepId, workflow),
            FromStepId = fromStepId,
            ToStepId = toStepId,
            Label = GetString(operation, "label"),
            Condition = GetString(operation, "condition"),
            IsDefault = GetBool(operation, "isDefault") ?? false,
        };

        var transitions = workflow.Transitions.ToList();
        InsertAtOrAppend(transitions, transition, operation.NewIndex ?? GetInt(operation, "index"));
        return ReplaceWorkflow(document, workflow, CopyWorkflow(workflow, transitions: transitions));
    }

    private BusinessAuthoringDocument ApplyUpdateWorkflowTransition(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var workflow = RequireWorkflow(document, operation.WorkflowId, "operations.workflowId");
        var transition = RequireWorkflowTransition(workflow, operation.WorkflowTransitionId, "operations.workflowTransitionId");
        var transitions = workflow.Transitions.ToList();
        var index = transitions.FindIndex(item => string.Equals(item.Id, transition.Id, StringComparison.OrdinalIgnoreCase));

        transitions[index] = new BusinessWorkflowTransitionNode
        {
            Id = transition.Id,
            FromStepId = GetString(operation, "fromStepId") ?? transition.FromStepId,
            ToStepId = GetString(operation, "toStepId") ?? transition.ToStepId,
            Label = HasValue(operation, "label") ? GetString(operation, "label") : transition.Label,
            Condition = HasValue(operation, "condition") ? GetString(operation, "condition") : transition.Condition,
            IsDefault = GetBool(operation, "isDefault") ?? transition.IsDefault,
        };

        return ReplaceWorkflow(document, workflow, CopyWorkflow(workflow, transitions: transitions));
    }

    private BusinessAuthoringDocument ApplyDeleteWorkflowTransition(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.WorkflowId) || string.IsNullOrWhiteSpace(operation.WorkflowTransitionId))
            return document;

        var workflow = RequireWorkflow(document, operation.WorkflowId, "operations.workflowId");
        var transitions = workflow.Transitions.Where(transition => !string.Equals(transition.Id, operation.WorkflowTransitionId, StringComparison.OrdinalIgnoreCase)).ToList();
        if (transitions.Count == workflow.Transitions.Count)
            return document;

        return ReplaceWorkflow(document, workflow, CopyWorkflow(workflow, transitions: transitions));
    }

    private BusinessAuthoringDocument ApplyBindWorkflowStep(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var workflow = RequireWorkflow(document, operation.WorkflowId, "operations.workflowId");
        var step = RequireWorkflowStep(workflow, operation.WorkflowStepId, "operations.workflowStepId");
        var steps = workflow.Steps.ToList();
        var index = steps.FindIndex(item => string.Equals(item.Id, step.Id, StringComparison.OrdinalIgnoreCase));

        var bindingDetail = new BusinessWorkflowStepBindingDetail
        {
            Source = GetEnum<CoreInfoSource>(operation, "source") ?? CoreInfoSource.Generated,
            CapabilityId = GetString(operation, "capabilityId"),
            ToolHandle = GetString(operation, "toolHandle"),
            BindingKind = GetString(operation, "bindingKind"),
            BindingSummary = GetString(operation, "bindingSummary"),
            LastSyncedAt = DateTimeOffset.UtcNow,
        };

        steps[index] = new BusinessWorkflowStepNode
        {
            Id = step.Id,
            Name = step.Name,
            Kind = step.Kind,
            Summary = step.Summary,
            RelatedEntityId = step.RelatedEntityId,
            RelatedBehaviorId = step.RelatedBehaviorId,
            Actor = step.Actor,
            Inputs = step.Inputs,
            Outputs = step.Outputs,
            BindingDetail = bindingDetail,
            Notes = step.Notes,
        };

        return ReplaceWorkflow(document, workflow, CopyWorkflow(workflow, steps: steps));
    }

    private BusinessAuthoringDocument ApplyUpdateWorkflowBinding(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var workflow = RequireWorkflow(document, operation.WorkflowId, "operations.workflowId");
        var step = RequireWorkflowStep(workflow, operation.WorkflowStepId, "operations.workflowStepId");

        if (step.BindingDetail is null)
        {
            throw new PatchOperationException(
                "patch.workflow_step.binding.missing",
                $"Workflow step {step.Id} nema binding detail k aktualizaci.",
                "operations.workflowStepId");
        }

        var steps = workflow.Steps.ToList();
        var index = steps.FindIndex(item => string.Equals(item.Id, step.Id, StringComparison.OrdinalIgnoreCase));

        var existing = step.BindingDetail;
        var bindingDetail = new BusinessWorkflowStepBindingDetail
        {
            Source = GetEnum<CoreInfoSource>(operation, "source") ?? existing.Source,
            CapabilityId = HasValue(operation, "capabilityId") ? GetString(operation, "capabilityId") : existing.CapabilityId,
            ToolHandle = HasValue(operation, "toolHandle") ? GetString(operation, "toolHandle") : existing.ToolHandle,
            BindingKind = HasValue(operation, "bindingKind") ? GetString(operation, "bindingKind") : existing.BindingKind,
            BindingSummary = HasValue(operation, "bindingSummary") ? GetString(operation, "bindingSummary") : existing.BindingSummary,
            LastSyncedAt = existing.LastSyncedAt,
        };

        steps[index] = new BusinessWorkflowStepNode
        {
            Id = step.Id,
            Name = step.Name,
            Kind = step.Kind,
            Summary = step.Summary,
            RelatedEntityId = step.RelatedEntityId,
            RelatedBehaviorId = step.RelatedBehaviorId,
            Actor = step.Actor,
            Inputs = step.Inputs,
            Outputs = step.Outputs,
            BindingDetail = bindingDetail,
            Notes = step.Notes,
        };

        return ReplaceWorkflow(document, workflow, CopyWorkflow(workflow, steps: steps));
    }
}
