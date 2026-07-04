using MetaForge.BusinessModel;
using MetaForge.Core.Catalog;

namespace MetaForge.Translator;

/// <summary>
/// Konzervativni scope validator pro AI navrzene operace v node assist preview.
/// Nepovolene nebo mimo-scope operace jsou odfiltrovany driv, nez se dostanou k host surface.
/// </summary>
internal static class NodeAssistOperationScopeValidator
{
    public static NodeAssistResult Sanitize(NodeAssistContext context, NodeAssistResult result)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(result);

        if (result.ProposedOperations.Count == 0)
            return result;

        var allowedOperations = new List<BusinessPatchOperation>();
        var rejectedOperations = new List<string>();

        foreach (var operation in result.ProposedOperations)
        {
            if (IsAllowed(context, operation))
            {
                allowedOperations.Add(operation);
            }
            else
            {
                rejectedOperations.Add(string.IsNullOrWhiteSpace(operation.Op) ? "<empty>" : operation.Op);
            }
        }

        if (rejectedOperations.Count == 0)
            return result;

        var warnings = result.Warnings
            .Concat([
                $"Node assist odfiltroval {rejectedOperations.Count} operaci mimo povoleny scope: {string.Join(", ", rejectedOperations.Distinct(StringComparer.OrdinalIgnoreCase))}."
            ])
            .ToArray();

        return new NodeAssistResult
        {
            Summary = result.Summary,
            Inputs = result.Inputs,
            Returns = result.Returns,
            Explanation = result.Explanation,
            ProposedOperations = allowedOperations,
            Warnings = warnings,
        };
    }

    private static bool IsAllowed(NodeAssistContext context, BusinessPatchOperation operation)
    {
        if (context.Kind is null || operation is null)
            return false;

        if (!string.IsNullOrWhiteSpace(operation.RelationId)
            || !string.IsNullOrWhiteSpace(operation.QuestionId)
            || !string.IsNullOrWhiteSpace(operation.WorkflowId)
            || !string.IsNullOrWhiteSpace(operation.WorkflowStepId)
            || !string.IsNullOrWhiteSpace(operation.WorkflowTransitionId))
        {
            return false;
        }

        return context.Kind.Value switch
        {
            NodeKind.Entity => IsAllowedForEntityContext(context, operation),
            NodeKind.Attribute => IsAllowedForAttributeContext(context, operation),
            NodeKind.Behavior => IsAllowedForBehaviorContext(context, operation),
            _ => false,
        };
    }

    private static bool IsAllowedForEntityContext(NodeAssistContext context, BusinessPatchOperation operation)
    {
        if (!EntityMatches(context, operation))
            return false;

        return operation.Op switch
        {
            "update_entity" => HasNoChildTarget(operation),
            "add_attribute" => HasNoChildTarget(operation),
            "add_behavior" => HasNoChildTarget(operation),
            "update_attribute" => HasAttributeTarget(operation),
            "apply_preset" => HasAttributeTarget(operation),
            "enrich_attribute" => HasAttributeTarget(operation),
            "update_coredetail" => HasAttributeTarget(operation),
            "update_behavior" => HasBehaviorTarget(operation),
            _ => false,
        };
    }

    private static bool IsAllowedForAttributeContext(NodeAssistContext context, BusinessPatchOperation operation)
    {
        return EntityMatches(context, operation)
            && AttributeMatches(context, operation)
            && operation.Op is "update_attribute" or "apply_preset" or "enrich_attribute" or "update_coredetail"
            && string.IsNullOrWhiteSpace(operation.BehaviorId);
    }

    private static bool IsAllowedForBehaviorContext(NodeAssistContext context, BusinessPatchOperation operation)
    {
        return EntityMatches(context, operation)
            && BehaviorMatches(context, operation)
            && operation.Op == "update_behavior"
            && string.IsNullOrWhiteSpace(operation.AttributeId);
    }

    private static bool EntityMatches(NodeAssistContext context, BusinessPatchOperation operation)
    {
        return !string.IsNullOrWhiteSpace(operation.EntityId)
            && string.Equals(operation.EntityId, context.EntityId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool AttributeMatches(NodeAssistContext context, BusinessPatchOperation operation)
    {
        return !string.IsNullOrWhiteSpace(context.NodeId)
            && !string.IsNullOrWhiteSpace(operation.AttributeId)
            && string.Equals(operation.AttributeId, context.NodeId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool BehaviorMatches(NodeAssistContext context, BusinessPatchOperation operation)
    {
        return !string.IsNullOrWhiteSpace(context.NodeId)
            && !string.IsNullOrWhiteSpace(operation.BehaviorId)
            && string.Equals(operation.BehaviorId, context.NodeId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasNoChildTarget(BusinessPatchOperation operation)
    {
        return string.IsNullOrWhiteSpace(operation.AttributeId)
            && string.IsNullOrWhiteSpace(operation.BehaviorId);
    }

    private static bool HasAttributeTarget(BusinessPatchOperation operation)
    {
        return !string.IsNullOrWhiteSpace(operation.AttributeId)
            && string.IsNullOrWhiteSpace(operation.BehaviorId);
    }

    private static bool HasBehaviorTarget(BusinessPatchOperation operation)
    {
        return !string.IsNullOrWhiteSpace(operation.BehaviorId)
            && string.IsNullOrWhiteSpace(operation.AttributeId);
    }
}

/// <summary>
/// Validátor pro AI-generované operace v rámci node-level asistence.
/// Zajišťuje, že návrhy míří pouze do povoleného node scope.
/// </summary>
internal static class NodeAssistOperationValidator
{
    private static readonly HashSet<string> AllowedOps = new(StringComparer.OrdinalIgnoreCase)
    {
        "add_attribute",
        "update_attribute",
        "delete_attribute",
        "add_behavior",
        "update_behavior",
        "delete_behavior",
        "update_entity",
    };

    /// <summary>
    /// Ověří, že všechny operace jsou povoleného typu a míří do cílové entity.
    /// </summary>
    public static bool Validate(
        IReadOnlyList<BusinessPatchOperation> operations,
        BusinessEntityNode targetEntity)
    {
        foreach (var op in operations)
        {
            if (!AllowedOps.Contains(op.Op))
                return false;

            if (!IsTargetEntity(op.EntityId, targetEntity))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Normalizuje EntityId všech operací na canonical ID cílové entity.
    /// </summary>
    public static IReadOnlyList<BusinessPatchOperation> NormalizeEntityIds(
        IReadOnlyList<BusinessPatchOperation> operations,
        BusinessEntityNode targetEntity)
    {
        return operations.Select(op => new BusinessPatchOperation
        {
            Op = op.Op,
            EntityId = targetEntity.Id,
            AttributeId = op.AttributeId,
            BehaviorId = op.BehaviorId,
            RelationId = op.RelationId,
            QuestionId = op.QuestionId,
            WorkflowId = op.WorkflowId,
            WorkflowStepId = op.WorkflowStepId,
            WorkflowTransitionId = op.WorkflowTransitionId,
            NewIndex = op.NewIndex,
            Data = op.Data,
        }).ToList();
    }

    private static bool IsTargetEntity(string? entityId, BusinessEntityNode targetEntity)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            return false;

        return string.Equals(entityId, targetEntity.Id, StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityId, targetEntity.Name, StringComparison.OrdinalIgnoreCase);
    }
}
