# PROP-056: Sjednocení projekcí + Structured JSON Snapshot

> **Stav:** 📝 Navrženo (revidováno 2026-07-17 — CoreId synergie)
> **Datum:** 2026-07-14
> **Poslední revize:** 2026-07-17
> **Oblast:** Translator, BusinessModel, Host Surfaces
> **Odhad:** ~2.5 dne
> **Zdroj:** IDEA-011 (Snapshot Testing + Playground/REPL)
> **Závislosti:**
> - **PROP-060** (Element Identity Stabilization) — 🔴 HARD. `DocumentProjection` používá `ElementIdMapping` pro `CoreId` pole.
> - **PROP-055** (ReferenceGraph) — 🟡 SOFT. `DependencyGraphSection` je volitelná sekce projekce konzumující `ReferenceGraph`.

## Cíl

Nahradit duplicitní `ProjectionView` (basic) a `ExpertProjectionView` (expert) jedním unifikovaným `DocumentProjection` typem s řiditelnou úrovní detailu přes `ProjectionFilter`. Přidat schopnost serializovat projekci do JSON (`ToJson(filter)`) pro:

1. **AI enrichment kontext** — strukturovaný JSON vstup místo string interpolation, včetně stabilních CoreId
2. **CLI / MCP inspect** — `metaforge inspect --json --view expert`
3. **Custom views** — uživatelem definované filtry co chtějí vidět
4. **Pročištění codebase** — odstranění duplicitního kódu po refaktoringu
5. **Traceabilita Business → Core** — `CoreId` (Guid) z PROP-060 vedle `Id` (string)
6. **Graf závislostí** — `DependencyGraphSection` z PROP-055 jako volitelná sekce

---

## Motivace

### Dnešní problém

V `MetaForge.Translator.Host` existují **dva oddělené projekční typy** se **dvěma oddělenými mappingy**.
Navíc **chybí CoreId** — projekce obsahuje jen BusinessModel `string Id`, ale žádné mapování na Core `Guid Id` (PROP-060). To znamená, že z projekce nelze zjistit, který `ClassElement` odpovídá které `BusinessEntityNode`.

| Aspekt | `ProjectionView` | `ExpertProjectionView` |
|--------|-----------------|----------------------|
| Typ | mutable class | immutable record |
| Data | entity + attributes | vše + relations, behaviors, questions, diagnostics |
| Options | žádné | `ProjectionOptions` (Expert, Workflow, ...) |
| Mapping | `BuildProjection()` | `GetExpertProjection()` |
| Umístění | stejný soubor | separátní soubor |

Když se přidá nová property na `BusinessEntityNode`, musí se mapovat na dvou místech.

### AI enrichment používá stringy

`DefaultBusinessTranslator.TryEnrichAsync()` posílá AI modelu pouze:

```
Atribut: {attribute.Name}
Typ: {attribute.Type}
Siblingové: {string.Join(", ", siblingNames)}
```

Místo strukturovaného JSON kontextu celé entity. AI dostává ochuzený vstup, přestože má vracet strukturovaný JSON výstup (`SemanticBriefJson`).

### Chybí custom views

Dnes nelze říct "chci vidět jen atributy se syncState" nebo "zobraz jen názvy a typy bez metadat".

---

## Návrh

### 1. `DocumentProjection` — unifikovaný projekční typ

Nahradí `ProjectionView` i `ExpertProjectionView`. Obsahuje **všechna data**, která BusinessModel umí poskytnout — filtr rozhodne, co se dostane na výstup.

