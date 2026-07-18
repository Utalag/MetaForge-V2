# BusinessModel

> Event Sourcing, CommandLog, Document, Replay, Patches, Validace

**Aktualizace:** PROP-020 (2026-07-04) — immutable record modely, CoreDetail vrstvení, SyncState, CommandLog provenance, BusinessIdAllocator, validace. Workflow odstraněno PROP-063 (2026-07-18), nahrazeno FlowGraphSection (PROP-062).

---

## Struktura projektu

```
Src/MetaForge.BusinessModel/
├── Models/                          (19 souborů)
│   ├── BusinessAuthoringDocument.cs — hlavní dokument (sealed record)
│   ├── BusinessProjectInfo.cs       — strukturovaná metadata projektu
│   ├── BusinessEntityNode.cs        — entita s atributy, chováním, relacemi
│   ├── BusinessAttributeNode.cs     — atribut + CoreDetail (PROP-020)
│   ├── BusinessAttributeCoreDetail.cs — Core enrichment vrstva (PROP-020)
│   ├── BusinessBehaviorNode.cs      — chování (Query/Command/Rule)
│   ├── BusinessBehaviorKind.cs      — enum Query|Command|Rule
│   ├── BusinessRelationNode.cs      — relace mezi entitami
│   ├── BusinessRelationKind.cs      — enum BelongsTo|HasMany|HasOne|ManyToMany
│   ├── AttributeSyncState.cs        — enum New|Synced|BusinessEdited|CoreEdited|Conflict
│   ├── CoreInfoSource.cs            — enum Unknown|Manual|Generated|Hybrid
│   ├── BusinessNoteNode.cs          — poznámka k entitě
│   ├── PendingQuestionNode.cs       — otázka k doptání
│   └── CustomTypeDefinition.cs      — custom type definice
├── CommandLog/                      (6 souborů)
│   ├── CommandEnvelope.cs           — obálka commandu (sealed record)
│   ├── CommandSource.cs             — enum Chat|Cli|Mcp|Import|System|WebApi|Desktop
│   ├── CommandIssuedBy.cs           — kdo command vydal
│   ├── CommandProvenance.cs         — provenance (mode, model, confidence)
│   ├── CommandLogStore.cs           — append-only store s idempotencí
│   └── ReplayEngine.cs              — full + incremental replay
├── Patches/                         (9 souborů)
│   ├── IPatchOperation.cs           — interface (CommandType, Apply, ToEnvelope)
│   ├── PatchEngine.cs               — atomický engine pro mutace
│   └── Operations/
│       ├── AddEntityOp.cs           — přidání entity
│       ├── UpdateEntityOp.cs        — přejmenování entity
│       ├── DeleteEntityOp.cs        — smazání entity + relací
│       ├── AddAttributeOp.cs        — přidání atributu
│       ├── UpdateAttributeOp.cs     — úprava atributu
│       ├── SetCoreDetailOp.cs       — zápis CoreDetail (PROP-020)
│       └── UpdateSyncStateOp.cs     — změna SyncState (PROP-020)
├── Validation/                      (2 soubory)
│   ├── BusinessDocumentValidator.cs — validace dokumentu
│   └── BusinessValidationIssue.cs   — issue s kódem, cestou, závažností
└── Identity/                        (1 soubor)
    └── BusinessIdAllocator.cs       — lidsky čitelné ID (slugy)
```

---

## Modely — sealed record s immutabilitou

Všechny modely jsou **C# `sealed record` s `{ get; init; }`** syntaxí. Poskytuje:
- `with` výrazy — `document with { Name = "NewName" }` vytvoří novou instanci
- Automatický `Equals`/`GetHashCode` podle hodnot
- `ToString()` s výpisem všech properties pro debugging

### BusinessAuthoringDocument

