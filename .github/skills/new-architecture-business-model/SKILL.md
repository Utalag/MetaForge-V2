---
name: new-architecture-business-model
description: "Pouzij pri: praci s BusinessModel vrstvou — BusinessAuthoringDocument, CommandLogStore, PatchEngine, ReplayEngine, BusinessEntityNode, BusinessRelationNode, PendingQuestionNode, CustomTypeDefinition."
---

# new-architecture-business-model

Zajistit konzistentní implementaci BusinessModel vrstvy dle `07-BusinessModel.md`. Hlídat invarianty CommandLog, PatchEngine a ReplayEngine.

## Kdy použít

- Při práci se soubory v `Src/MetaForge.BusinessModel/`
- Při implementaci BusinessAuthoringDocument, CommandLogStore, PatchEngine
- Při implementaci ReplayEngine, BusinessEntityNode, relací
- Při práci s PendingQuestions a CustomTypes

## Invarianty (neporušitelné)

| # | Invariant | Důsledek |
|---|-----------|----------|
| 1 | **BusinessAuthoringDocument je source of truth** | Veškerý stav systému je odvoditelný z tohoto dokumentu |
| 2 | **CommandLog je append-only** | Historie změn se nikdy nemaže ani nepřepisuje. `Count` nikdy neklesá. |
| 3 | **Replay je autoritativní rekonstrukce** | Stav se rekonstruuje přehráním commandů, ne čtením cache |
| 4 | **Žádná přímá mutace dokumentu** | Každá změna MUSÍ projít přes PatchEngine |
| 5 | **SchemaVersion musí být konzistentní** | Při replayi se kontroluje shoda verze |

## Klíčové typy

### BusinessAuthoringDocument

```csharp
public class BusinessAuthoringDocument
{
    public string ProjectName { get; set; }
    public string SchemaVersion { get; set; } = "1.0";
    public DateTime LastModified { get; set; }
    public List<BusinessEntityNode> Entities { get; } = new();
    public List<BusinessRelationNode> Relations { get; } = new();
    public List<CustomTypeDefinition> CustomTypes { get; } = new();
    public List<PendingQuestionNode> PendingQuestions { get; } = new();
}
```

### CommandLogStore

```csharp
public class CommandLogStore
{
    public void Append(CommandEnvelope envelope) { }
    public IReadOnlyList<CommandEnvelope> GetAll() { }
    public CommandEnvelope? GetAt(int index) { }
    public IReadOnlyList<CommandEnvelope> GetFrom(int startIndex) { }
    public int Count { get; }
}
```

### CommandEnvelope

```csharp
public sealed record CommandEnvelope
{
    public string Id { get; init; }
    public DateTime Timestamp { get; init; }
    public string CommandType { get; init; }
    public string? TargetEntityId { get; init; }
    public string? TargetAttributeId { get; init; }
    public string Payload { get; init; }
    public string SchemaVersion { get; init; } = "1.0";
}
```

### PatchEngine

```csharp
public class PatchEngine
{
    public PatchEngine(CommandLogStore logStore) { }
    public void Apply(BusinessAuthoringDocument document, IPatchOperation operation) { }
    public CommandEnvelope CreateEnvelope(IPatchOperation operation) { }
}
```

### ReplayEngine

```csharp
public class ReplayEngine
{
    public BusinessAuthoringDocument Replay(IReadOnlyList<CommandEnvelope> commands) { }
    public void ReplayFrom(BusinessAuthoringDocument document, IReadOnlyList<CommandEnvelope> commands, int startIndex) { }
}
```

### Patch operace

| Operace | CommandType | Účel |
|---------|-------------|------|
| `AddEntityOp` | AddEntity | Přidá novou entitu |
| `UpdateEntityOp` | UpdateEntity | Přejmenuje entitu |
| `DeleteEntityOp` | DeleteEntity | Smaže entitu + relace |
| `AddAttributeOp` | AddAttribute | Přidá atribut k entitě |

## Workflow

```
Command → PatchEngine.Apply() → CommandLogStore.Append() → ReplayEngine.Replay() → dokument
```

## Anti-patterny

- ❌ Přímá mutace BusinessAuthoringDocument properties mimo PatchEngine
- ❌ Mazání nebo úprava commandů v CommandLog
- ❌ CommandLogStore závislý na BusinessAuthoringDocument
- ❌ Validace až po mutaci (validace musí proběhnout před Apply)

## Výstupní checklist

- [ ] BusinessAuthoringDocument není přímo mutován
- [ ] CommandLog je append-only (žádný delete/update)
- [ ] Každá mutace prošla přes PatchEngine
- [ ] Replay je deterministický — stejný log = stejný dokument
- [ ] SchemaVersion je konzistentní
