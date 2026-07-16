# PROP-056: Sjednocení projekcí + Structured JSON Snapshot

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-14
> **Oblast:** Translator, BusinessModel, Host Surfaces
> **Odhad:** ~3 dny
> **Zdroj:** IDEA-011 (Snapshot Testing + Playground/REPL)

## Cíl

Nahradit duplicitní `ProjectionView` (basic) a `ExpertProjectionView` (expert) jedním unifikovaným `DocumentProjection` typem s řiditelnou úrovní detailu přes `ProjectionFilter`. Přidat schopnost serializovat projekci do JSON (`ToJson(filter)`) pro:

1. **AI enrichment kontext** — strukturovaný JSON vstup místo string interpolation
2. **CLI / MCP inspect** — `metaforge inspect --json --view expert`
3. **Custom views** — uživatelem definované filtry co chtějí vidět
4. **Pročištění codebase** — odstranění duplicitního kódu po refaktoringu

---

## Motivace

### Dnešní problém

V `MetaForge.Translator.Host` existují **dva oddělené projekční typy** se **dvěma oddělenými mappingy**:

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
}

public sealed record EntityProjection
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? PresetId { get; init; }
    public IReadOnlyList<AttributeProjection> Attributes { get; init; } = [];
    public IReadOnlyList<BehaviorProjection> Behaviors { get; init; } = [];
    public int NoteCount { get; init; }
}

public sealed record AttributeProjection
{
    public string Id { get; init; } = string.Empty;
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
    public string RelationType { get; init; } = string.Empty;
    public string? NavigationName { get; init; }
}

public sealed record BehaviorProjection
{
    public string Id { get; init; } = string.Empty;
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
        AttributeDetail = AttributeDetailLevel.Full,
    };

    /// <summary>Pro AI enrichment — typy, validace, kontext.</summary>
    public static ProjectionFilter AiEnrichment => new()
    {
        IncludeCoreDetail = true,
        IncludeConstraints = true,
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

Serializace atributu podle detail levelu:

```csharp
private static JsonObject SerializeAttribute(AttributeProjection attr, ProjectionFilter filter)
{
    var json = new JsonObject
    {
        ["name"] = attr.Name,
    };

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
    "attributeDetail": "withValidation"
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

### 6. Zapojení do AI enrichmentu

`DefaultBusinessTranslator.TryEnrichAsync()` místo string interpolation:

```csharp
// Dnes:
var userPrompt = AuthoringTranslationModelPrompt.BuildUserPrompt(
    attribute.Name, attribute.Type, siblingAttributes, entityName);

// Zítra:
var entityProj = projection.Entities.First(e => e.Attributes.Any(a => a.Id == attribute.Id));
var contextJson = entityProj.ToJson(ProjectionPresets.AiEnrichment);
var userPrompt = $"""
    Analyzuj atribut v kontextu entity.

    Kontext (JSON):
    {contextJson}

    Vrať POUZE JSON podle schématu.
    """;
```

---

## Refactoring — co se mění

### Nové soubory (v `Src/MetaForge.Translator/Projections/`)

| Soubor | Obsah |
|--------|-------|
| `DocumentProjection.cs` | Hlavní projekční typ + všechny podtypy |
| `ProjectionFilter.cs` | Filtr + `AttributeDetailLevel` |
| `ProjectionPresets.cs` | Basic / Expert / AiEnrichment / Custom |
| `ProjectionSerializer.cs` | `ToJson(filter)` extension metoda |
| `ProjectionBuilder.cs` | Logika z `ProjectionReadService.BuildProjection()` sem |

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
| `Host/ProjectionReadService.cs` | `BuildProjection()` → používá `ProjectionBuilder`. `GetProjection()` vrací `DocumentProjection` místo `ProjectionView`. `GetExpertProjection()` → odstraněn (použít `GetProjection(doc, ProjectionPresets.Expert)`) |
| `Host/BusinessAuthoringHostFacade.cs` | Metody vracející `ProjectionView` / `ExpertProjection` → vrací `DocumentProjection` |
| `Translation/DefaultBusinessTranslator.cs` | `TryEnrichAsync()` používá JSON kontext místo string promptu |
| `Prompting/SemanticBriefJson.cs` | Zůstává beze změny |

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
    A[1. Vytvořit DocumentProjection + ProjectionFilter] --> B[2. Implementovat ProjectionBuilder]
    B --> C[3. Implementovat ProjectionSerializer.ToJson]
    C --> D[4. Přepnout ProjectionReadService na nový typ]
    D --> E[5. Přepnout BusinessAuthoringHostFacade]
    E --> F[6. Zapojit do DefaultBusinessTranslator.TryEnrichAsync]
    F --> G[7. Přidat custom views (.metaforge/views.json)]
    G --> H[8. Odstranit staré soubory]
    H --> I[9. Pročistit codebase a reference]
    I --> J[10. Aktualizovat a přidat testy]
    J --> K[11. Ověřit build + testy]
```

---

## Otevřené otázky

1. **Zpětná kompatibilita** — `BusinessAuthoringHostFacade` má metody vracející `ProjectionView`. Mají host surfaces (CLI, MCP, WebApi) přímou závislost na starých typech? Pokud ano, přidat adaptér nebo rovnou zlomit.
2. **Custom views storage** — JSON soubor v `.metaforge/views.json` nebo v projektu? Možná obojí.
3. **Format výstupu** — JSON je primární, ale `ToJson()` používá `System.Text.Json.Nodes` → YAML by šel přes konverzi (ale není v scope tohoto PROP).