```csharp
// Src/MetaForge.Translator/Projections/DocumentProjection.cs

public sealed record DocumentProjection
{
    // === Základní info (vždy) ===
    public string SchemaVersion { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public ProjectInfo Project { get; init; } = new();
    public IReadOnlyList<EntityProjection> Entities { get; init; } = [];

    // === Rozšířené sekce (podle filtru) ===
    public IReadOnlyList<RelationProjection> Relations { get; init; } = [];
    public IReadOnlyList<BehaviorProjection> Behaviors { get; init; } = [];
    public IReadOnlyList<PendingQuestionProjection> PendingQuestions { get; init; } = [];
    public ProjectionDiagnostics Diagnostics { get; init; } = new();

    // === PROP-055 synergie — graf závislostí ===
    public DependencyGraphSection? DependencyGraph { get; init; }
}

public sealed record EntityProjection
{
    /// <summary>BusinessModel ID (string, např. "a1b2c3d4").</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Core ID (Guid) — z ElementIdMapping (PROP-060). Null = dosud nepřeloženo.</summary>
    public Guid? CoreId { get; init; }

    public string Name { get; init; } = string.Empty;
    public string? PresetId { get; init; }
    public IReadOnlyList<AttributeProjection> Attributes { get; init; } = [];
    public IReadOnlyList<BehaviorProjection> Behaviors { get; init; } = [];
    public int NoteCount { get; init; }
}

public sealed record AttributeProjection
{
    /// <summary>BusinessModel ID (string).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Core ID (Guid) — z ElementIdMapping (PROP-060). Null = dosud nepřeloženo.</summary>
    public Guid? CoreId { get; init; }

    public string Name { get; init; } = string.Empty;
    public string BusinessType { get; init; } = "string";
    public string? CoreType { get; init; }
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public string? DefaultValue { get; init; }

    // === Rozšířené (podle filtru) ===
    public CoreDetailInfo? CoreDetail { get; init; }
    public AttributeSyncState SyncState { get; init; } = AttributeSyncState.New;
    public IReadOnlyList<string> Constraints { get; init; } = [];
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}

// === Podpůrné typy ===

public sealed record ProjectInfo
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public int Version { get; init; }
}

public sealed record RelationProjection
{
    public string FromEntityId { get; init; } = string.Empty;
    public string ToEntityId { get; init; } = string.Empty;

    /// <summary>Core ID zdrojové entity — z ElementIdMapping (PROP-060).</summary>
    public Guid? FromEntityCoreId { get; init; }

    /// <summary>Core ID cílové entity — z ElementIdMapping (PROP-060).</summary>
    public Guid? ToEntityCoreId { get; init; }

    public string RelationType { get; init; } = string.Empty;
    public string? NavigationName { get; init; }
}

public sealed record BehaviorProjection
{
    /// <summary>BusinessModel ID (string).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Core ID (Guid) — z ElementIdMapping (PROP-060). Null = dosud nepřeloženo.</summary>
    public Guid? CoreId { get; init; }

    public string Name { get; init; } = string.Empty;
    public string? ReturnType { get; init; }
    public IReadOnlyList<string> Parameters { get; init; } = [];
    public IReadOnlyList<string> Constraints { get; init; } = [];
}

public sealed record PendingQuestionProjection
{
    public string Id { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string? ContextEntityId { get; init; }
    public bool IsBlocking { get; init; }
}

public sealed record ProjectionDiagnostics
{
    public int TotalAttributes { get; init; }
    public int WithConstraints { get; init; }
    public int StrongTypes { get; init; }
    public int PresetsUsed { get; init; }
    public int UnsyncedAttributes { get; init; }
    public DateTimeOffset BuiltAt { get; init; } = DateTimeOffset.UtcNow;
}
```

### 2. `ProjectionFilter` — řiditelná úroveň detailu

Namísto `ProjectionOptions` (basic/expert bool flagy) zavést plnohodnotný filtr, který řídí každou sekci.

```csharp
// Src/MetaForge.Translator/Projections/ProjectionFilter.cs

public sealed record ProjectionFilter
{
    // === Sekce ===
    public bool IncludeRelations { get; init; }
    public bool IncludeBehaviors { get; init; }
    public bool IncludeDiagnostics { get; init; }
    public bool IncludePendingQuestions { get; init; }
    public bool IncludeNotes { get; init; }

    // === Úroveň detailu atributu ===
    public AttributeDetailLevel AttributeDetail { get; init; } = AttributeDetailLevel.NameAndType;

    // === Enrichment data ===
    public bool IncludeCoreDetail { get; init; }
    public bool IncludeSyncState { get; init; }
    public bool IncludeConstraints { get; init; }
    public bool IncludeMetadata { get; init; }
    public bool IncludeCoinCost { get; init; }

    // === CoreId (PROP-060 synergie) ===
    public bool IncludeCoreIds { get; init; }

    // === DependencyGraph (PROP-055 synergie) ===
    public bool IncludeDependencyGraph { get; init; }
}

public enum AttributeDetailLevel
{
    NameOnly,          // jen název atributu
    NameAndType,       // + businessType, coreType
    WithValidation,    // + isRequired, maxLength, defaultValue, constraints
    Full,              // + coreDetail, syncState, metadata, coin
}
```

