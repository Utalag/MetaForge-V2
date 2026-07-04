# PROP-020: BusinessModel — Architektonický upgrade dle původního konceptu

> **Stav:** 🟢 Schváleno
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-003 (dokončeno), PROP-004 (dokončeno), PROP-012 (kandidát), PROP-019 (kandidát)

---

## Cíl

Upgradovat současný `MetaForge.BusinessModel` na úroveň původního konceptu z `For_Inspiration/BusinessModel/`, aby podporoval dvouúrovňový authoring flow:
**abstraktní vrstva (uživatel) → Translator → Core enrichment → Write-Back (CoreDetail) → SyncState tracking**.

## Odůvodnění

Současný BusinessModel (PROP-003) je příliš zjednodušený — chybí mu:
- **Vrstvení atributů** — `BusinessAttributeCoreDetail` pro ukládání Core-konkretizovaných informací
- **Sync state machine** — `AttributeSyncState` pro sledování synchronizace mezi business a core vrstvou
- **Provenance tracking** — `CoreInfoSource`, `CommandProvenance`, `CommandSource`, `CommandIssuedBy`
- **Immutable document pattern** — `{ get; init; }` a `IReadOnlyList<T>` místo mutable `{ get; set; }` a `List<T>`
- **Workflow podpora** — `BusinessWorkflowNode`, `BusinessWorkflowStepNode`, `BusinessWorkflowTransitionNode`
- **Validace** — `BusinessDocumentValidator`
- **Enum typy** — `BusinessBehaviorKind`, `BusinessRelationKind`
- **Rich CommandEnvelope** — streamId, provenance, correlationId, causationId

Bez těchto konceptů nelze realizovat popsaný flow:
> Host → AI-1 → BusinessModel (abstrakt) → Translator → Core → AI-2 (enrichment) → Write-Back (CoreDetail) → SyncState

---

## Analýza gapů

### 🔴 Kritické — blokují core flow

| # | Gap | Původní | Současný | Dopad |
|---|-----|---------|----------|-------|
| 1 | **Vrstvení atributů** | `BusinessAttributeCoreDetail` na atributu | Není — vše ploché | Nelze ukládat Core-výstup vedle uživatelského vstupu |
| 2 | **Sync state** | `AttributeSyncState` enum (New/Synced/BusinessEdited/CoreEdited/Conflict) | Není | Nelze sledovat co je třeba synchronizovat |
| 3 | **Info source** | `CoreInfoSource` enum (Manual/Generated/Hybrid) | Není | Nelze rozlišit kdo data vytvořil |
| 4 | **Command provenance** | `CommandProvenance` (mode, reason, model, confidence, ...) | Není | Nelze trackovat AI model a confidence při enrichmentu |

### 🟡 Významné — potřebné pro plnohodnotný provoz

| # | Gap | Původní | Současný |
|---|-----|---------|----------|
| 5 | **Immutabilita dokumentu** | `{ get; init; }`, `IReadOnlyList<T>` | `{ get; set; }`, `List<T>` |
| 6 | **Workflow** | `BusinessWorkflowNode` + steps + transitions + bindingDetail | Není |
| 7 | **Validace** | `BusinessDocumentValidator` s Error/Warning, path reference | Není |
| 8 | **Enum typy** | `BusinessBehaviorKind`, `BusinessRelationKind` | Stringové typy |
| 9 | **BusinessProjectInfo** | Id, Name, Description, Icon, Version | Jen `ProjectName: string` |
| 10 | **CommandSource** | Chat, Cli, Mcp, Import, System, WebApi, Desktop | Není |
| 11 | **CommandIssuedBy** | actorType, actorId, displayName | Není |
| 12 | **StreamId** | Multi-tenant / multi-project log | Není |

### 🟢 Nízké — nice to have

| # | Gap | Původní | Současný |
|---|-----|---------|----------|
| 13 | BusinessBehaviorInputNode | Samostatný typ | BusinessParameterNode (jednodušší) |
| 14 | PendingQuestionNode rozšíření | Status, Scope, více RelatedId | Jen ContextEntityId |
| 15 | Persistence (JSONL) | JsonlShadowCommandStore/Reader | Není |

---

## Rozsah změn

### Fáze 1: Core modely — Immutabilita + Vrstvení (🔴 Kritická)

#### 1.1 Record-based modely s immutabilitou

