# Translator

> Facade, Projection, Write-back, ITranslationService, DefaultBusinessTranslator

**Aktualizace:** PROP-020 (2026-07-04) — WriteBackService používá SetCoreDetailOp přes PatchEngine, CoreDetail/SyncState v projekcích, překlad celého dokumentu.

---

## BusinessAuthoringHostFacade

```csharp
//context//
// Účel: Jediný vstupní bod pro host surfaces. Orchestruje write (PatchEngine) i read (ProjectionReadService) path.
// Vrstva: Translator.
// Vstup: Příkazy z CLI, MCP nebo WebApi.
// Výstup: Výsledky operací, ProjectionView, ExpertProjection.
// Závislosti: PatchEngine, CommandLogStore, ProjectionReadService, DefaultBusinessTranslator.
// Nezávislosti: Nezávisí na konkrétním host surface — facade je surface-agnostic.
// Invarianty: Každý write prochází PatchEngine. Každý read prochází ProjectionReadService. Žádný shortcut.
// PROP-020: Nové operace SetCoreDetail, UpdateSyncState. ApplyEnrichment → SetCoreDetailOp.
// Související typy: ProjectionReadService, ProjectionView, ExpertProjection, all host surfaces.
// Testy: Translator.Tests/Host/BusinessAuthoringHostFacadeTests.cs.

public class BusinessAuthoringHostFacade
{
    // Write operations
    public void AddEntity(string name) { }
    public void UpdateEntity(string entityId, string newName) { }
    public void DeleteEntity(string entityId) { }
    public void AddAttribute(string entityId, string name, string type) { }
    public void UpdateAttribute(string entityId, string attributeId, string? name, string? type) { }
    public void ApplyEnrichment(string entityId, string attributeId, object enrichmentData) { }

    // --- PROP-020: Nové operace ---
    public void SetCoreDetail(string entityId, string attributeId, BusinessAttributeCoreDetail detail) { }
    public void UpdateSyncState(string entityId, string attributeId, AttributeSyncState newState) { }

    // Read operations
    public ProjectionView GetProjection() { }
    public ExpertProjection GetExpertProjection() { }
    public BusinessAuthoringDocument GetDocument() { }
}
```

## DefaultBusinessTranslator — rozšíření na celý dokument (PROP-020)

```csharp
//context//
// Účel: Překládá celý BusinessAuthoringDocument na Core elementy (ClassElement, PropertyElement).
//        Dříve překládal jen BusinessAttributeNode → TypeModel.
//        Nyní: entity → ClassElement, atributy → PropertyElement, chování → MethodElement.
// Vrstva: Translator.
// Vstup: BusinessAuthoringDocument z BusinessModel.
// Výstup: Seznam RootElement pro Core vrstvu, enrichment data pro write-back.
// Závislosti: CatalogManager (Core), BusinessAuthoringDocument (BusinessModel).
// Nezávislosti: Nezávisí na Generators — překlad je nezávislý na výstupním formátu.
// Invarianty: Překlad musí být deterministický pro stejný vstup. AI enrichment je volitelný overlay.
// PROP-020: Rozšířeno na celý dokument (entity→ClassElement). Bere v potaz CoreDetail.
// Související typy: CatalogManager, TypeModel, WriteBackService, AiTranslationService.
// Testy: Translator.Tests/Translation/DefaultBusinessTranslatorTests.cs.

public class DefaultBusinessTranslator : IBusinessTranslator
{
    public TypeModel Translate(BusinessAttributeNode attribute) { }
    public EnrichmentResult? TryEnrich(BusinessAttributeNode attribute) { }

    // PROP-020: Překlad celého dokumentu
    public IReadOnlyList<RootElement> TranslateDocument(BusinessAuthoringDocument document) { }
}
```

## WriteBackService — SetCoreDetailOp (PROP-020 klíčová změna)

```csharp
//context//
// Účel: Zapisuje AI enrichment výsledky zpět do BusinessModel pomocí SetCoreDetailOp přes PatchEngine.
// Vrstva: Translator.
// Vstup: EnrichmentResult z AI vrstvy.
// Výstup: CoreDetail zapsaný na BusinessAttributeNode (přes PatchEngine).
// PROP-020: NEMUTUJE přímo dokument — používá SetCoreDetailOp přes PatchEngine.
//            Tím je zajištěna konzistence command logu a možnost replaye.
// Související typy: PatchEngine, SetCoreDetailOp, BusinessAttributeCoreDetail, AiTranslationService.
// Testy: Translator.Tests/Translation/WriteBackServiceTests.cs.

public sealed class WriteBackService
{
    public void WriteCoreDetail(
        BusinessAuthoringDocument document,
        string entityId,
        string attributeId,
        EnrichmentResult result) { }  // interně: PatchEngine.Apply(document, new SetCoreDetailOp(...))
}
```