```csharp
//context//
// Účel: Single source of truth pro celý business model.
// Vrstva: BusinessModel.
// Vstup: Vzniká replay z CommandLog nebo deserializací z JSON.
// Výstup: Kompletní stav business modelu pro projekci a export.
// Závislosti: BusinessEntityNode, BusinessRelationNode, CustomTypeDefinition.
// Nezávislosti: Nezávisí na Core ani na Translator — čistý doménový model.
// Invarianty: Nesmí být mutován přímo — pouze přes PatchEngine + CommandLog.
//             SchemaVersion musí být konzistentní. Všechny listy jsou IReadOnlyList.
// Související typy: CommandLogStore, ReplayEngine, PatchEngine, BusinessAuthoringHostFacade.
// Testy: BusinessModel.Tests/Models/BusinessAuthoringDocumentTests.cs.

public sealed record BusinessAuthoringDocument
{
    public const string CurrentSchemaVersion = "1.0";
    public string ProjectName { get; init; } = string.Empty;
    public BusinessProjectInfo Project { get; init; } = new();               // PROP-020
    public string SchemaVersion { get; init; } = CurrentSchemaVersion;
    public DateTime LastModified { get; init; } = DateTime.UtcNow;
    public IReadOnlyList<BusinessEntityNode> Entities { get; init; } = [];
    public IReadOnlyList<BusinessRelationNode> Relations { get; init; } = [];
    public IReadOnlyList<CustomTypeDefinition> CustomTypes { get; init; } = [];
    public IReadOnlyList<PendingQuestionNode> PendingQuestions { get; init; } = [];
    // Workflows removed PROP-063 (2026-07-18) — replaced by FlowGraphSection (PROP-062)
}
```

### BusinessProjectInfo

```csharp
// PROP-020: Strukturovaná metadata projektu namísto prostého ProjectName
public sealed record BusinessProjectInfo
{
    public string Id { get; init; } = "new-project";
    public string Name { get; init; } = "NewProject";
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public int Version { get; init; } = 1;
}
```

### BusinessEntityNode

```csharp
public sealed record BusinessEntityNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<BusinessAttributeNode> Attributes { get; init; } = [];
    public IReadOnlyList<BusinessBehaviorNode> Behaviors { get; init; } = [];
    public IReadOnlyList<BusinessRelationNode> Relations { get; init; } = [];
    public IReadOnlyList<BusinessNoteNode> Notes { get; init; } = [];
}
```

### BusinessAttributeNode + CoreDetail (PROP-020 klíčová změna)

```csharp
public sealed record BusinessAttributeNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "string";
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public string? MinValue { get; init; }
    public string? MaxValue { get; init; }
    public string? DefaultValue { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = new();
    public BusinessAttributeCoreDetail? CoreDetail { get; init; }  // PROP-020
}
```

### BusinessAttributeCoreDetail

```csharp
// PROP-020: Vrstva Core-konkretizovaných informací vedle uživatelského vstupu.
// Umožňuje dvouúrovňový authoring: UserInput → AI enrichment → CoreDetail.
public sealed record BusinessAttributeCoreDetail
{
    public CoreInfoSource Source { get; init; } = CoreInfoSource.Unknown;
    public string? ResolvedPresetId { get; init; }
    public string? ValueObjectName { get; init; }
    public bool IsStrongType { get; init; }
    public DateTimeOffset? LastSyncedAt { get; init; }

    [JsonIgnore]
    public AttributeSyncState SyncState { get; set; } = AttributeSyncState.New;
}
```

### AttributeSyncState + CoreInfoSource

```csharp
// PROP-020: Stav synchronizace mezi business a core vrstvou
public enum AttributeSyncState
{
    New = 0,
    Synced = 1,
    BusinessEdited = 2,
    CoreEdited = 3,
    Conflict = 4,
}

// PROP-020: Kdo/původ dat
public enum CoreInfoSource
{
    Unknown = 0,
    Manual = 1,
    Generated = 2,
    Hybrid = 3,
}
```

### BusinessBehaviorNode + BusinessBehaviorKind

```csharp
public sealed record BusinessBehaviorNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string ReturnType { get; init; } = "void";
    public BusinessBehaviorKind Kind { get; init; } = BusinessBehaviorKind.Command; // PROP-020
    public IReadOnlyList<BusinessParameterNode> Parameters { get; init; } = [];
}

// PROP-020: Typ-safe enum pro druhy chování
public enum BusinessBehaviorKind { Query = 0, Command = 1, Rule = 2 }

public sealed record BusinessParameterNode
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "string";
    public bool IsRequired { get; init; } = true;
    public string? DefaultValue { get; init; }
    public string? Summary { get; init; }
}
```