Převést všechny modely na **C# `record` s `{ get; init; }`** syntaxí. Record poskytuje built-in `Equals`/`GetHashCode`, `with` výrazy pro nemutující "změny" a `ToString()`:

```diff
- public sealed class BusinessAuthoringDocument
+ public sealed record BusinessAuthoringDocument
  {
-     public string ProjectName { get; set; } = string.Empty;
+     public string Name { get; init; } = string.Empty;

-     public List<BusinessEntityNode> Entities { get; } = new();
+     public IReadOnlyList<BusinessEntityNode> Entities { get; init; } = [];
  }
```

**Výhody `record`:**
- `with` výrazy — `document with { Name = "NewName" }` vytvoří novou instanci se změněnou property
- Automatický `Equals` podle hodnot, ne referencí — dva dokumenty se stejným obsahem jsou equal
- `ToString()` vypíše všechny properties — usnadňuje debugging a logování

**Dotčené soubory (všechny modely v `Models/`):**
- `BusinessAuthoringDocument.cs`
- `BusinessEntityNode.cs`
- `BusinessAttributeNode.cs`
- `BusinessBehaviorNode.cs`
- `BusinessRelationNode.cs`
- `BusinessNoteNode.cs`
- `PendingQuestionNode.cs`
- `CustomTypeDefinition.cs`
- Všechny nové modely (CoreDetail, Workflow, atd.)

**Důsledek:** PatchEngine používá `with` pro vytváření nových instancí dokumentu místo mutace in-place. `CommandEnvelope` již `record` je — zachováno.

#### 1.2 BusinessAttributeCoreDetail

Nový soubor: `Models/BusinessAttributeCoreDetail.cs`

```csharp
public sealed class BusinessAttributeCoreDetail
{
    public CoreInfoSource Source { get; init; }
    public string? ResolvedPresetId { get; init; }
    public string? ValueObjectName { get; init; }
    public bool IsStrongType { get; init; }
    public DateTimeOffset? LastSyncedAt { get; init; }
    
    [JsonIgnore]
    public AttributeSyncState SyncState { get; set; } = AttributeSyncState.New;
}
```

#### 1.3 Rozšíření BusinessAttributeNode

Přidat `CoreDetail` property:

```diff
  public sealed class BusinessAttributeNode
  {
      // ... stávající properties (převedené na init-only) ...
+     public BusinessAttributeCoreDetail? CoreDetail { get; init; }
  }
```

#### 1.4 Nové enum typy

Nové soubory:
- `Models/CoreInfoSource.cs` — `Unknown, Manual, Generated, Hybrid`
- `Models/AttributeSyncState.cs` — `New, Synced, BusinessEdited, CoreEdited, Conflict`

#### 1.4 BusinessIdAllocator — lidsky čitelné ID

Nový soubor: `Identity/BusinessIdAllocator.cs`

```csharp
public sealed class BusinessIdAllocator
{
    public string CreateProjectId(string projectName);
    public string CreateEntityId(string entityName, BusinessAuthoringDocument document);
    public string CreateAttributeId(string attributeName, BusinessEntityNode entity);
    public string CreateBehaviorId(string behaviorName, BusinessEntityNode entity);
    public string CreateRelationId(string sourceEntityId, string kind, string targetEntityId,
                                    BusinessAuthoringDocument document);
    public string CreateWorkflowId(string workflowName, BusinessAuthoringDocument document);
    public string CreateQuestionId(BusinessAuthoringDocument document);
    public string CreateNoteId();
}
```

Generuje **lidsky čitelné slugy** s detekcí kolizí:
- `"Employee"` → `"entity.employee"`
- `"Email"` → `"attr.employee-email"`
- `"CalculateNetSalary"` → `"behavior.employee-calculate-net-salary"`

Při kolizi přidává suffix (`-2`, `-3`, ...).

**Přínos:** Čitelné command logy, debuggovatelné replaye, intuitivní diffy. Nahrazuje současné `Guid.NewGuid().ToString("N")[..8]`.

**Poznámka:** ID alokátor je **pomocný nástroj**, ne vynucený — PatchEngine ho může používat, ale negarantuje unikátnost (tu řeší validátor).

#### 2.1 Rozšíření CommandEnvelope

Přidat pole z původního konceptu + **MutationId** pro idempotenci:

