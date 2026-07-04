using System.Text;

namespace MetaForge.BusinessModel;

public sealed class BusinessIdAllocator
{
    public string CreateProjectId(string projectName)
    {
        return CreateSlug(projectName, "new-project");
    }

    public string CreateEntityId(string entityName, BusinessAuthoringDocument document)
    {
        return CreateUniqueId(CreateSlug(entityName, "entity"), document.Entities.Select(entity => entity.Id));
    }

    public string CreateCustomTypeId(string customTypeName, BusinessAuthoringDocument document)
    {
        return CreateUniqueId(CreateSlug(customTypeName, "customtype"), document.CustomTypes.Select(ct => ct.Id));
    }

    public string CreateAttributeId(string attributeName, BusinessEntityNode entity)
    {
        return CreateUniqueId(CreateSlug(attributeName, "attribute"), entity.Attributes.Select(attribute => attribute.Id));
    }

    public string CreateBehaviorId(string behaviorName, BusinessEntityNode entity)
    {
        return CreateUniqueId(CreateSlug(behaviorName, "behavior"), entity.Behaviors.Select(behavior => behavior.Id));
    }

    public string CreateRelationId(BusinessRelationNode relation, BusinessAuthoringDocument document)
    {
        var baseId = CreateSlug($"{relation.SourceEntityId}-{relation.Kind}-{relation.TargetEntityId}", "relation");
        return CreateUniqueId(baseId, document.Relations.Select(item => item.Id));
    }

    public string CreateWorkflowId(string workflowName, BusinessAuthoringDocument document)
    {
        return CreateUniqueId(CreateSlug(workflowName, "workflow"), document.Workflows.Select(workflow => workflow.Id));
    }

    public string CreateWorkflowStepId(string stepName, BusinessWorkflowNode workflow)
    {
        return CreateUniqueId(CreateSlug(stepName, "step"), workflow.Steps.Select(step => step.Id));
    }

    public string CreateWorkflowTransitionId(string? fromStepId, string? toStepId, BusinessWorkflowNode workflow)
    {
        var baseId = CreateSlug($"{fromStepId}-{toStepId}", "transition");
        return CreateUniqueId(baseId, workflow.Transitions.Select(transition => transition.Id));
    }

    public string CreateQuestionId(BusinessAuthoringDocument document)
    {
        return CreateUniqueId("q", document.PendingQuestions.Select(question => question.Id));
    }

    public string CreateNoteId()
    {
        return $"note-{Guid.NewGuid():N}";
    }

    private static string CreateUniqueId(string baseId, IEnumerable<string> existingIds)
    {
        var used = new HashSet<string>(existingIds.Where(id => !string.IsNullOrWhiteSpace(id)), StringComparer.OrdinalIgnoreCase);
        if (!used.Contains(baseId))
            return baseId;

        var suffix = 2;
        while (used.Contains($"{baseId}-{suffix}"))
            suffix++;

        return $"{baseId}-{suffix}";
    }

    private static string CreateSlug(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var builder = new StringBuilder(value.Length);
        var pendingDash = false;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                if (pendingDash && builder.Length > 0)
                    builder.Append('-');

                builder.Append(character);
                pendingDash = false;
                continue;
            }

            pendingDash = builder.Length > 0;
        }

        return builder.Length == 0 ? fallback : builder.ToString();
    }
}