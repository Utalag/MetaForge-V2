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
    private readonly BusinessAuthoringDocument _document;
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
        _patchEngine.Apply(_document, op);
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
        _patchEngine.Apply(_document, op);
    }

    /// <summary>Smaže entitu a všechny její relace.</summary>
    public void DeleteEntity(string entityId)
    {
        var entity = _document.Entities.FirstOrDefault(e => e.Id == entityId)
            ?? throw new InvalidOperationException($"Entita s Id '{entityId}' neexistuje.");

        var op = new DeleteEntityOp(entityId);
        _patchEngine.Apply(_document, op);
    }

    /// <summary>Přidá atribut k entitě.</summary>
    public string AddAttribute(string entityId, string name, string type = "string", bool isRequired = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Název atributu nesmí být prázdný.", nameof(name));

        var entity = _document.Entities.FirstOrDefault(e => e.Id == entityId)
            ?? throw new InvalidOperationException($"Entita s Id '{entityId}' neexistuje.");

        var op = new AddAttributeOp(entityId, name, type, isRequired);
        _patchEngine.Apply(_document, op);
        return op.AttributeId;
    }

    /// <summary>Aplikuje enrichment data na atribut.</summary>
    public void ApplyEnrichment(string entityId, EnrichmentResult enrichment)
    {
        _writeBackService.ApplyEnrichment(_document, entityId, enrichment);
    }

    // === READ OPERATIONS ===

    /// <summary>Vrátí aktuální projekci.</summary>
    public ProjectionView GetProjection() =>
        _projectionService.GetProjection(_document);

    /// <summary>Vrátí samotný dokument (pro debugging).</summary>
    public BusinessAuthoringDocument GetDocument() => _document;

    /// <summary>Vrátí počet commandů v logu.</summary>
    public int GetCommandCount() => _logStore.Count;
}