### BusinessRelationNode + BusinessRelationKind

```csharp
public sealed record BusinessRelationNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public string FromEntityId { get; init; } = string.Empty;
    public string ToEntityId { get; init; } = string.Empty;
    public string RelationType { get; init; } = "OneToMany";
    public BusinessRelationKind Kind { get; init; } = BusinessRelationKind.HasMany; // PROP-020
    public string? FromNavigationName { get; init; }
    public string? ToNavigationName { get; init; }
}

// PROP-020: Typ-safe enum pro druhy relací
public enum BusinessRelationKind { BelongsTo = 0, HasMany = 1, HasOne = 2, ManyToMany = 3 }
```

### Workflow modely — ODSTRANĚNO PROP-063 (2026-07-18)

> Workflow model (6 typů: BusinessWorkflowNode, BusinessWorkflowStepNode, BusinessWorkflowTransitionNode,
> BusinessWorkflowStepBindingDetail, BusinessWorkflowStepKind, WorkflowBindingSyncState) byl odstraněn.
> Náhrada: `FlowGraphSection` v `DocumentProjection` — odvozená grafová vizualizace z entit a relací (PROP-062).
> Poslední verze s workflow: tag `archive/workflow-last` (`be1c052`).

### BusinessNoteNode + PendingQuestionNode

```csharp
public sealed record BusinessNoteNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public string Text { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public sealed record PendingQuestionNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public string Question { get; init; } = string.Empty;
    public string? ContextEntityId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
```

### CustomTypeDefinition

```csharp
public sealed record CustomTypeDefinition
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; init; } = string.Empty;
    public string BaseType { get; init; } = "string";
    public IReadOnlyList<string> ValidationRules { get; init; } = [];
    public string? Description { get; init; }
}
```

---

## CommandLog

### CommandEnvelope (PROP-020 rozšíření)

```csharp
//context//
// Účel: Obálka commandu pro append-only log. Nese metadata o původu a provenienci.
// Vrstva: BusinessModel.
// Invarianty: Jakmile zapsán, neměnný. MutationId pro idempotenci.
// Související typy: CommandLogStore, ReplayEngine, PatchEngine.

public sealed record CommandEnvelope
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CommandType { get; init; } = string.Empty;
    public string? TargetEntityId { get; init; }
    public string? TargetAttributeId { get; init; }
    public string Payload { get; init; } = "{}";
    public string SchemaVersion { get; init; } = BusinessAuthoringDocument.CurrentSchemaVersion;

    // --- PROP-020 additions ---
    public string StreamId { get; init; } = "default";           // multi-tenant / multi-project
    public CommandSource Source { get; init; } = CommandSource.Unknown;  // Chat, Cli, MCP...
    public CoreInfoSource InfoSource { get; init; } = CoreInfoSource.Manual;
    public CommandIssuedBy IssuedBy { get; init; } = new();      // kdo vydal
    public CommandProvenance Provenance { get; init; } = new();  // AI model, confidence...
    public string? CorrelationId { get; init; }                  // pro tracing
    public string? CausationId { get; init; }                    // kauzální řetěz
    public string? MutationId { get; init; }                     // idempotence
}
```

### CommandSource, CommandIssuedBy, CommandProvenance

```csharp
// PROP-020: Zdroje commandů
public enum CommandSource { Unknown = 0, Chat = 1, Cli = 2, Mcp = 3, Import = 4, System = 5, WebApi = 6, Desktop = 7 }

// PROP-020: Kdo command vydal
public sealed class CommandIssuedBy
{
    public string ActorType { get; init; } = "user";  // "user", "ai", "system"
    public string? ActorId { get; init; }
    public string? DisplayName { get; init; }
}

// PROP-020: Provenience — důvod, AI model, confidence
public sealed class CommandProvenance
{
    public string Mode { get; init; } = "manual";  // "manual", "ai-assisted", "ai-generated", "import"
    public string? Reason { get; init; }
    public string? Model { get; init; }
    public double? Confidence { get; init; }
    public string? PromptVersion { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = new();
}
```

