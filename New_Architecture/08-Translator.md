# Translator

> Facade, Projection, Write-back, TranslationService

---


### BusinessAuthoringHostFacade

```csharp
//context//
// Účel: Jediný vstupní bod pro host surfaces. Orchestruje write (PatchEngine) i read (ProjectionReadService) path.
// Vrstva: Translator.
// Vstup: Příkazy z CLI, MCP nebo WebApi.
// Výstup: Výsledky operací, ProjectionView, ExpertProjection.
// Závislosti: PatchEngine, CommandLogStore, ProjectionReadService, DefaultBusinessTranslator.
// Nezávislosti: Nezávisí na konkrétním host surface — facade je surface-agnostic.
// Invarianty: Každý write prochází PatchEngine. Každý read prochází ProjectionReadService. Žádný shortcut.
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

    // Read operations
    public ProjectionView GetProjection() { }
    public ExpertProjection GetExpertProjection() { }
    public BusinessAuthoringDocument GetDocument() { }
}
```

### DefaultBusinessTranslator

```csharp
//context//
// Účel: Překládá BusinessAttributeNode na TypeModel (Core). Resolvuje presety přes CatalogManager.
// Vrstva: Translator.
// Vstup: BusinessAttributeNode z BusinessAuthoringDocument.
// Výstup: TypeModel pro Core vrstvu, enrichment data pro write-back.
// Závislosti: CatalogManager (Core), BusinessAttributeNode (BusinessModel).
// Nezávislosti: Nezávisí na Generators — překlad je nezávislý na výstupním formátu.
// Invarianty: Překlad musí být deterministický pro stejný vstup. AI enrichment je volitelný overlay.
// Související typy: CatalogManager, TypeModel, WriteBackService, AiTranslationService.
// Testy: Translator.Tests/Translation/DefaultBusinessTranslatorTests.cs.

public class DefaultBusinessTranslator : IBusinessTranslator
{
    public TypeModel Translate(BusinessAttributeNode attribute) { }
    public EnrichmentResult? TryEnrich(BusinessAttributeNode attribute) { }
}
```

---

## Generators vrstva