```diff
  public sealed record CommandEnvelope
  {
      public string Id { get; init; }
      public DateTime Timestamp { get; init; }
      public string CommandType { get; init; }
      public string? TargetEntityId { get; init; }
      public string? TargetAttributeId { get; init; }
      public string Payload { get; init; }
      public string SchemaVersion { get; init; }
+     public string StreamId { get; init; } = "default";
+     public CommandSource Source { get; init; } = CommandSource.Unknown;
+     public CoreInfoSource InfoSource { get; init; } = CoreInfoSource.Manual;
+     public CommandIssuedBy IssuedBy { get; init; } = new();
+     public CommandProvenance Provenance { get; init; } = new();
+     public string? CorrelationId { get; init; }
+     public string? CausationId { get; init; }
+     public string? MutationId { get; init; }
  }
```

Nové soubory:
- `CommandLog/CommandSource.cs`
- `CommandLog/CommandIssuedBy.cs`
- `CommandLog/CommandProvenance.cs`

#### 2.2 CommandLogStore s idempotencí

Rozšířit `CommandLogStore.Append()` o deduplikaci podle `MutationId`:

```csharp
public sealed class CommandLogStore
{
    private readonly List<CommandEnvelope> _commands = new();
    private readonly HashSet<string> _appliedMutationIds = new();
    private readonly object _lock = new();

    /// <summary>
    /// Přidá command na konec logu.
    /// Pokud MutationId již existuje, command se ignoruje (idempotence).
    /// </summary>
    /// <returns>true pokud byl command přidán, false pokud již existuje.</returns>
    public bool TryAppend(CommandEnvelope envelope)
    {
        lock (_lock)
        {
            if (envelope.MutationId is not null && !_appliedMutationIds.Add(envelope.MutationId))
                return false; // idempotentní — již aplikováno

            _commands.Add(envelope);
            return true;
        }
    }

    // Původní Append() zachován pro zpětnou kompatibilitu:
    public void Append(CommandEnvelope envelope) => TryAppend(envelope);

    // ... zbytek zůstává beze změny ...
}
```

**Přínos `MutationId`:**
- Bezpečné retry — stejný command poslaný dvakrát se aplikuje jen jednou
- Distribuované systémy — MCP klient může poslat command znovu po reconnectu
- Síťové výpadky — žádné duplicitní entity/atributy

**Poznámka:** `MutationId = null` → idempotence se nekontroluje (zpětná kompatibilita). Interně zůstává `List<T>` s `lock` — nepotřebujeme `ImmutableList`.

### Fáze 3: PatchEngine — immutable mutace (🟡 Významná)

#### 3.1 Nové patch operace

Přidat operace pro CoreDetail:

| Operace | CommandType | Účel |
|---------|-------------|------|
| `SetCoreDetailOp` | SetCoreDetail | Zápis/záměna CoreDetail na atributu |
| `UpdateSyncStateOp` | UpdateSyncState | Změna AttributeSyncState |

#### 3.2 Oprava WriteBackService (Translator)

Po těchto změnách `WriteBackService` **nebude mutovat dokument přímo**, ale použije `SetCoreDetailOp` přes PatchEngine.

### Fáze 4: Workflow + Validace (🟡 Významná)

#### 4.1 Workflow modely

Nové soubory v `Models/`:
- `BusinessWorkflowNode.cs`
- `BusinessWorkflowStepNode.cs`
- `BusinessWorkflowTransitionNode.cs`
- `BusinessWorkflowStepBindingDetail.cs`
- `BusinessWorkflowStepKind.cs`
- `WorkflowBindingSyncState.cs`

#### 4.2 Enum typy pro Behaviors a Relations

Nové soubory:
- `Models/BusinessBehaviorKind.cs` — `Query, Command, Rule`
- `Models/BusinessRelationKind.cs` — `BelongsTo, HasMany, HasOne, ManyToMany`

Upravit:
- `BusinessBehaviorNode.cs` — `Kind` místo `ReturnType`
- `BusinessRelationNode.cs` — `Kind` místo `RelationType`

#### 4.3 BusinessProjectInfo

Nový soubor: `Models/BusinessProjectInfo.cs`

```csharp
public sealed class BusinessProjectInfo
{
    public string Id { get; init; } = "new-project";
    public string Name { get; init; } = "NewProject";
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public int Version { get; init; } = 1;
}
```

Upravit `BusinessAuthoringDocument` — nahradit `ProjectName` za `Project: BusinessProjectInfo`.

#### 4.4 Validace

