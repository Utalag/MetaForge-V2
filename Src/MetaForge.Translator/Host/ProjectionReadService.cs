// ---------------------------------------------------------------------------
// MetaForge.Translator — ProjectionReadService
// Builds projections from BusinessAuthoringDocument using ProjectionBuilder (PROP-056).
// Vrstva: Translator / Host
// ---------------------------------------------------------------------------

using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.Core.Abstractions;
using MetaForge.Translator.Projections;
using MetaForge.Translator.Translation;

namespace MetaForge.Translator.Host;

public sealed class ProjectionReadService
{
    private readonly CommandLogStore _logStore;
    private readonly ReplayEngine _replayEngine;
    private readonly ProjectionBuilder _projectionBuilder;

    public ProjectionReadService(
        CommandLogStore logStore,
        ReplayEngine replayEngine,
        IBusinessTranslator translator,
        ElementIdMapping? idMapping = null)
    {
        _logStore = logStore ?? throw new ArgumentNullException(nameof(logStore));
        _replayEngine = replayEngine ?? throw new ArgumentNullException(nameof(replayEngine));
        _projectionBuilder = new ProjectionBuilder(
            translator ?? throw new ArgumentNullException(nameof(translator)),
            idMapping);
    }

    public DocumentProjection GetProjection(BusinessAuthoringDocument document, ProjectionFilter? filter = null)
    {
        filter ??= ProjectionPresets.Basic;
        return _projectionBuilder.Build(document, filter);
    }

    public DocumentProjection GetProjectionFromLog(ProjectionFilter? filter = null)
    {
        filter ??= ProjectionPresets.Basic;
        var commands = _logStore.GetAll();
        var document = _replayEngine.Replay(commands);
        return _projectionBuilder.Build(document, filter);
    }
}