### 3. Presety filtrů

```csharp
// Src/MetaForge.Translator/Projections/ProjectionPresets.cs

public static class ProjectionPresets
{
    /// <summary>Základní projekce pro běžné zobrazení.</summary>
    public static ProjectionFilter Basic => new();

    /// <summary>Plná projekce pro vývojáře a diagnostiku.</summary>
    public static ProjectionFilter Expert => new()
    {
        IncludeRelations = true,
        IncludeBehaviors = true,
        IncludeDiagnostics = true,
        IncludePendingQuestions = true,
        IncludeNotes = true,
        IncludeCoreDetail = true,
        IncludeSyncState = true,
        IncludeConstraints = true,
        IncludeMetadata = true,
        IncludeCoinCost = true,
        IncludeCoreIds = true,
        IncludeDependencyGraph = true,
        AttributeDetail = AttributeDetailLevel.Full,
    };

    /// <summary>Pro AI enrichment — typy, validace, kontext + CoreId pro traceabilitu.</summary>
    public static ProjectionFilter AiEnrichment => new()
    {
        IncludeCoreDetail = true,
        IncludeConstraints = true,
        IncludeCoreIds = true,
        AttributeDetail = AttributeDetailLevel.WithValidation,
    };

    /// <summary>Custom view — definuje uživatel v konfiguraci nebo přes CLI flagy.</summary>
    public static ProjectionFilter Custom(Action<ProjectionFilter> configure)
    {
        var filter = new ProjectionFilter();
        configure(filter);
        return filter;
    }
}
```

### 4. `ToJson(filter)` — serializace projekce

```csharp
// Src/MetaForge.Translator/Projections/ProjectionSerializer.cs

public static class ProjectionSerializer
{
    /// <summary>
    /// Serializuje projekci do JSON s respektováním filtru.
    /// Filtr řídí, které sekce a jaký detail se zahrne.
    /// </summary>
    public static string ToJson(this DocumentProjection projection, ProjectionFilter filter)
    {
        var json = new JsonObject();

        // Základní info (vždy)
        json["schemaVersion"] = projection.SchemaVersion;
        json["projectName"] = projection.ProjectName;

        // Entities (vždy)
        json["entities"] = new JsonArray(
            projection.Entities.Select(e => SerializeEntity(e, filter)).ToArray()
        );

        // Relations (volitelné)
        if (filter.IncludeRelations && projection.Relations.Count > 0)
            json["relations"] = new JsonArray(
                projection.Relations.Select(SerializeRelation).ToArray()
            );

        // Behaviors (volitelné)
        if (filter.IncludeBehaviors && projection.Behaviors.Count > 0)
            json["behaviors"] = new JsonArray(
                projection.Behaviors.Select(SerializeBehavior).ToArray()
            );

        // Diagnostics (volitelné)
        if (filter.IncludeDiagnostics)
            json["diagnostics"] = SerializeDiagnostics(projection.Diagnostics);

        // Pending questions (volitelné)
        if (filter.IncludePendingQuestions && projection.PendingQuestions.Count > 0)
            json["pendingQuestions"] = new JsonArray(
                projection.PendingQuestions.Select(SerializeQuestion).ToArray()
            );

        return json.ToString();
    }

    // Interní serializační metody respektují AttributeDetailLevel
    private static JsonObject SerializeEntity(EntityProjection entity, ProjectionFilter filter) { ... }
    private static JsonObject SerializeAttribute(AttributeProjection attr, ProjectionFilter filter) { ... }
    // ...
}
```

