---
name: new-architecture-translator
description: "Pouzij pri: praci s Translator vrstvou — BusinessAuthoringHostFacade, DefaultBusinessTranslator, ProjectionReadService, WriteBackService, ExpertProjection, ProjectionView."
---

# new-architecture-translator

Zajistit konzistentní implementaci Translator vrstvy dle `08-Translator.md`. Hlídat, že Facade je jediný entry point pro host surfaces a že write/read path jsou oddělené.

## Kdy použít

- Při práci se soubory v `Src/MetaForge.Translator/`
- Při implementaci BusinessAuthoringHostFacade
- Při implementaci DefaultBusinessTranslator, ProjectionReadService
- Při implementaci WriteBackService

## Invarianty

| # | Invariant | Důsledek |
|---|-----------|----------|
| 1 | **Facade je jediný entry point** | Host surfaces (CLI, MCP, WebApi) volají pouze Facade |
| 2 | **Write path jde přes PatchEngine** | Každá mutace prochází PatchEngine → CommandLog |
| 3 | **Read path jde přes ProjectionReadService** | Každé čtení prochází replayem |
| 4 | **Facade je surface-agnostic** | Nezávisí na CLI/MCP/WebApi — žádné usingy na host projekty |
| 5 | **Translator je deterministický** | Stejný BusinessAttributeNode → stejný TypeModel. AI enrichment je volitelný overlay. |

## Klíčové typy

### BusinessAuthoringHostFacade

```csharp
public class BusinessAuthoringHostFacade
{
    // Write operations
    public string AddEntity(string name);
    public void UpdateEntity(string entityId, string newName);
    public void DeleteEntity(string entityId);
    public string AddAttribute(string entityId, string name, string type = "string", bool isRequired = false);
    public void ApplyEnrichment(string entityId, EnrichmentResult enrichment);

    // Read operations
    public ProjectionView GetProjection();
    public BusinessAuthoringDocument GetDocument();
    public int GetCommandCount();
}
```

### DefaultBusinessTranslator

```csharp
public class DefaultBusinessTranslator : IBusinessTranslator
{
    public DefaultBusinessTranslator(CatalogManager catalog);
    public TypeModel Translate(BusinessAttributeNode attribute);
    public EnrichmentResult? TryEnrich(BusinessAttributeNode attribute);
}
```

### IBusinessTranslator

```csharp
public interface IBusinessTranslator
{
    TypeModel Translate(BusinessAttributeNode attribute);
    EnrichmentResult? TryEnrich(BusinessAttributeNode attribute);
}
```

### ProjectionReadService

```csharp
public class ProjectionReadService
{
    public ProjectionReadService(ReplayEngine replayEngine, IBusinessTranslator translator);
    public ProjectionView GetProjection(CommandLogStore logStore);
    public ProjectionView GetProjection(BusinessAuthoringDocument document);
}
```

### WriteBackService

```csharp
public class WriteBackService
{
    public WriteBackService(PatchEngine patchEngine);
    public void ApplyEnrichment(BusinessAuthoringDocument document, string entityId, EnrichmentResult enrichment);
    public void ApplyEnrichmentWithLog(BusinessAuthoringDocument document, string entityId, EnrichmentResult enrichment);
}
```

## Vazba na ostatní vrstvy

| Závislost | Směr | Účel |
|-----------|------|-------|
| `PatchEngine` | Translator → BusinessModel | Write path |
| `CommandLogStore` | Translator → BusinessModel | Write path |
| `ProjectionReadService` | Translator → BusinessModel | Read path |
| `CatalogManager` | Translator → Core | Resolve presetů při překladu |
| `DefaultBusinessTranslator` | Translator → Core | Překlad BusinessAttributeNode na TypeModel |

## Anti-patterny

- ❌ Host surface volající PatchEngine přímo (mimo Facade)
- ❌ Business logika v host vrstvě
- ❌ Facade závislá na konkrétním host surface
- ❌ Write-back obcházející PatchEngine

## Výstupní checklist

- [ ] Facade je jediný entry point pro host surfaces
- [ ] Write path jde přes PatchEngine → CommandLog
- [ ] Read path jde přes ProjectionReadService (replay)
- [ ] Facade neobsahuje reference na CLI/MCP/WebApi
- [ ] Translator je deterministický (AI je volitelný overlay)
- [ ] Enrichment prochází write-back servisou