Nový soubor: `Validation/BusinessDocumentValidator.cs`
Nový soubor: `Validation/BusinessValidationIssue.cs`

### Fáze 5: Rozšíření modelů (🟢 Nízká)

#### 5.1 BusinessBehaviorInputNode

Nový soubor: `Models/BusinessBehaviorInputNode.cs`

```csharp
public sealed class BusinessBehaviorInputNode
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "text";
    public bool Required { get; init; }
    public string? Summary { get; init; }
}
```

Upravit `BusinessBehaviorNode` — nahradit `Parameters` (List<BusinessParameterNode>) za `Inputs` (IReadOnlyList<BusinessBehaviorInputNode>).

#### 5.2 PendingQuestionNode rozšíření

Přidat:
- `PendingQuestionStatus` enum — `Open, Answered, Archived`
- `PendingQuestionScope` enum — `Project, Entity, Attribute, Behavior, Relation, Workflow`
- Rozšířit `Related*Id` properties

---

## Dopady na ostatní vrstvy

### Translator (`Src/MetaForge.Translator/`)

| Soubor | Změna |
|--------|-------|
| `DefaultBusinessTranslator.cs` | Překládat celý dokument (entity→ClassElement), nejen atributy |
| `WriteBackService.cs` | **Opravit** — používat `SetCoreDetailOp` přes PatchEngine, ne přímou mutaci |
| `ProjectionReadService.cs` | Zohlednit `CoreDetail` v projekci, přidat `SyncState` |
| `ProjectionView.cs` | Přidat `CoreDetailInfo`, `SyncState` do `AttributeProjection` |
| `BusinessAuthoringHostFacade.cs` | Přidat operace pro workflow, sync state management |

### Core (`Src/MetaForge.Core/`)

Bez přímých změn — Core zůstává nezávislý na BusinessModel.

### Generators (`Src/MetaForge.Generators/`)

Pozdější fáze — `CSharpGenerator` bude číst `CoreDetail` pro rozhodnutí o value objectech.

---

## Výstup

### Nové soubory

| Soubor | Fáze |
|--------|------|
| `Models/BusinessAttributeCoreDetail.cs` | Fáze 1 |
| `Models/CoreInfoSource.cs` | Fáze 1 |
| `Models/AttributeSyncState.cs` | Fáze 1 |
| `Identity/BusinessIdAllocator.cs` | Fáze 1 |
| `CommandLog/CommandSource.cs` | Fáze 2 |
| `CommandLog/CommandIssuedBy.cs` | Fáze 2 |
| `CommandLog/CommandProvenance.cs` | Fáze 2 |
| `Patches/Operations/SetCoreDetailOp.cs` | Fáze 3 |
| `Patches/Operations/UpdateSyncStateOp.cs` | Fáze 3 |
| `Models/BusinessWorkflowNode.cs` | Fáze 4 |
| `Models/BusinessWorkflowStepNode.cs` | Fáze 4 |
| `Models/BusinessWorkflowTransitionNode.cs` | Fáze 4 |
| `Models/BusinessWorkflowStepBindingDetail.cs` | Fáze 4 |
| `Models/BusinessWorkflowStepKind.cs` | Fáze 4 |
| `Models/WorkflowBindingSyncState.cs` | Fáze 4 |
| `Models/BusinessBehaviorKind.cs` | Fáze 4 |
| `Models/BusinessRelationKind.cs` | Fáze 4 |
| `Models/BusinessProjectInfo.cs` | Fáze 4 |
| `Validation/BusinessDocumentValidator.cs` | Fáze 4 |
| `Validation/BusinessValidationIssue.cs` | Fáze 4 |
| `Models/BusinessBehaviorInputNode.cs` | Fáze 5 |
| `Models/PendingQuestionStatus.cs` | Fáze 5 |
| `Models/PendingQuestionScope.cs` | Fáze 5 |

### Modifikované soubory