### CommandLogStore (PROP-020 idempotence)

```csharp
//context//
// Účel: Append-only úložiště commandů. Žádný command se nikdy nemaže ani nepřepisuje.
// Vrstva: BusinessModel.
// Vstup: CommandEnvelope z PatchEngine.
// Výstup: Sekvence commandů pro replay.
// Invarianty: APPEND-ONLY. Count nikdy neklesá.
// PROP-020: TryAppend s idempotencí podle MutationId.
// Související typy: CommandEnvelope, ReplayEngine, PatchEngine.
// Testy: BusinessModel.Tests/CommandLog/CommandLogStoreTests.cs.

public sealed class CommandLogStore
{
    public int Count { get; }
    public void Append(CommandEnvelope envelope);            // původní API
    public bool TryAppend(CommandEnvelope envelope);         // PROP-020: vrací false pokud MutationId duplicitní
    public IReadOnlyList<CommandEnvelope> GetAll();
    public CommandEnvelope? GetAt(int index);
    public IReadOnlyList<CommandEnvelope> GetFrom(int startIndex);  // PROP-020: inkrementální replay
}
```

### ReplayEngine

```csharp
//context//
// Účel: Rekonstrukce BusinessAuthoringDocument z command logu.
// Vrstva: BusinessModel.
// Vstup: Sekvence CommandEnvelope.
// Výstup: BusinessAuthoringDocument.
// PROP-020: Podpora SetCoreDetail, UpdateSyncState. Inkremenální replay.
// Související typy: CommandLogStore, CommandEnvelope, PatchEngine.
// Testy: BusinessModel.Tests/CommandLog/ReplayEngineTests.cs.

public sealed class ReplayEngine
{
    public BusinessAuthoringDocument Replay(IReadOnlyList<CommandEnvelope> commands);
    public BusinessAuthoringDocument ReplayFrom(BusinessAuthoringDocument document,
        IReadOnlyList<CommandEnvelope> commands, int startIndex);
}
```

Podporované command typy v replayi: `AddEntity`, `UpdateEntity`, `DeleteEntity`, `AddAttribute`, `UpdateAttribute`, `DeleteAttribute`, `AddRelation`, `SetCoreDetail`, `UpdateSyncState`.

---

## PatchEngine (PROP-020 immutable mutace)

```csharp
//context//
// Účel: Atomické mutace BusinessAuthoringDocument. Každá mutace vytváří CommandEnvelope do CommandLog.
// Vrstva: BusinessModel.
// Vstup: IPatchOperation od facade nebo hosta.
// Výstup: Nová instance dokumentu (immutable) + záznam v CommandLog.
// Závislosti: BusinessAuthoringDocument, CommandLogStore, IPatchOperation.
// Nezávislosti: Nezávisí na Translator — čistý doménový engine.
// Invarianty: Každá mutace MUSÍ projít přes PatchEngine. Přímá mutace dokumentu je zakázaná.
// PROP-020: Apply vrací nový dokument (immutable pattern). SetCoreDetailOp, UpdateSyncStateOp.
// Související typy: IPatchOperation, AddEntityOp, SetCoreDetailOp, BusinessAuthoringHostFacade.
// Testy: BusinessModel.Tests/Patches/PatchEngineTests.cs.

public sealed class PatchEngine
{
    public PatchEngine(CommandLogStore logStore);
    public BusinessAuthoringDocument Apply(BusinessAuthoringDocument document, IPatchOperation operation);
    public CommandEnvelope CreateEnvelope(IPatchOperation operation);
}
```

### IPatchOperation

```csharp
public interface IPatchOperation
{
    string CommandType { get; }
    BusinessAuthoringDocument Apply(BusinessAuthoringDocument document);  // vrací nový dokument
    CommandEnvelope ToEnvelope();
}
```

### Operace

