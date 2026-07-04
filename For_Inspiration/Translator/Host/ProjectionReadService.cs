using MetaForge.BusinessModel;
using MetaForge.Core.Catalog;
using MetaForge.Core.Discovery;

namespace MetaForge.Translator;

/// <summary>
/// Orchestruje <see cref="IProjectionQueryService"/> (replay) a <see cref="ExpertProjectionBuilder"/> (expert sekce)
/// podle zadanych <see cref="ProjectionOptions"/>.
/// Tato trida je jediny legalni vstupni bod pro read projekci v Translator vrstve.
/// </summary>
public sealed class ProjectionReadService
{
    private readonly IProjectionQueryService _projectionQueryService;
    private readonly ExpertProjectionBuilder _expertProjectionBuilder;
    private readonly IDiscoverySession? _discoverySession;

    public ProjectionReadService(
        IProjectionQueryService projectionQueryService,
        CatalogManager catalogManager,
        IDiscoverySession? discoverySession = null)
    {
        _projectionQueryService = projectionQueryService ?? throw new ArgumentNullException(nameof(projectionQueryService));
        _expertProjectionBuilder = new ExpertProjectionBuilder(catalogManager ?? throw new ArgumentNullException(nameof(catalogManager)));
        _discoverySession = discoverySession;
    }

    /// <summary>
    /// Vrati projekci podle zadanych options.
    /// Pokud options == null nebo Basic(), expert sekce se nestavi.
    /// </summary>
    public async Task<ProjectionView> GetProjectionAsync(
        string? streamId = null,
        ProjectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var replay = await _projectionQueryService.GetProjectionAsync(streamId, cancellationToken);

        var effectiveOptions = options ?? ProjectionOptions.Basic();
        ExpertProjectionView? expert = null;

        if (effectiveOptions.HasAnyExpertSection)
            expert = _expertProjectionBuilder.Build(replay.Document, replay, effectiveOptions);

        WorkflowProjectionView? workflow = null;
        if (effectiveOptions.Workflow)
            workflow = WorkflowProjectionBuilder.Build(replay.Document, replay.WorkflowBindingSyncStates);

        AuthoringContextView? authoringContext = null;
        if (effectiveOptions.AuthoringContext || effectiveOptions.DiscoveryContext)
        {
            authoringContext = AuthoringContextBuilder.Build(
                replay.Document,
                includeDiscovery: effectiveOptions.DiscoveryContext,
                discoverySession: _discoverySession);
        }

        return new ProjectionView
        {
            Replay = replay,
            Expert = expert,
            Workflow = workflow,
            AuthoringContext = authoringContext,
        };
    }
}