| Soubor | Fáze | Změna |
|--------|------|-------|
| `BusinessAuthoringDocument.cs` | 1, 4 | Immutable + ProjectInfo + Workflows |
| `BusinessEntityNode.cs` | 1 | Immutable |
| `BusinessAttributeNode.cs` | 1 | Immutable + CoreDetail |
| `BusinessBehaviorNode.cs` | 1, 4, 5 | Immutable + Kind + Inputs |
| `BusinessRelationNode.cs` | 1, 4 | Immutable + Kind |
| `BusinessNoteNode.cs` | 1 | Immutable |
| `PendingQuestionNode.cs` | 1, 5 | Immutable + rozšíření |
| `CustomTypeDefinition.cs` | 1 | Immutable |
| `CommandEnvelope.cs` | 2 | Rozšíření o provenance/streamId/MutationId |
| `CommandLogStore.cs` | 2 | Idempotence přes MutationId |
| `ReplayEngine.cs` | 2, 3 | Podpora nových command typů |
| `IPatchOperation.cs` | — | Beze změny |
| `PatchEngine.cs` | 3 | Immutable mutace (nový dokument) |
| `AddEntityOp.cs` | 3 | Immutable kompatibilita |
| `UpdateEntityOp.cs` | 3 | Immutable kompatibilita |
| `DeleteEntityOp.cs` | 3 | Immutable kompatibilita |
| `AddAttributeOp.cs` | 3 | Immutable kompatibilita |
| `UpdateAttributeOp.cs` | 3 | Immutable kompatibilita |
| `WriteBackService.cs` (Translator) | 3 | Opravit — SetCoreDetailOp |
| `DefaultBusinessTranslator.cs` (Translator) | — | Rozšířit na celý dokument |

---

## Odhad

| Fáze | Obsah | Dny |
|------|-------|-----|
| **Fáze 1** | Record modely + CoreDetail + enum typy + BusinessIdAllocator | 2 dny |
| **Fáze 2** | CommandLog rozšíření (provenance, streamId, MutationId, idempotence) | 1 den |
| **Fáze 3** | PatchEngine immutable + SetCoreDetailOp + WriteBack oprava | 1,5 dne |
| **Fáze 4** | Workflow modely + validace + enum typy + ProjectInfo | 2 dny |
| **Fáze 5** | BehaviorInputNode + PendingQuestion rozšíření | 0,5 dne |
| **Testy** | Aktualizace + nové testy pro všechny fáze | 2 dny |
| **Translator dopad** | Úprava Translatoru na nové API | 1 den |
| **Celkem** | | **10 dní** |

---

## Rizika

| Riziko | Pravděpodobnost | Dopad | Mitigace |
|--------|-----------------|-------|----------|
| Immutabilita rozbije PatchEngine | Střední | 🔴 Vysoký | Postupná migrace — nejdřív modely, pak engine |
| Změna API rozbije Translator | Střední | 🟡 Střední | Paralelní úprava Translatoru ve stejné fázi |
| Velký rozsah — regrese | Střední | 🟡 Střední | Fázový postup, testy po každé fázi |
| Workflow modely nejsou potřeba hned | Nízká | 🟢 Nízký | Fáze 4 lze odložit |

---

## Závislosti

| Závislost | Stav |
|-----------|------|
| PROP-003 (BusinessModel base) | ✅ Hotovo |
| PROP-004 (Translator) | ✅ Hotovo (bude upraven) |
| PROP-012 (Payload escaping) | 🟡 Kandidát — řešit paralelně |
| PROP-019 (AI Translator) | 🟡 Kandidát — závisí na CoreDetail |

---

## Rozhodnutí

### D1: Immutabilita — postupně nebo najednou?

**Rozhodnuto:** Postupně. Nejdříve přidat nové properties (`CoreDetail`, `StreamId`, ...) jako additivní změny. Immutabilitu (`init`, `IReadOnlyList`) provést jako druhou vlnu, aby se minimalizovalo riziko rozbití PatchEngine.

### D2: BusinessParameterNode → BusinessBehaviorInputNode?

**Rozhodnuto:** Ponechat `BusinessParameterNode` a přidat `Summary`. Plná migrace na `BusinessBehaviorInputNode` ve Fázi 5 (nízká priorita).

### D3: Zachovat CommandLogStore jako mutable interně?

**Rozhodnuto:** Ano. Interní `List<CommandEnvelope>` s `lock` zůstává. Immutabilita se týká jen modelů, ne infrastruktury.

### D4: Record vs class pro modely?

**Rozhodnuto:** `sealed record` s `{ get; init; }` syntaxí. Zachovává čitelnost jako class, získává `with`, `Equals`, `ToString`. Nepoužíváme positional records (moc properties).

### D5: MutationId povinné nebo volitelné?

**Rozhodnuto:** Volitelné. `MutationId = null` → idempotence se nekontroluje (zpětná kompatibilita). Host surfaces (CLI, MCP) by měly MutationId poskytovat.
