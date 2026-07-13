using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.BusinessModel.Patches.Operations;
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

    // === WORKFLOW OPERATIONS ===

    /// <summary>Přidá nové workflow do dokumentu.</summary>
    public string AddWorkflow(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Název workflow nesmí být prázdný.", nameof(name));

        var op = new AddWorkflowOp(name, description);
        lock (_documentLock) { _document = _patchEngine.Apply(_document, op); }
        return op.WorkflowId;
    }

    /// <summary>Přidá krok do existujícího workflow.</summary>
    public string AddWorkflowStep(string workflowId, string stepName, BusinessWorkflowStepKind kind = BusinessWorkflowStepKind.Manual)
    {
        if (string.IsNullOrWhiteSpace(stepName))
            throw new ArgumentException("Název kroku nesmí být prázdný.", nameof(stepName));

        var op = new AddWorkflowStepOp(workflowId, stepName, kind);
        lock (_documentLock) { _document = _patchEngine.Apply(_document, op); }
        return op.StepId;
    }

    /// <summary>Přidá přechod mezi dvěma kroky workflow.</summary>
    public string AddWorkflowTransition(string workflowId, string fromStepId, string toStepId, string? condition = null, string? label = null)
    {
        var op = new AddWorkflowTransitionOp(workflowId, fromStepId, toStepId, condition, label);
        lock (_documentLock) { _document = _patchEngine.Apply(_document, op); }
        return op.TransitionId;
    }

    // === READ OPERATIONS ===

    /// <summary>Vrátí aktuální projekci.</summary>
    public ProjectionView GetProjection() =>
        _projectionService.GetProjection(_document);

    /// <summary>Vrátí expertní projekci s diagnostikou a relacemi (PROP-018).</summary>
    public ExpertProjectionView GetExpertProjection(ProjectionOptions? options = null) =>
        _projectionService.GetExpertProjection(_document, options);

    /// <summary>Vrátí samotný dokument (pro debugging).</summary>
    public BusinessAuthoringDocument GetDocument() { lock (_documentLock) { return _document; } }

    /// <summary>Vrátí počet commandů v logu.</summary>
    public int GetCommandCount() => _logStore.Count;
}
