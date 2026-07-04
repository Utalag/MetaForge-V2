# BusinessModel

> Event Sourcing, CommandLog, Document, Replay, Patches

---


### BusinessAuthoringDocument

```csharp
//context//
// Účel: Single source of truth pro celý business model. Obsahuje entity, atributy, chování, relace, custom typy.
// Vrstva: BusinessModel.
// Vstup: Vzniká replay z CommandLog nebo deserializací z JSON.
// Výstup: Kompletní stav business modelu pro projekci a export.
// Závislosti: BusinessEntityNode, BusinessRelationNode, CustomTypeDefinition.
// Nezávislosti: Nezávisí na Core ani na Translator — je to čistý doménový model.
// Invarianty: Nesmí být mutován přímo — pouze přes PatchEngine + CommandLog. SchemaVersion musí být konzistentní.
// Související typy: CommandLogStore, ReplayEngine, PatchEngine, BusinessAuthoringHostFacade.
// Testy: BusinessModel.Tests/Models/BusinessAuthoringDocumentTests.cs.

public class BusinessAuthoringDocument
{
    public string ProjectName { get; set; }
    public string SchemaVersion { get; set; } = "1.0";
    public List<BusinessEntityNode> Entities { get; } = new();
    public List<BusinessRelationNode> Relations { get; } = new();
    public List<CustomTypeDefinition> CustomTypes { get; } = new();
    public List<PendingQuestionNode> PendingQuestions { get; } = new();
}
```

### CommandLogStore

```csharp
//context//
// Účel: Append-only úložiště commandů. Žádný command se nikdy nemazání ani nepřepisuje.
// Vrstva: BusinessModel.
// Vstup: CommandEnvelope z PatchEngine.
// Výstup: Sekvence commandů pro replay.
// Závislosti: CommandEnvelope.
// Nezávislosti: Nezávisí na BusinessAuthoringDocument — pouze ukládá commandy.
// Invarianty: APPEND-ONLY. Žádný delete, žádný update. Count nikdy neklesá. Commandy se nikdy nemažou.
// Související typy: CommandEnvelope, ReplayEngine, PatchEngine.
// Testy: BusinessModel.Tests/CommandLog/CommandLogStoreTests.cs.

public class CommandLogStore
{
    public void Append(CommandEnvelope envelope) { }
    public IReadOnlyList<CommandEnvelope> GetAll() { }
    public int Count { get; }
}
```

### PatchEngine

```csharp
//context//
// Účel: Atomické mutace BusinessAuthoringDocument. Každá mutace vytváří CommandEnvelope do CommandLog.
// Vrstva: BusinessModel.
// Vstup: Patch operace (typ + payload) od facade nebo hosta.
// Výstup: Mutovaný dokument + záznam v CommandLog.
// Závislosti: BusinessAuthoringDocument, CommandLogStore, IPatchOperation.
// Nezávislosti: Nezávisí na Translator — je to čistý doménový engine.
// Invarianty: Každá mutace MUSÍ projít přes PatchEngine. Přímá mutace dokumentu je zakázaná.
// Související typy: IPatchOperation, AddEntityOp, UpdateEntityOp, BusinessAuthoringHostFacade.
// Testy: BusinessModel.Tests/Patches/PatchEngineTests.cs.

public class PatchEngine
{
    public void Apply(BusinessAuthoringDocument document, IPatchOperation operation) { }
    public CommandEnvelope CreateEnvelope(IPatchOperation operation) { }
}
```

---

## Translator vrstva