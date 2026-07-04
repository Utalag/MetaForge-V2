using System.Text.Json;

namespace MetaForge.BusinessModel;

public sealed partial class BusinessPatchEngine
{
    private BusinessAuthoringDocument ReplaceEntity(BusinessAuthoringDocument document, BusinessEntityNode current, BusinessEntityNode updated)
    {
        var entities = document.Entities.ToList();
        var index = entities.FindIndex(item => string.Equals(item.Id, current.Id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            throw new PatchOperationException("patch.entity.missing", $"Entita {current.Id} nebyla nalezena.", "operations.entityId");

        entities[index] = updated;
        return CopyDocument(document, entities: entities);
    }

    private BusinessAuthoringDocument ReplaceRelation(BusinessAuthoringDocument document, BusinessRelationNode current, BusinessRelationNode updated)
    {
        var relations = document.Relations.ToList();
        var index = relations.FindIndex(item => string.Equals(item.Id, current.Id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            throw new PatchOperationException("patch.relation.missing", $"Relation {current.Id} nebyla nalezena.", "operations.relationId");

        relations[index] = updated;
        return CopyDocument(document, relations: relations);
    }

    private BusinessAuthoringDocument ReplaceWorkflow(BusinessAuthoringDocument document, BusinessWorkflowNode current, BusinessWorkflowNode updated)
    {
        var workflows = document.Workflows.ToList();
        var index = workflows.FindIndex(item => string.Equals(item.Id, current.Id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            throw new PatchOperationException("patch.workflow.missing", $"Workflow {current.Id} nebylo nalezeno.", "operations.workflowId");

        workflows[index] = updated;
        return CopyDocument(document, workflows: workflows);
    }

    private static BusinessAuthoringDocument CloneDocument(BusinessAuthoringDocument document)
    {
        return BusinessDocumentJsonSerializer.Parse(BusinessDocumentJsonSerializer.Serialize(document));
    }

    private static BusinessAuthoringDocument CopyDocument(
        BusinessAuthoringDocument source,
        BusinessProjectInfo? project = null,
        IReadOnlyList<BusinessEntityNode>? entities = null,
        IReadOnlyList<BusinessRelationNode>? relations = null,
        IReadOnlyList<BusinessWorkflowNode>? workflows = null,
        IReadOnlyList<BusinessNoteNode>? notes = null,
        IReadOnlyList<PendingQuestionNode>? pendingQuestions = null,
        IReadOnlyList<CustomTypeDefinition>? customTypes = null)
    {
        return new BusinessAuthoringDocument
        {
            SchemaVersion = source.SchemaVersion,
            Project = project ?? source.Project,
            Entities = entities ?? source.Entities,
            Relations = relations ?? source.Relations,
            Workflows = workflows ?? source.Workflows,
            Notes = notes ?? source.Notes,
            PendingQuestions = pendingQuestions ?? source.PendingQuestions,
            CustomTypes = customTypes ?? source.CustomTypes,
        };
    }

    private static BusinessEntityNode CopyEntity(
        BusinessEntityNode source,
        string? name = null,
        string? summary = null,
        string? icon = null,
        string? presetId = null,
        IReadOnlyList<BusinessAttributeNode>? attributes = null,
        IReadOnlyList<BusinessBehaviorNode>? behaviors = null,
        IReadOnlyList<BusinessNoteNode>? notes = null)
    {
        return new BusinessEntityNode
        {
            Id = source.Id,
            Name = name ?? source.Name,
            Summary = summary ?? source.Summary,
            Icon = icon ?? source.Icon,
            PresetId = presetId ?? source.PresetId,
            Attributes = attributes ?? source.Attributes,
            Behaviors = behaviors ?? source.Behaviors,
            Notes = notes ?? source.Notes,
        };
    }

    private static BusinessBehaviorNode CopyBehavior(
        BusinessBehaviorNode source,
        IReadOnlyList<BusinessNoteNode>? notes = null)
    {
        return new BusinessBehaviorNode
        {
            Id = source.Id,
            Name = source.Name,
            Kind = source.Kind,
            Summary = source.Summary,
            Inputs = source.Inputs,
            Returns = source.Returns,
            Notes = notes ?? source.Notes,
        };
    }

    private static BusinessRelationNode CopyRelation(
        BusinessRelationNode source,
        IReadOnlyList<BusinessNoteNode>? notes = null)
    {
        return new BusinessRelationNode
        {
            Id = source.Id,
            SourceEntityId = source.SourceEntityId,
            TargetEntityId = source.TargetEntityId,
            Kind = source.Kind,
            SourceNavigationName = source.SourceNavigationName,
            TargetNavigationName = source.TargetNavigationName,
            Notes = notes ?? source.Notes,
        };
    }

    private static BusinessWorkflowNode CopyWorkflow(
        BusinessWorkflowNode source,
        string? name = null,
        string? summary = null,
        string? trigger = null,
        string? presetId = null,
        IReadOnlyList<BusinessWorkflowStepNode>? steps = null,
        IReadOnlyList<BusinessWorkflowTransitionNode>? transitions = null,
        IReadOnlyList<BusinessNoteNode>? notes = null)
    {
        return new BusinessWorkflowNode
        {
            Id = source.Id,
            Name = name ?? source.Name,
            Summary = summary ?? source.Summary,
            Trigger = trigger ?? source.Trigger,
            PresetId = presetId ?? source.PresetId,
            Steps = steps ?? source.Steps,
            Transitions = transitions ?? source.Transitions,
            Notes = notes ?? source.Notes,
        };
    }

    private static PendingQuestionNode DismissQuestionIfMatches(
        PendingQuestionNode question,
        string? entityId = null,
        string? attributeId = null,
        string? behaviorId = null,
        string? relationId = null)
    {
        var matches =
            (!string.IsNullOrWhiteSpace(entityId) && string.Equals(question.RelatedEntityId, entityId, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(attributeId) && string.Equals(question.RelatedAttributeId, attributeId, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(behaviorId) && string.Equals(question.RelatedBehaviorId, behaviorId, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(relationId) && string.Equals(question.RelatedRelationId, relationId, StringComparison.OrdinalIgnoreCase));

        if (!matches)
            return question;

        return new PendingQuestionNode
        {
            Id = question.Id,
            Text = question.Text,
            Status = PendingQuestionStatus.Dismissed,
            Scope = question.Scope,
        };
    }

    private static PendingQuestionNode MoveQuestionAttributeReference(
        PendingQuestionNode question,
        string attributeId,
        string sourceEntityId,
        string targetEntityId)
    {
        if (!string.Equals(question.RelatedAttributeId, attributeId, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(question.RelatedEntityId, sourceEntityId, StringComparison.OrdinalIgnoreCase))
        {
            return question;
        }

        return new PendingQuestionNode
        {
            Id = question.Id,
            Text = question.Text,
            Status = question.Status,
            Scope = question.Scope,
            RelatedEntityId = targetEntityId,
            RelatedAttributeId = question.RelatedAttributeId,
            RelatedBehaviorId = question.RelatedBehaviorId,
            RelatedRelationId = question.RelatedRelationId,
        };
    }

    private static BusinessEntityNode RequireEntity(BusinessAuthoringDocument document, string? entityId, string path)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            throw new PatchOperationException("patch.entity.required", "Patch operace vyzaduje entityId.", path);

        return document.Entities.FirstOrDefault(entity => string.Equals(entity.Id, entityId, StringComparison.OrdinalIgnoreCase))
            ?? throw new PatchOperationException("patch.entity.missing", $"Entita {entityId} nebyla nalezena.", path);
    }

    private static BusinessAttributeNode RequireAttribute(BusinessEntityNode entity, string? attributeId, string path)
    {
        if (string.IsNullOrWhiteSpace(attributeId))
            throw new PatchOperationException("patch.attribute.required", "Patch operace vyzaduje attributeId.", path);

        return entity.Attributes.FirstOrDefault(attribute => string.Equals(attribute.Id, attributeId, StringComparison.OrdinalIgnoreCase))
            ?? throw new PatchOperationException("patch.attribute.missing", $"Atribut {attributeId} v entite {entity.Id} nebyl nalezen.", path);
    }

    private static BusinessBehaviorNode RequireBehavior(BusinessEntityNode entity, string? behaviorId, string path)
    {
        if (string.IsNullOrWhiteSpace(behaviorId))
            throw new PatchOperationException("patch.behavior.required", "Patch operace vyzaduje behaviorId.", path);

        return entity.Behaviors.FirstOrDefault(behavior => string.Equals(behavior.Id, behaviorId, StringComparison.OrdinalIgnoreCase))
            ?? throw new PatchOperationException("patch.behavior.missing", $"Behavior {behaviorId} v entite {entity.Id} nebyl nalezen.", path);
    }

    private static BusinessRelationNode RequireRelation(BusinessAuthoringDocument document, string? relationId, string path)
    {
        if (string.IsNullOrWhiteSpace(relationId))
            throw new PatchOperationException("patch.relation.required", "Patch operace vyzaduje relationId.", path);

        return document.Relations.FirstOrDefault(relation => string.Equals(relation.Id, relationId, StringComparison.OrdinalIgnoreCase))
            ?? throw new PatchOperationException("patch.relation.missing", $"Relation {relationId} nebyla nalezena.", path);
    }

    private static BusinessWorkflowNode RequireWorkflow(BusinessAuthoringDocument document, string? workflowId, string path)
    {
        if (string.IsNullOrWhiteSpace(workflowId))
            throw new PatchOperationException("patch.workflow.required", "Patch operace vyzaduje workflowId.", path);

        return document.Workflows.FirstOrDefault(workflow => string.Equals(workflow.Id, workflowId, StringComparison.OrdinalIgnoreCase))
            ?? throw new PatchOperationException("patch.workflow.missing", $"Workflow {workflowId} nebylo nalezeno.", path);
    }

    private static BusinessWorkflowStepNode RequireWorkflowStep(BusinessWorkflowNode workflow, string? stepId, string path)
    {
        if (string.IsNullOrWhiteSpace(stepId))
            throw new PatchOperationException("patch.workflow_step.required", "Patch operace vyzaduje workflowStepId.", path);

        return workflow.Steps.FirstOrDefault(step => string.Equals(step.Id, stepId, StringComparison.OrdinalIgnoreCase))
            ?? throw new PatchOperationException("patch.workflow_step.missing", $"Workflow step {stepId} ve workflow {workflow.Id} nebyl nalezen.", path);
    }

    private static BusinessWorkflowTransitionNode RequireWorkflowTransition(BusinessWorkflowNode workflow, string? transitionId, string path)
    {
        if (string.IsNullOrWhiteSpace(transitionId))
            throw new PatchOperationException("patch.workflow_transition.required", "Patch operace vyzaduje workflowTransitionId.", path);

        return workflow.Transitions.FirstOrDefault(transition => string.Equals(transition.Id, transitionId, StringComparison.OrdinalIgnoreCase))
            ?? throw new PatchOperationException("patch.workflow_transition.missing", $"Workflow transition {transitionId} ve workflow {workflow.Id} nebyl nalezen.", path);
    }

    private void MaybeAddWorkflowReferenceQuestions(
        BusinessAuthoringDocument document,
        BusinessWorkflowNode workflow,
        BusinessWorkflowStepNode step,
        ICollection<PendingQuestionNode> generatedQuestions)
    {
        if (!string.IsNullOrWhiteSpace(step.RelatedEntityId)
            && !document.Entities.Any(entity => string.Equals(entity.Id, step.RelatedEntityId, StringComparison.OrdinalIgnoreCase)))
        {
            MaybeAddGeneratedQuestion(
                generatedQuestions,
                CreateGeneratedQuestionId(workflow.Id, step.Id, "entity"),
                $"Workflow {workflow.Name} krok {step.Name} odkazuje na neznámou entitu {step.RelatedEntityId}.",
                PendingQuestionScope.Entity,
                relatedEntityId: step.RelatedEntityId);
        }

        if (!string.IsNullOrWhiteSpace(step.RelatedBehaviorId))
        {
            if (string.IsNullOrWhiteSpace(step.RelatedEntityId))
            {
                MaybeAddGeneratedQuestion(
                    generatedQuestions,
                    CreateGeneratedQuestionId(workflow.Id, step.Id, "behavior-entity"),
                    $"Workflow {workflow.Name} krok {step.Name} odkazuje na behavior {step.RelatedBehaviorId}, ale nema RelatedEntityId.",
                    PendingQuestionScope.Behavior);
            }
            else
            {
                var entity = document.Entities.FirstOrDefault(item => string.Equals(item.Id, step.RelatedEntityId, StringComparison.OrdinalIgnoreCase));
                if (entity is null || !entity.Behaviors.Any(behavior => string.Equals(behavior.Id, step.RelatedBehaviorId, StringComparison.OrdinalIgnoreCase)))
                {
                    MaybeAddGeneratedQuestion(
                        generatedQuestions,
                        CreateGeneratedQuestionId(workflow.Id, step.Id, "behavior"),
                        $"Workflow {workflow.Name} krok {step.Name} odkazuje na neznámý behavior {step.RelatedBehaviorId} v entite {step.RelatedEntityId}.",
                        PendingQuestionScope.Behavior,
                        relatedEntityId: step.RelatedEntityId,
                        relatedBehaviorId: step.RelatedBehaviorId);
                }
            }
        }
    }

    private static void MaybeAddGeneratedQuestion(
        ICollection<PendingQuestionNode> generatedQuestions,
        string id,
        string text,
        PendingQuestionScope scope,
        string? relatedEntityId = null,
        string? relatedBehaviorId = null)
    {
        if (generatedQuestions.Any(question => string.Equals(question.Id, id, StringComparison.OrdinalIgnoreCase)))
            return;

        generatedQuestions.Add(new PendingQuestionNode
        {
            Id = id,
            Text = text,
            Scope = scope,
            RelatedEntityId = relatedEntityId,
            RelatedBehaviorId = relatedBehaviorId,
        });
    }

    private static string CreateGeneratedQuestionId(string workflowId, string stepId, string suffix)
    {
        return $"workflow-{workflowId}-{stepId}-{suffix}";
    }
}
