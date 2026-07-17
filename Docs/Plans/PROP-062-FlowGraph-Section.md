# PROP-062: FlowGraphSection — Derived Flow Visualization

**Status:** Proposed
**Owner:** Utalag
**Date:** 2026-07-18
**Layer:** Translator (Projections)
**Priority:** 🟡 Vysoká
**Estimate:** 1–2 dny
**Depends on:** PROP-056 (DocumentProjection — hotovo)
**Replaces:** Explicitní workflow model (PROP-063)

---

## 1. Context

Workflow je dnes explicitní součást `BusinessAuthoringDocument` (PROP-020), ale nemá authoring use-case, projekci, ani napojení na doménový model. Místo samostatného workflow modelu dává větší smysl odvozovat flow pohled z entit a relací, které v modelu reálně žijí.

`DocumentProjection` (PROP-056) už dnes poskytuje `Entities` a `Relations` sekce. Přidáním `FlowGraphSection` (nody + hrany) lze z projekce odvodit grafovou strukturu pro vizualizaci typu Obsidian-graph / JsonCrack — bez nutnosti samostatného workflow modelování.

## 2. Decision

Přidat `FlowGraphSection` do `DocumentProjection` jako read-only derivát z business modelu:
- **Nody**: entity (MVP). Behaviors jako nody → future.
- **Hrany**: relace mezi entitami. Invokes hrany → future.
- **FlowGraph je READ-ONLY** — nikdy se nezapisuje zpět do `BusinessAuthoringDocument`.
- **MVP stačí JSON výstup** pro JsonCrack — query engine (path finding, highlighting) je samostatná future vrstva.

## 3. Scope — Fáze 1: Datový model

### 3.1 FlowGraphSection.cs (nový soubor)

`Src/MetaForge.Translator/Projections/FlowGraphSection.cs`:

```csharp
public sealed record FlowGraphSection
{
    public IReadOnlyList<FlowNode> Nodes { get; init; } = [];
    public IReadOnlyList<FlowEdge> Edges { get; init; } = [];
}

public sealed record FlowNode
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public FlowNodeKind Kind { get; init; }
    public string? ParentEntityId { get; init; }  // pro Behavior nody (future)
}

public sealed record FlowEdge
{
    public string FromId { get; init; } = "";
    public string ToId { get; init; } = "";
    public FlowEdgeKind Kind { get; init; }
    public string? Label { get; init; }       // RelationType, behavior name
    public string? Condition { get; init; }   // future: z kontraktů/invariantů
}

public enum FlowNodeKind { Entity = 0, Behavior = 1 }

public enum FlowEdgeKind { Relation = 0, Invokes = 1 }
```

### 3.2 DocumentProjection.cs (modifikace)

Přidat property:
```csharp
public FlowGraphSection? FlowGraph { get; init; }
```

### 3.3 ProjectionFilter.cs (modifikace)

Přidat property:
```csharp
public bool IncludeFlowGraph { get; init; }
```

Přidat preset do `ProjectionPresets`:
```csharp
public static ProjectionFilter FlowGraph => new()
{
    IncludeRelations = true,
    IncludeFlowGraph = true,
};
```

## 4. Scope — Fáze 2: Builder

### 4.1 FlowGraphBuilder.cs (nový soubor)

`Src/MetaForge.Translator/Projections/FlowGraphBuilder.cs`:

```csharp
public static class FlowGraphBuilder
{
    public static FlowGraphSection Build(BusinessAuthoringDocument document)
    {
        var nodes = new List<FlowNode>();
        var edges = new List<FlowEdge>();

        foreach (var entity in document.Entities)
        {
            nodes.Add(new FlowNode
            {
                Id = entity.Id,
                Name = entity.Name,
                Kind = FlowNodeKind.Entity
            });
        }

        foreach (var relation in document.Relations)
        {
            edges.Add(new FlowEdge
            {
                FromId = relation.FromEntityId,
                ToId = relation.ToEntityId,
                Kind = FlowEdgeKind.Relation,
                Label = relation.RelationType
            });
        }

        return new FlowGraphSection { Nodes = nodes, Edges = edges };
    }
}
```

### 4.2 ProjectionBuilder.cs (modifikace)

V `Build()` metodě přidat:
```csharp
if (filter.IncludeFlowGraph)
{
    projection = projection with { FlowGraph = FlowGraphBuilder.Build(document) };
}
```

## 5. Scope — Fáze 3: Testy

`Tests/MetaForge.Translator.Tests/Projections/FlowGraphSectionTests.cs`:

| Test | Popis |
|------|-------|
| `Build_EmptyDocument_ReturnsEmptyGraph` | Prázdný dokument → 0 nodů, 0 hran |
| `Build_EntitiesOnly_ReturnsNodesWithoutEdges` | 3 entity bez relací → 3 nody, 0 hran |
| `Build_EntitiesWithRelations_ReturnsNodesAndEdges` | Entity + relace → správné nody a hrany |
| `Build_RelationTypes_ArePreservedInEdgeLabels` | `RelationType` se propsuje do `Label` |
| `Build_WithFilter_OnlyWhenIncludeFlowGraphTrue` | FlowGraph se staví jen když filtr povolí |

Použít existující `TestDocumentBuilder` pattern.

## 6. Scope — Fáze 4: Dokumentace

- `New_Architecture/08-Translator.md` — přidat popis `FlowGraphSection`
- `New_Architecture/00-Index.md` — přidat odkaz

## 7. Verifikace

1. `dotnet test Tests/MetaForge.Translator.Tests/` — všechny testy projdou
2. Vygenerovat JSON projekci s `IncludeFlowGraph = true` — ověřit `flowGraph` sekci
3. Otevřít JSON v JsonCrack — vizuálně ověřit

## 8. Relevantní soubory

| Akce | Soubor |
|------|--------|
| **Nový** | `Src/MetaForge.Translator/Projections/FlowGraphSection.cs` |
| **Nový** | `Src/MetaForge.Translator/Projections/FlowGraphBuilder.cs` |
| **Nový** | `Tests/MetaForge.Translator.Tests/Projections/FlowGraphSectionTests.cs` |
| **Mod** | `Src/MetaForge.Translator/Projections/DocumentProjection.cs` — přidat `FlowGraph` |
| **Mod** | `Src/MetaForge.Translator/Projections/ProjectionFilter.cs` — přidat `IncludeFlowGraph` + preset |
| **Mod** | `Src/MetaForge.Translator/Projections/ProjectionBuilder.cs` — volat `FlowGraphBuilder` |

## 9. Decisions

- MVP: Entity nody + Relation hrany. Behavior nody a Invokes hrany jsou `Future`.
- FlowGraph je READ-ONLY derivát. Nikdy se nezapisuje zpět.
- `Condition` na hranách zůstává `string?` — navázání na kontrakty/invarianty je future.
- Nepřidává se query engine (path finding, highlighting) — future vrstva.

## 10. Risks

- **Nízké:** Staví na existující infrastruktuře (`DocumentProjection`, `ProjectionBuilder`)
- **Nízké:** Nemění write model — pouze rozšiřuje read path
- **Nízké:** Žádné breaking changes v existujících typech (jen přidání optional property)

## 11. Související

- PROP-056: DocumentProjection (základ, na kterém to stojí)
- PROP-063: Odstranění starého workflow modelu (závisí na tomto PROP)
- IDEA-015: Workflow Projection & Write-Back (SUPERSEDED)