```csharp
private static JsonObject SerializeAttribute(AttributeProjection attr, ProjectionFilter filter)
{
    var json = new JsonObject
    {
        ["name"] = attr.Name,
    };

    // CoreId (PROP-060 synergie — vždy pokud je k dispozici a filtr to chce)
    if (filter.IncludeCoreIds && attr.CoreId is not null)
        json["coreId"] = attr.CoreId.Value.ToString();

    if (filter.AttributeDetail >= AttributeDetailLevel.NameAndType)
    {
        json["businessType"] = attr.BusinessType;
        if (attr.CoreType is not null)
            json["coreType"] = attr.CoreType;
    }

    if (filter.AttributeDetail >= AttributeDetailLevel.WithValidation)
    {
        json["isRequired"] = attr.IsRequired;
        if (attr.MaxLength is not null) json["maxLength"] = attr.MaxLength.Value;
        if (attr.DefaultValue is not null) json["defaultValue"] = attr.DefaultValue;

        if (filter.IncludeConstraints && attr.Constraints.Count > 0)
            json["constraints"] = new JsonArray(attr.Constraints.Select(c => (JsonNode)c).ToArray());
    }

    if (filter.AttributeDetail >= AttributeDetailLevel.Full)
    {
        if (filter.IncludeCoreDetail && attr.CoreDetail is not null)
            json["coreDetail"] = SerializeCoreDetail(attr.CoreDetail);

        if (filter.IncludeSyncState)
            json["syncState"] = attr.SyncState.ToString();

        if (filter.IncludeMetadata && attr.Metadata is not null)
            json["metadata"] = SerializeMetadata(attr.Metadata);
    }

    return json;
}
```

### 5. Custom views — uživatelská konfigurace

Custom view definované v `.metaforge/views.json`:

```json
{
  "sync-overview": {
    "includeRelations": false,
    "includeSyncState": true,
    "includeDiagnostics": true,
    "attributeDetail": "nameAndType"
  },
  "constraints-audit": {
    "includeConstraints": true,
    "includeCoreDetail": true,
    "includeCoreIds": true,
    "attributeDetail": "withValidation"
  },
  "code-traceability": {
    "includeCoreIds": true,
    "includeDependencyGraph": true,
    "attributeDetail": "nameOnly"
  }
}
```

### 6. `DependencyGraphSection` — PROP-055 synergie

```csharp
// Src/MetaForge.Translator/Projections/DependencyGraphSection.cs

/// <summary>
/// Projekce referenčního grafu (PROP-055).
/// Zobrazuje závislosti mezi elementy pro CLI/MCP inspect.
/// </summary>
public sealed record DependencyGraphSection
{
    public int NodeCount { get; init; }
    public int EdgeCount { get; init; }
    public bool HasCycles { get; init; }
    public IReadOnlyList<DependencyNodeProjection> Nodes { get; init; } = [];
    public IReadOnlyList<CycleProjection> Cycles { get; init; } = [];
    public IReadOnlyList<UnresolvedProjection> Unresolved { get; init; } = [];
}

public sealed record DependencyNodeProjection
{
    public Guid ElementId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string ElementKind { get; init; } = string.Empty;
    public int InDegree { get; init; }
    public int OutDegree { get; init; }
}

public sealed record CycleProjection
{
    public IReadOnlyList<string> DisplayNames { get; init; } = [];
}

public sealed record UnresolvedProjection
{
    public string SourceDisplayName { get; init; } = string.Empty;
    public string ReferencedAs { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
}
```

### 7. ProjectionBuilder — integrace ElementIdMapping

```csharp
// Src/MetaForge.Translator/Projections/ProjectionBuilder.cs

public sealed class ProjectionBuilder
{
    private readonly IBusinessTranslator _translator;
    private readonly ElementIdMapping? _idMapping;  // PROP-060 — volitelné

    public ProjectionBuilder(IBusinessTranslator translator, ElementIdMapping? idMapping = null)
    {
        _translator = translator;
        _idMapping = idMapping;
    }

    public DocumentProjection Build(BusinessAuthoringDocument document, ProjectionFilter filter)
    {
        var projection = new DocumentProjection { ... };

        foreach (var entity in document.Entities)
        {
            var entityProj = new EntityProjection
            {
                Id = entity.Id,
                CoreId = _idMapping?.ResolveBusinessId(entity.Id),  // PROP-060
                Name = entity.Name,
            };

            foreach (var attr in entity.Attributes)
            {
                var attrProj = new AttributeProjection
                {
                    Id = attr.Id,
                    CoreId = _idMapping?.ResolveBusinessId(attr.Id),  // PROP-060
                    Name = attr.Name,
                };
            }
        }

        // PROP-055 synergie
        if (filter.IncludeDependencyGraph && _referenceGraph is not null)
        {
            projection.DependencyGraph = BuildDependencyGraphSection(_referenceGraph);
        }

        return projection;
    }
}
```

