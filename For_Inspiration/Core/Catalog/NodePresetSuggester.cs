namespace MetaForge.Core.Catalog;

/// <summary>
/// Deterministický orchestrátor pro nabízení presetů při vytváření node.
/// Nepoužívá AI — pracuje pouze nad <see cref="CatalogManager"/>.
/// </summary>
public sealed class NodePresetSuggester
{
    private readonly CatalogManager _catalogManager;
    private const int MaxResults = 5;

    public NodePresetSuggester(CatalogManager catalogManager)
    {
        _catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
    }

    /// <summary>
    /// Navrhne presety pro zadaný node kontext.
    /// </summary>
    public IReadOnlyList<NodePresetSuggestion> SuggestForNode(NodeCreateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Kind switch
        {
            NodeKind.Attribute => SuggestForAttribute(context),
            NodeKind.Entity => SuggestForEntity(context),
            NodeKind.Behavior => SuggestForBehavior(context),
            NodeKind.WorkflowStep => SuggestForWorkflowStep(context),
            _ => [],
        };
    }

    private IReadOnlyList<NodePresetSuggestion> SuggestForAttribute(NodeCreateContext context)
    {
        return _catalogManager.SuggestPresets(
            context.Name,
            context.Type,
            context.Description,
            context.Tags,
            itemTypes: [CatalogItemType.ValueObject])
            .Take(MaxResults)
            .ToList();
    }

    private IReadOnlyList<NodePresetSuggestion> SuggestForEntity(NodeCreateContext context)
    {
        return _catalogManager.SuggestPresets(
            context.Name,
            type: null,
            context.Description,
            context.Tags,
            itemTypes:
            [
                CatalogItemType.EntityTemplate,
                CatalogItemType.DomainTemplate,
                CatalogItemType.ArchitectureTemplate,
                CatalogItemType.ValueObject,
            ]).Take(MaxResults).ToList();
    }

    private IReadOnlyList<NodePresetSuggestion> SuggestForBehavior(NodeCreateContext context)
    {
        return _catalogManager.SuggestPresets(
            context.Name,
            type: null,
            context.Description,
            context.Tags,
            itemTypes: [CatalogItemType.ValueObject])
            .Take(MaxResults)
            .ToList();
    }

    private IReadOnlyList<NodePresetSuggestion> SuggestForWorkflowStep(NodeCreateContext context)
    {
        return _catalogManager.SuggestPresets(
            context.Name,
            type: null,
            context.Description,
            context.Tags,
            itemTypes:
            [
                CatalogItemType.ValueObject,
                CatalogItemType.ClassPreset,
            ])
            .Take(MaxResults)
            .ToList();
    }
}
