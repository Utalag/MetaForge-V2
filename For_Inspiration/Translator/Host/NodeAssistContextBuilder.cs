using MetaForge.Core.Catalog;

namespace MetaForge.Translator;

/// <summary>
/// Builder pro <see cref="NodeAssistContext"/> z <see cref="ProjectionView"/> a <see cref="NodePath"/>.
/// </summary>
internal static class NodeAssistContextBuilder
{
    public static NodeAssistContext? Build(ProjectionView projection, NodePath path, bool includeDiscovery = false)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(path);

        if (projection.Expert is null)
            return null;

        var expert = projection.Expert;

        // --- Najit entitu ---
        var entity = expert.Entities.FirstOrDefault(e =>
            string.Equals(e.Id, path.EntityNameOrId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.Name, path.EntityNameOrId, StringComparison.OrdinalIgnoreCase));

        if (entity is null)
            return null;

        // --- Cela entita (bez specifickeho node) ---
        if (string.IsNullOrWhiteSpace(path.NodeNameOrId))
        {
            return new NodeAssistContext
            {
                EntityId = entity.Id,
                EntityName = entity.Name,
                Kind = NodeKind.Entity,
                Entity = entity,
                SiblingAttributeNames = entity.Attributes.Select(a => a.Name).ToArray(),
                SiblingBehaviorNames = entity.Behaviors.Select(b => b.Name).ToArray(),
                TotalEntityCount = expert.EntityCount,
                TotalRelationCount = expert.RelationCount,
                Workflow = projection.AuthoringContext?.Workflow ?? new WorkflowSummary(),
                OpenQuestionCount = expert.OpenQuestionCount,
                OpenQuestionTexts = projection.AuthoringContext?.PendingQuestions.OpenQuestionTexts.Take(5).ToArray() ?? [],
                DiscoveryHints = includeDiscovery ? projection.AuthoringContext?.Discovery : null,
            };
        }

        // --- Najit node podle Kind nebo odvodit ---
        var attribute = entity.Attributes.FirstOrDefault(a =>
            string.Equals(a.Id, path.NodeNameOrId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(a.Name, path.NodeNameOrId, StringComparison.OrdinalIgnoreCase));

        var behavior = entity.Behaviors.FirstOrDefault(b =>
            string.Equals(b.Id, path.NodeNameOrId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(b.Name, path.NodeNameOrId, StringComparison.OrdinalIgnoreCase));

        if (path.Kind.HasValue)
        {
            switch (path.Kind.Value)
            {
                case NodeKind.Attribute when attribute is not null:
                    return BuildAttributeContext(entity, attribute, expert, projection, includeDiscovery);
                case NodeKind.Behavior when behavior is not null:
                    return BuildBehaviorContext(entity, behavior, expert, projection, includeDiscovery);
                case NodeKind.WorkflowStep:
                    // WorkflowStep assist zatim neni implementovan
                    return null;
                default:
                    return null;
            }
        }

        // --- Kind neni zadan — odvodit nebo detekovat nejednoznacnost ---
        if (attribute is not null && behavior is not null)
        {
            // Nejednoznacne — existuje jak attribute tak behavior se stejnym nazvem
            return null;
        }

        if (attribute is not null)
            return BuildAttributeContext(entity, attribute, expert, projection, includeDiscovery);

        if (behavior is not null)
            return BuildBehaviorContext(entity, behavior, expert, projection, includeDiscovery);

        // Nic nenalezeno
        return null;
    }

    private static NodeAssistContext BuildAttributeContext(
        ExpertEntityProjection entity,
        ExpertAttributeProjection attribute,
        ExpertProjectionView expert,
        ProjectionView projection,
        bool includeDiscovery)
    {
        return new NodeAssistContext
        {
            EntityId = entity.Id,
            EntityName = entity.Name,
            NodeId = attribute.Id,
            NodeName = attribute.Name,
            Kind = NodeKind.Attribute,
            Attribute = attribute,
            SiblingAttributeNames = entity.Attributes.Where(a => a.Id != attribute.Id).Select(a => a.Name).ToArray(),
            SiblingBehaviorNames = entity.Behaviors.Select(b => b.Name).ToArray(),
            TotalEntityCount = expert.EntityCount,
            TotalRelationCount = expert.RelationCount,
            Workflow = projection.AuthoringContext?.Workflow ?? new WorkflowSummary(),
            OpenQuestionCount = expert.OpenQuestionCount,
            OpenQuestionTexts = projection.AuthoringContext?.PendingQuestions.OpenQuestionTexts.Take(5).ToArray() ?? [],
            DiscoveryHints = includeDiscovery ? projection.AuthoringContext?.Discovery : null,
        };
    }

    private static NodeAssistContext BuildBehaviorContext(
        ExpertEntityProjection entity,
        ExpertBehaviorProjection behavior,
        ExpertProjectionView expert,
        ProjectionView projection,
        bool includeDiscovery)
    {
        return new NodeAssistContext
        {
            EntityId = entity.Id,
            EntityName = entity.Name,
            NodeId = behavior.Id,
            NodeName = behavior.Name,
            Kind = NodeKind.Behavior,
            Behavior = behavior,
            SiblingAttributeNames = entity.Attributes.Select(a => a.Name).ToArray(),
            SiblingBehaviorNames = entity.Behaviors.Where(b => b.Id != behavior.Id).Select(b => b.Name).ToArray(),
            TotalEntityCount = expert.EntityCount,
            TotalRelationCount = expert.RelationCount,
            Workflow = projection.AuthoringContext?.Workflow ?? new WorkflowSummary(),
            OpenQuestionCount = expert.OpenQuestionCount,
            OpenQuestionTexts = projection.AuthoringContext?.PendingQuestions.OpenQuestionTexts.Take(5).ToArray() ?? [],
            DiscoveryHints = includeDiscovery ? projection.AuthoringContext?.Discovery : null,
        };
    }
}
