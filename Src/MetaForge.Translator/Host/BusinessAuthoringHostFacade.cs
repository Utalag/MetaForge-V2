using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.BusinessModel.Patches.Operations;
using MetaForge.Translator.Projections;
using MetaForge.Translator.Translation;

namespace MetaForge.Translator.Host;

/// <summary>
/// JEDINÝ VSTUPNÍ BOD pro host surfaces (CLI, MCP, WebApi).
/// Orchestruje write (PatchEngine) i read (ProjectionReadService) path.
/// </summary>
public sealed class BusinessAuthoringHostFacade
{
    private BusinessAuthoringDocument _document;
    private readonly object _documentLock = new();
    private readonly CommandLogStore _logStore;
    private readonly PatchEngine _patchEngine;
    private readonly ReplayEngine _replayEngine;
    private readonly ProjectionReadService _projectionService;
    private readonly WriteBackService _writeBackService;
    private readonly IBusinessTranslator _translator;

    public BusinessAuthoringHostFacade(
        BusinessAuthoringDocument document,
        CommandLogStore logStore,
        PatchEngine patchEngine,
        ReplayEngine replayEngine,
        ProjectionReadService projectionService,
        WriteBackService writeBackService,
        IBusinessTranslator translator)
    {
        _document = document;
        _logStore = logStore;
        _patchEngine = patchEngine;
        _replayEngine = replayEngine;
        _projectionService = projectionService;
        _writeBackService = writeBackService;
        _translator = translator;
    }

    // === WRITE OPERATIONS ===

    /// <summary>Přidá novou entitu.</summary>
    public string AddEntity(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Název entity nesmí být prázdný.", nameof(name));

        var op = new AddEntityOp(name);
        lock (_documentLock) { _document = _patchEngine.Apply(_document, op); }
        return op.EntityId;
    }

    /// <summary>Aktualizuje název entity.</summary>
    /// <exception cref="InvalidOperationException">Pokud entita neexistuje.</exception>
    public void UpdateEntity(string entityId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Nový název entity nesmí být prázdný.", nameof(newName));

        var entity = _document.Entities.FirstOrDefault(e => e.Id == entityId)
            ?? throw new InvalidOperationException($"Entita s Id '{entityId}' neexistuje.");

        var op = new UpdateEntityOp(entityId, newName);
        lock (_documentLock) { _document = _patchEngine.Apply(_document, op); }
    }

    /// <summary>Smaže entitu a všechny její relace.</summary>
    public void DeleteEntity(string entityId)
    {
        var entity = _document.Entities.FirstOrDefault(e => e.Id == entityId)
            ?? throw new InvalidOperationException($"Entita s Id '{entityId}' neexistuje.");

        var op = new DeleteEntityOp(entityId);
        lock (_documentLock) { _document = _patchEngine.Apply(_document, op); }
    }

    /// <summary>Přidá atribut k entitě.</summary>
    public string AddAttribute(string entityId, string name, string type = "string", bool isRequired = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Název atributu nesmí být prázdný.", nameof(name));

        var entity = _document.Entities.FirstOrDefault(e => e.Id == entityId)
            ?? throw new InvalidOperationException($"Entita s Id '{entityId}' neexistuje.");

        var op = new AddAttributeOp(entityId, name, type, isRequired);
        lock (_documentLock) { _document = _patchEngine.Apply(_document, op); }
        return op.AttributeId;
    }

    /// <summary>Aplikuje enrichment data na atribut.</summary>
    public void ApplyEnrichment(string entityId, EnrichmentResult enrichment)
    {
        lock (_documentLock) { _document = _writeBackService.ApplyEnrichment(_document, entityId, enrichment); }
    }

    /// <summary>Nastaví CoreDetail na atributu (přes SetCoreDetailOp).</summary>
    public void SetCoreDetail(string entityId, string attributeId, BusinessAttributeCoreDetail coreDetail)
    {
        var op = new SetCoreDetailOp(entityId, attributeId, coreDetail);
        lock (_documentLock) { _document = _patchEngine.Apply(_document, op); }
    }

    /// <summary>Aktualizuje SyncState na atributu.</summary>
    public void UpdateSyncState(string entityId, string attributeId, AttributeSyncState newState)
    {
        var op = new UpdateSyncStateOp(entityId, attributeId, newState);
        lock (_documentLock) { _document = _patchEngine.Apply(_document, op); }
    }

    /// <summary>Vrátí aktuální projekci (PROP-056 — DocumentProjection).</summary>
    public DocumentProjection GetProjection(ProjectionFilter? filter = null) =>
        _projectionService.GetProjection(_document, filter);

    /// <summary>Vrátí expertní projekci — ekvivalent GetProjection(ProjectionPresets.Expert).</summary>
    public DocumentProjection GetExpertProjection() =>
        _projectionService.GetProjection(_document, ProjectionPresets.Expert);

    /// <summary>Exponuje ElementIdMapping z Translatoru (PROP-060).</summary>
    public ElementIdMapping? GetElementIdMapping()
    {
        if (_translator is DefaultBusinessTranslator dbt)
            return dbt.LastMapping;
        return null;
    }

    /// <summary>Vrátí samotný dokument (pro debugging).</summary>
    public BusinessAuthoringDocument GetDocument() { lock (_documentLock) { return _document; } }

    /// <summary>Vrátí počet commandů v logu.</summary>
    public int GetCommandCount() => _logStore.Count;
}