| Operace | CommandType | Účel | PROP-020 |
|---------|-------------|------|----------|
| `AddEntityOp` | `AddEntity` | Přidá novou entitu | ✅ Immutable |
| `UpdateEntityOp` | `UpdateEntity` | Přejmenuje entitu | ✅ Immutable |
| `DeleteEntityOp` | `DeleteEntity` | Smaže entitu + relace | ✅ Immutable |
| `AddAttributeOp` | `AddAttribute` | Přidá atribut k entitě | ✅ Immutable |
| `UpdateAttributeOp` | `UpdateAttribute` | Upraví atribut | ✅ Immutable |
| `SetCoreDetailOp` | `SetCoreDetail` | Zápis CoreDetail na atribut | 🆕 Nové |
| `UpdateSyncStateOp` | `UpdateSyncState` | Změna AttributeSyncState | 🆕 Nové |

---

## Validace (PROP-020)

### BusinessDocumentValidator

```csharp
// PROP-020: Validace dokumentu s Error/Warning, path reference, suggestion.
// Testy: BusinessModel.Tests/Validation/BusinessDocumentValidatorTests.cs.

public sealed class BusinessDocumentValidator
{
    public IReadOnlyList<BusinessValidationIssue> Validate(BusinessAuthoringDocument document);
}
```

Validuje:
- Projektové jméno a schema verze
- Duplicitní ID/názvy entit a atributů
- `SyncState == Conflict` → Warning
- Osiřelé relace (neexistující source/target entity)

### BusinessValidationIssue

```csharp
public enum ValidationSeverity { Warning = 0, Error = 1 }

public sealed record BusinessValidationIssue
{
    public ValidationSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Path { get; init; }       // např. "Entities[0].Attributes[2]"
    public string? ElementId { get; init; }
    public string? Suggestion { get; init; }
}
```

---

## Identity — BusinessIdAllocator (PROP-020)

```csharp
// PROP-020: Lidsky čitelné ID pro entity, atributy, chování, atd.
// Testy: BusinessModel.Tests/Identity/BusinessIdAllocatorTests.cs.

public sealed class BusinessIdAllocator
{
    public string CreateProjectId(string projectName);        // → "project.my-app"
    public string CreateEntityId(string name, BusinessAuthoringDocument doc); // → "entity.employee"
    public string CreateAttributeId(string name, BusinessEntityNode entity);  // → "attr.employee-email"
    public string CreateBehaviorId(string name, BusinessEntityNode entity);   // → "behavior.employee-calc..."
    public string CreateRelationId(string srcId, string kind, string tgtId, BusinessAuthoringDocument doc);
    // CreateWorkflowId removed PROP-063 (2026-07-18)
    public string CreateQuestionId(BusinessAuthoringDocument doc); // → "question.3"
    public string CreateNoteId();                                // → "note.1720123456"
}
```

Vlastnosti:
- Generuje **lidsky čitelné slugy** (kebab-case) s prefixem dle typu
- Detekce kolizí — při duplicitě přidává `-2`, `-3`, ...
- `Slugify()` konvertuje PascalCase na kebab-case
- ID alokátor je **pomocný nástroj**, ne vynucený — PatchEngine ho může používat, ale negarantuje unikátnost (tu řeší validátor)

---

## Core flow — dvouúrovňový authoring (PROP-020)

```
Host → AI-1 → BusinessModel (abstrakt) → Translator → Core
    → AI-2 (enrichment) → Write-Back (SetCoreDetailOp) → SyncState tracking
```

1. Uživatel (nebo AI-1) vytvoří entity a atributy → `BusinessAuthoringDocument`
2. `DefaultBusinessTranslator` přeloží dokument na Core `TypeModel`/`ClassElement`
3. AI-2 provede enrichment → získá `CoreInfoSource`, `ResolvedPresetId`, `ValueObjectName`
4. `WriteBackService` použije `SetCoreDetailOp` přes `PatchEngine` → zapíše do dokumentu
5. `AttributeSyncState` se nastaví na `Synced` nebo `CoreEdited`
6. Při detekci konfliktu → `Conflict` stav → validátor varuje