Použití v CLI:

```bash
# Built-in presety
metaforge inspect                              # basic
metaforge inspect --view expert                 # expert
metaforge inspect --view ai-enrichment          # pro AI kontext

# Custom views
metaforge inspect --view sync-overview          # z views.json
metaforge inspect --view my-custom-view

# Inline flagy (přepíší preset)
metaforge inspect --view expert --include-coin-cost=false --attr-detail=nameAndType

# Výstup do souboru
metaforge inspect --view expert --output model.json
```

### 8. Zapojení do AI enrichmentu

`DefaultBusinessTranslator.TryEnrichAsync()` místo string interpolation:

```csharp
// Dnes:
var userPrompt = AuthoringTranslationModelPrompt.BuildUserPrompt(
    attribute.Name, attribute.Type, siblingAttributes, entityName);

// Zítra:
var entityProj = projection.Entities.First(e => e.Attributes.Any(a => a.Id == attribute.Id));
var contextJson = entityProj.ToJson(ProjectionPresets.AiEnrichment);
// contextJson nyní obsahuje i "coreId" pro každý atribut — AI vidí provázání na vygenerovaný kód
var userPrompt = $"""
    Analyzuj atribut v kontextu entity.

    Kontext (JSON včetně coreId pro traceabilitu):
    {contextJson}

    Vrať POUZE JSON podle schématu.
    """;
```

---

## Refactoring — co se mění

### Nové soubory (v `Src/MetaForge.Translator/Projections/`)

| Soubor | Obsah |
|--------|-------|
| `DocumentProjection.cs` | Hlavní projekční typ + EntityProjection, AttributeProjection, BehaviorProjection, RelationProjection, PendingQuestionProjection, ProjectionDiagnostics |
| `ProjectionFilter.cs` | Filtr + `AttributeDetailLevel` enum |
| `ProjectionPresets.cs` | Basic / Expert / AiEnrichment / Custom presety |
| `ProjectionSerializer.cs` | `ToJson(filter)` extension metoda — serializuje včetně CoreId |
| `ProjectionBuilder.cs` | Logika z `ProjectionReadService.BuildProjection()` sem — konzumuje `ElementIdMapping` (PROP-060) |
| `DependencyGraphSection.cs` | Projekce grafu závislostí (PROP-055 synergie) — `DependencyNodeProjection`, `CycleProjection`, `UnresolvedProjection` |

### Odstraněné soubory

| Starý soubor | Důvod | Náhrada |
|-------------|-------|---------|
| `Host/ProjectionView.cs` | Nahrazen `DocumentProjection` | `Projections/DocumentProjection.cs` |
| `Host/ExpertProjectionView.cs` | Nahrazen `DocumentProjection` | `Projections/DocumentProjection.cs` |
| `Host/ProjectionOptions.cs` | Nahrazen `ProjectionFilter` | `Projections/ProjectionFilter.cs` |
| `Prompting/ModelPrompts/AuthoringTranslationModelPrompt.cs` | String prompt nahrazen JSON kontextem | přímo v `DefaultBusinessTranslator.TryEnrichAsync` |
| `Prompting/SemanticBriefJson.cs` | Zůstává (výstupní schéma AI) | — |

### Upravené soubory

| Soubor | Změna |
|--------|-------|
| `Host/ProjectionReadService.cs` | `BuildProjection()` → deleguje na `ProjectionBuilder`. `GetProjection()` vrací `DocumentProjection`. `GetExpertProjection()` → odstraněn. Přidáno `GetElementIdMapping()` (PROP-060). Přidán `ReferenceGraph` parametr (PROP-055). |
| `Host/BusinessAuthoringHostFacade.cs` | Metody vracející `ProjectionView`/`ExpertProjection` → vrací `DocumentProjection`. Ukládá `ElementIdMapping` z posledního `TranslateDocument()`. |
| `Translation/DefaultBusinessTranslator.cs` | `TryEnrichAsync()` používá JSON kontext včetně CoreId. `TranslateDocument()` ukládá `ElementIdMapping` do Facade. |
| `Translation/ElementIdMapping.cs` | **Použito** (nový soubor z PROP-060) — `ProjectionBuilder` konzumuje `ResolveBusinessId()`. |
| `Prompting/SemanticBriefJson.cs` | Zůstává beze změny (výstupní schéma AI). |