## ProjectionReadService + ProjectionView — CoreDetail a SyncState (PROP-020)

```csharp
//context//
// Účel: Projekce dokumentu pro read path. Zohledňuje CoreDetail a SyncState.
// Vrstva: Translator.
// PROP-020: ProjectionView přidává CoreDetailInfo a SyncState do projekce atributu.
// Související typy: ProjectionView, BusinessAttributeCoreDetail, AttributeSyncState.
// Testy: Translator.Tests/Host/ProjectionReadServiceTests.cs.

public class ProjectionReadService
{
    public ProjectionView GetProjection(BusinessAuthoringDocument document) { }
}

public class ProjectionView
{
    public IReadOnlyList<EntityProjection> Entities { get; init; } = [];
}

public class EntityProjection
{
    public string Id { get; init; }
    public string Name { get; init; }
    public IReadOnlyList<AttributeProjection> Attributes { get; init; } = [];
}

public class AttributeProjection
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Type { get; init; }
    public bool IsRequired { get; init; }

    // PROP-020: Core enrichment informace
    public CoreDetailInfo? CoreDetail { get; init; }
    public AttributeSyncState SyncState { get; init; }
}

// PROP-020: Projekce CoreDetail pro read path
public class CoreDetailInfo
{
    public CoreInfoSource Source { get; init; }
    public string? ValueObjectName { get; init; }
    public bool IsStrongType { get; init; }
    public DateTimeOffset? LastSyncedAt { get; init; }
}
```

## ExpertProjection (PROP-018)

```csharp
//context//
// Účel: Detailní projekce pro AI modely — obsahuje entity, atributy, coreDetail, syncState, vztahy.
// Vrstva: Translator.
// Vstup: BusinessAuthoringDocument.
// Výstup: Strukturovaný JSON pro AI kontext.
// Související typy: ProjectionReadService, ProjectionView.
// Testy: Translator.Tests/Host/ExpertProjectionTests.cs.

public class ExpertProjection
{
    public string ProjectName { get; init; }
    public string SchemaVersion { get; init; }
    public IReadOnlyList<ExpertEntityProjection> Entities { get; init; } = [];
}
```

---

## ITranslationService

```csharp
//context//
// Účel: Kontrakt pro AI-assisted enrichment business atributů.
// Vrstva: Translator (interface).
// Implementace: MetaForge.Ai.Translation.AiTranslationService.
// Vstup: BusinessAttributeNode + ProjectionView (kontext projekce).
// Výstup: EnrichmentResult? — null pokud AI není dostupná (graceful fallback).
// Závislosti: BusinessAttributeNode (BusinessModel), ProjectionView (Translator.Host).
// Nezávislosti: Nezávisí na AI infrastruktuře — implementace je volitelná.
// Invarianty: Volající musí počítat s null (AI není k dispozici).
// Související typy: BusinessAttributeNode, ProjectionView, EnrichmentResult, AiTranslationService.
// Testy: Translator.Tests/Translation/ITranslationServiceTests.cs.

public interface ITranslationService
{
    /// <summary>
    /// Pokusí se o AI enrichment atributu s kontextem projekce.
    /// Vrací null pokud AI selže nebo není k dispozici.
    /// </summary>
    Task<EnrichmentResult?> EnrichAsync(
        BusinessAttributeNode attribute,
        ProjectionView context,
        CancellationToken ct = default);
}
```

---

## Kompletní flow (PROP-020)

```
User/AI-1 → BusinessAuthoringHostFacade.AddEntity() → PatchEngine → CommandLogStore → Document
         → DefaultBusinessTranslator.TranslateDocument() → Core elementy
         → AiTranslationService.EnrichAsync() → EnrichmentResult
         → WriteBackService.WriteCoreDetail()
             → PatchEngine.Apply(document, SetCoreDetailOp{...})
             → CommandLogStore.TryAppend(envelope)
             → Document s CoreDetail
         → ProjectionReadService.GetProjection() → ProjectionView (včetně SyncState)
```