### Pročištění codebase po refaktoringu

1. **Odstranit duplicitní mapping** v `ProjectionReadService` — dnes `BuildProjection()` a `GetExpertProjection()` mapují stejná data dvakrát
2. **Odstranit `ProjectionView`** a všechny jeho reference
3. **Odstranit `ExpertProjectionView`** a všechny jeho reference
4. **Odstranit `ProjectionOptions`** a nahradit `ProjectionFilter`
5. **Odstranit `AuthoringTranslationModelPrompt`** pokud už není používán jinde
6. **Aktualizovat testy** — všechny testy, které používají `ProjectionView` / `ExpertProjectionView`, přepsat na `DocumentProjection`
7. **Ověřit žádné zbylé artefakty** — grep na `ProjectionView`, `ExpertProjectionView`, `ProjectionOptions`, `AuthoringTranslationModelPrompt` — žádný výskyt mimo historii gitu

### Testy

| Testovací soubor | Úprava |
|-----------------|--------|
| `ProjectionReadServiceTests.cs` | `GetProjection()` vrací `DocumentProjection` |
| `ExpertProjectionTests.cs` | Expert je jen preset, testovat filtr |
| `DefaultBusinessTranslatorTests.cs` | `TryEnrichAsync` používá JSON kontext |
| Nové: `ProjectionFilterTests.cs` | Ověřit, že filtr správně omezuje data |
| Nové: `ProjectionSerializerTests.cs` | Ověřit JSON výstup pro basic/expert/custom |

---

## Sekvence kroků

```mermaid
graph TD
    A[0. PROP-060 — ElementIdMapping existuje] --> B[1. Vytvořit DocumentProjection + ProjectionFilter]
    B --> C[2. Implementovat ProjectionBuilder s ElementIdMapping]
    C --> D[3. Implementovat ProjectionSerializer.ToJson vč. CoreId]
    D --> E[4. Přepnout ProjectionReadService na nový typ]
    E --> F[5. Přepnout BusinessAuthoringHostFacade]
    F --> G[6. Zapojit JSON kontext do DefaultBusinessTranslator.TryEnrichAsync]
    G --> H[7. Přidat DependencyGraphSection (PROP-055)]
    H --> I[8. Přidat custom views .metaforge/views.json]
    I --> J[9. Odstranit staré soubory]
    J --> K[10. Pročistit codebase a reference]
    K --> L[11. Aktualizovat a přidat testy]
    L --> M[12. Ověřit build + testy]
```

---

## Otevřené otázky

1. **Zpětná kompatibilita** — `BusinessAuthoringHostFacade` má metody vracející `ProjectionView`. Mají host surfaces (CLI, MCP) přímou závislost na starých typech? Pokud ano, přidat adaptér nebo rovnou zlomit.
2. **Custom views storage** — JSON soubor v `.metaforge/views.json` nebo v projektu? Možná obojí.
3. **Format výstupu** — JSON je primární, ale `ToJson()` používá `System.Text.Json.Nodes` → YAML by šel přes konverzi (ale není v scope tohoto PROP).
4. **OQ-056-04: Co když `ElementIdMapping` ještě neexistuje?** — `ProjectionBuilder` akceptuje `ElementIdMapping?` jako optional. `CoreId` bude `null`. PROP-060 je HARD dependency, takže by se to nemělo stát, ale graceful fallback je správný.
5. **OQ-056-05: Má `IncludeCoreIds` být výchozí pro Basic?** — Ne. `CoreId` je implementační detail. Basic preset ho nezahrnuje. Jen Expert a AiEnrichment.

## Navazuje na

- **PROP-060** (Element Identity Stabilization) — 🔴 HARD. `CoreId` pole jsou populována z `ElementIdMapping.ResolveBusinessId()`.
- **PROP-055** (ReferenceGraph) — 🟡 SOFT. `DependencyGraphSection` je volitelná sekce konzumující `ReferenceGraph`.
- `IDEA-011` — původní koncept Snapshot Testing
- `ProjectionReadService.cs` — hlavní soubor k refaktorování
- `DefaultBusinessTranslator.cs` — AI enrichment začne používat JSON kontext
