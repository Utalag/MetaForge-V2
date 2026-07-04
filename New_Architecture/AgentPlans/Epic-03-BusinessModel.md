# Epic 3 — BusinessModel vrstva

> **Cíl:** Vytvořit projekt `MetaForge.BusinessModel` s BusinessAuthoringDocument, CommandLog, ReplayEngine a PatchEngine.
> **Výstup:** Plně funkční BusinessModel knihovna — source of truth celého systému.
> **Závislosti:** Epic 1 (solution), Epic 2 (Core — jen volně, Translator propojí).

---

## DŮLEŽITÉ: BusinessModel je NEZÁVISLÝ na Core

BusinessModel vrstva NEOBSAHUJE přímé závislosti na Core. Používá vlastní doménové typy. Propojení s Core typovým modelem zajišťuje až Translator vrstva (Epic 4).

---

## TASK-3.1.1 — Založení projektu MetaForge.BusinessModel

**Vstup:** `MetaForge.slnx` existuje, Epic 1 dokončen.
**Výstup:** Nový class library projekt `Src/MetaForge.BusinessModel/MetaForge.BusinessModel.csproj`.
**Soubory:** `Src/MetaForge.BusinessModel/MetaForge.BusinessModel.csproj`, `MetaForge.slnx`

**Kód — `MetaForge.BusinessModel.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.BusinessModel</RootNamespace>
  </PropertyGroup>
</Project>
```

**Aktualizace `MetaForge.slnx`** — přidej za `MetaForge.Core`:

```xml
    <Project Path="Src/MetaForge.BusinessModel/MetaForge.BusinessModel.csproj" />
```

**Ověření:** `dotnet build Src/MetaForge.BusinessModel/` projde.
**Riziko:** Nízké.
**Rollback:** Odeber projekt ze slnx, smaž složku.

---

## TASK-3.2.1 — BusinessEntityNode

**Vstup:** Projekt existuje (TASK-3.1.1).
**Výstup:** Soubor `Src/MetaForge.BusinessModel/Models/BusinessEntityNode.cs`.
**Soubory:** `Src/MetaForge.BusinessModel/Models/BusinessEntityNode.cs`

**Kód:**

```csharp
namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Reprezentuje jednu business entitu — např. "Customer", "Order", "Product".
/// Obsahuje atributy, chování, relace a poznámky.
/// </summary>
public sealed class BusinessEntityNode
{
    /// <summary>Unikátní identifikátor entity.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název entity (např. "Customer").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Atributy entity.</summary>
    public List<BusinessAttributeNode> Attributes { get; } = new();

    /// <summary>Chování entity (metody).</summary>
    public List<BusinessBehaviorNode> Behaviors { get; } = new();

    /// <summary>Relace na jiné entity.</summary>
    public List<BusinessRelationNode> Relations { get; } = new();

    /// <summary>Poznámky k entitě.</summary>
    public List<BusinessNoteNode> Notes { get; } = new();
}
```

**Ověření:** `dotnet build` projde. Zatím selže kvůli chybějícím typům (Attribute, Behavior, Relation, Note) — to je v pořádku, vytvoří se v dalších taskech. Dočasně můžeš použít `object` místo neznámých typů a nahradit je později.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## TASK-3.2.2 — BusinessAttributeNode, BusinessBehaviorNode, BusinessRelationNode, BusinessNoteNode

**Vstup:** TASK-3.2.1 (BusinessEntityNode existuje).
**Výstup:** 4 soubory v `Models/`.
**Soubory:**
- `Src/MetaForge.BusinessModel/Models/BusinessAttributeNode.cs`
- `Src/MetaForge.BusinessModel/Models/BusinessBehaviorNode.cs`
- `Src/MetaForge.BusinessModel/Models/BusinessRelationNode.cs`
- `Src/MetaForge.BusinessModel/Models/BusinessNoteNode.cs`

**Kód — `BusinessAttributeNode.cs`:**

```csharp
namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Atribut business entity — např. "FirstName", "Email", "Price".
/// Popisuje CO atribut znamená, ne JAK je implementován (to řeší Translator).
/// </summary>
public sealed class BusinessAttributeNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název atributu (např. "FirstName").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Business typ (např. "string", "email", "money").</summary>
    public string Type { get; set; } = "string";

    /// <summary>Je atribut povinný?</summary>
    public bool IsRequired { get; set; }

    /// <summary>Maximální délka (pro string).</summary>
    public int? MaxLength { get; set; }

    /// <summary>Minimální hodnota (pro čísla).</summary>
    public string? MinValue { get; set; }

    /// <summary>Maximální hodnota (pro čísla).</summary>
    public string? MaxValue { get; set; }

    /// <summary>Výchozí hodnota.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Dodatečná metadata (JSON-friendly).</summary>
    public Dictionary<string, object?> Metadata { get; } = new();
}
```

**Kód — `BusinessBehaviorNode.cs`:**

```csharp
namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Chování (metoda) entity — např. "CalculateDiscount", "SendNotification".
/// </summary>
public sealed class BusinessBehaviorNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název chování.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Popis chování (co dělá).</summary>
    public string? Description { get; set; }

    /// <summary>Návratový typ (business typ, např. "decimal").</summary>
    public string ReturnType { get; set; } = "void";

    /// <summary>Parametry chování.</summary>
    public List<BusinessParameterNode> Parameters { get; } = new();
}

/// <summary>
/// Parametr chování.
/// </summary>
public sealed class BusinessParameterNode
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public bool IsRequired { get; set; } = true;
    public string? DefaultValue { get; set; }
}
```

**Kód — `BusinessRelationNode.cs`:**

```csharp
namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Relace mezi entitami — např. "Customer → Order" (1:N).
/// </summary>
public sealed class BusinessRelationNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>ID zdrojové entity.</summary>
    public string FromEntityId { get; set; } = string.Empty;

    /// <summary>ID cílové entity.</summary>
    public string ToEntityId { get; set; } = string.Empty;

    /// <summary>Typ relace: "OneToOne", "OneToMany", "ManyToOne", "ManyToMany".</summary>
    public string RelationType { get; set; } = "OneToMany";

    /// <summary>Název navigační property na zdrojové entitě.</summary>
    public string? FromNavigationName { get; set; }

    /// <summary>Název navigační property na cílové entitě.</summary>
    public string? ToNavigationName { get; set; }
}
```

**Kód — `BusinessNoteNode.cs`:**

```csharp
namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Poznámka k entitě — volný textový komentář.
/// </summary>
public sealed class BusinessNoteNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Text poznámky.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Datum vytvoření.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**Ověření:** `dotnet build` projde. Všechny 4 soubory kompilují.
**Riziko:** Nízké.
**Rollback:** Smaž všechny 4 soubory.

---

## TASK-3.2.3 — BusinessAuthoringDocument + PendingQuestionNode

**Vstup:** TASK-3.2.2 (všechny Node typy existují).
**Výstup:** 2 soubory v `Models/`.
**Soubory:**
- `Src/MetaForge.BusinessModel/Models/PendingQuestionNode.cs`
- `Src/MetaForge.BusinessModel/Models/BusinessAuthoringDocument.cs`

**Kód — `PendingQuestionNode.cs`:**

```csharp
namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Nezodpovězená otázka k business modelu — např. "Jaký je formát fakturační adresy?"
/// Slouží pro iterativní upřesňování modelu s uživatelem nebo AI.
/// </summary>
public sealed class PendingQuestionNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Text otázky.</summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>Kontext — ke které entitě/atributu se otázka vztahuje.</summary>
    public string? ContextEntityId { get; set; }

    /// <summary>Datum vytvoření.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**Kód — `BusinessAuthoringDocument.cs`:**

```csharp
namespace MetaForge.BusinessModel.Models;

/// <summary>
/// SOURCE OF TRUTH — kompletní stav business modelu.
/// Veškerý stav systému je odvoditelný z tohoto dokumentu.
/// NIKDY nemutovat přímo — vždy přes PatchEngine + CommandLog.
/// </summary>
public sealed class BusinessAuthoringDocument
{
    /// <summary>Název projektu.</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Verze schématu dokumentu.</summary>
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>Datum poslední modifikace.</summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>Business entity.</summary>
    public List<BusinessEntityNode> Entities { get; } = new();

    /// <summary>Relace mezi entitami.</summary>
    public List<BusinessRelationNode> Relations { get; } = new();

    /// <summary>Vlastní typy definované uživatelem.</summary>
    public List<CustomTypeDefinition> CustomTypes { get; } = new();

    /// <summary>Nezodpovězené otázky.</summary>
    public List<PendingQuestionNode> PendingQuestions { get; } = new();
}

/// <summary>
/// Vlastní typ definovaný uživatelem — např. "Address", "Money".
/// </summary>
public sealed class CustomTypeDefinition
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název typu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Základní typ (např. "string", "decimal").</summary>
    public string BaseType { get; set; } = "string";

    /// <summary>Validační pravidla.</summary>
    public List<string> ValidationRules { get; } = new();

    /// <summary>Popis typu.</summary>
    public string? Description { get; set; }
}
```

**Ověření:** `dotnet build` projde. BusinessAuthoringDocument obsahuje Entities, Relations, CustomTypes, PendingQuestions.
**Riziko:** Střední — toto je source of truth, musí být správně navržený.
**Rollback:** Smaž oba soubory.

---

## TASK-3.3.1 — CommandEnvelope

**Vstup:** Projekt existuje (TASK-3.1.1).
**Výstup:** Soubor `Src/MetaForge.BusinessModel/CommandLog/CommandEnvelope.cs`.
**Soubory:** `Src/MetaForge.BusinessModel/CommandLog/CommandEnvelope.cs`

**Kód:**

```csharp
namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Immutable záznam jednoho commandu v CommandLog.
/// APPEND-ONLY — nikdy se nemění, nemaže, nepřepisuje.
/// </summary>
public sealed record CommandEnvelope
{
    /// <summary>Unikátní identifikátor commandu.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Časové razítko vytvoření commandu.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>Typ commandu (např. "AddEntity", "UpdateAttribute", "DeleteEntity").</summary>
    public string CommandType { get; init; } = string.Empty;

    /// <summary>ID entity, které se command týká (pokud relevantní).</summary>
    public string? TargetEntityId { get; init; }

    /// <summary>ID atributu, kterého se command týká (pokud relevantní).</summary>
    public string? TargetAttributeId { get; init; }

    /// <summary>JSON payload s daty commandu.</summary>
    public string Payload { get; init; } = "{}";

    /// <summary>Verze schématu commandu (pro budoucí migrace).</summary>
    public string SchemaVersion { get; init; } = "1.0";
}
```

**Ověření:** `dotnet build` projde. Record je immutable.
**Riziko:** Střední — formát commandu musí být stabilní, změna formátu = ztráta replay kompatibility.
**Rollback:** Smaž soubor.

---

## TASK-3.3.2 — CommandLogStore

**Vstup:** TASK-3.3.1 (CommandEnvelope existuje).
**Výstup:** Soubor `Src/MetaForge.BusinessModel/CommandLog/CommandLogStore.cs`.
**Soubory:** `Src/MetaForge.BusinessModel/CommandLog/CommandLogStore.cs`

**Kód:**

```csharp
namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Append-only úložiště commandů.
/// INVARIANT: Count nikdy neklesá. Commandy se nikdy nemažou ani nepřepisují.
/// </summary>
public sealed class CommandLogStore
{
    private readonly List<CommandEnvelope> _commands = new();

    /// <summary>Počet commandů v logu. Nikdy neklesá.</summary>
    public int Count => _commands.Count;

    /// <summary>
    /// Přidá command na konec logu.
    /// Každé volání zvyšuje Count o 1.
    /// </summary>
    public void Append(CommandEnvelope envelope)
    {
        _commands.Add(envelope);
    }

    /// <summary>Vrátí všechny commandy v pořadí vložení (pro replay).</summary>
    public IReadOnlyList<CommandEnvelope> GetAll() =>
        _commands.AsReadOnly();

    /// <summary>Vrátí command na daném indexu.</summary>
    public CommandEnvelope? GetAt(int index) =>
        index >= 0 && index < _commands.Count ? _commands[index] : null;

    /// <summary>Vrátí všechny commandy od daného indexu (pro inkrementální replay).</summary>
    public IReadOnlyList<CommandEnvelope> GetFrom(int startIndex) =>
        _commands.Skip(startIndex).ToList().AsReadOnly();
}
```

**Ověření:** `dotnet build` projde. Count se po Append zvýší o 1. GetAll vrací správné pořadí.
**Riziko:** Střední — append-only invariant je kritický pro event sourcing.
**Rollback:** Smaž soubor.

---

## TASK-3.4.1 — ReplayEngine

**Vstup:** TASK-3.3.2 (CommandLogStore), TASK-3.2.3 (BusinessAuthoringDocument).
**Výstup:** Soubor `Src/MetaForge.BusinessModel/CommandLog/ReplayEngine.cs`.
**Soubory:** `Src/MetaForge.BusinessModel/CommandLog/ReplayEngine.cs`

**Kód:**

```csharp
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Autoritativní rekonstrukce stavu — přehraje commandy a vytvoří BusinessAuthoringDocument.
/// </summary>
public sealed class ReplayEngine
{
    /// <summary>
    /// Přehraje všechny commandy a vytvoří aktuální stav dokumentu.
    /// Toto je autoritativní způsob, jak získat stav — ne cache, ne databáze.
    /// </summary>
    public BusinessAuthoringDocument Replay(IReadOnlyList<CommandEnvelope> commands)
    {
        var document = new BusinessAuthoringDocument();

        foreach (var command in commands)
        {
            ApplyCommand(document, command);
        }

        return document;
    }

    /// <summary>
    /// Inkrementální replay — přehraje commandy od startIndex na existující dokument.
    /// </summary>
    public void ReplayFrom(BusinessAuthoringDocument document, IReadOnlyList<CommandEnvelope> commands, int startIndex)
    {
        for (int i = startIndex; i < commands.Count; i++)
        {
            ApplyCommand(document, commands[i]);
        }
    }

    /// <summary>Aplikuje jeden command na dokument.</summary>
    private static void ApplyCommand(BusinessAuthoringDocument document, CommandEnvelope command)
    {
        switch (command.CommandType)
        {
            case "AddEntity":
                ApplyAddEntity(document, command);
                break;
            case "UpdateEntity":
                ApplyUpdateEntity(document, command);
                break;
            case "DeleteEntity":
                ApplyDeleteEntity(document, command);
                break;
            case "AddAttribute":
                ApplyAddAttribute(document, command);
                break;
            case "UpdateAttribute":
                ApplyUpdateAttribute(document, command);
                break;
            case "DeleteAttribute":
                ApplyDeleteAttribute(document, command);
                break;
            case "AddRelation":
                ApplyAddRelation(document, command);
                break;
            // Neznámý command typ = přeskočit (pro budoucí kompatibilitu)
        }

        document.LastModified = command.Timestamp;
    }

    private static void ApplyAddEntity(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = new BusinessEntityNode
        {
            Id = cmd.TargetEntityId ?? Guid.NewGuid().ToString("N")[..8],
            Name = cmd.Payload, // Payload je název entity
        };
        doc.Entities.Add(entity);
    }

    private static void ApplyUpdateEntity(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = doc.Entities.FirstOrDefault(e => e.Id == cmd.TargetEntityId);
        if (entity is not null)
            entity.Name = cmd.Payload;
    }

    private static void ApplyDeleteEntity(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        doc.Entities.RemoveAll(e => e.Id == cmd.TargetEntityId);
        doc.Relations.RemoveAll(r => r.FromEntityId == cmd.TargetEntityId || r.ToEntityId == cmd.TargetEntityId);
    }

    private static void ApplyAddAttribute(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = doc.Entities.FirstOrDefault(e => e.Id == cmd.TargetEntityId);
        if (entity is null) return;

        // Payload formát: "NázevAtributu|Typ|IsRequired"
        var parts = cmd.Payload.Split('|');
        var attr = new BusinessAttributeNode
        {
            Id = cmd.TargetAttributeId ?? Guid.NewGuid().ToString("N")[..8],
            Name = parts.Length > 0 ? parts[0] : "Unnamed",
            Type = parts.Length > 1 ? parts[1] : "string",
            IsRequired = parts.Length > 2 && bool.TryParse(parts[2], out var req) && req,
        };
        entity.Attributes.Add(attr);
    }

    private static void ApplyUpdateAttribute(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = doc.Entities.FirstOrDefault(e => e.Id == cmd.TargetEntityId);
        var attr = entity?.Attributes.FirstOrDefault(a => a.Id == cmd.TargetAttributeId);
        if (attr is null) return;

        var parts = cmd.Payload.Split('|');
        if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0])) attr.Name = parts[0];
        if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1])) attr.Type = parts[1];
        if (parts.Length > 2 && bool.TryParse(parts[2], out var req)) attr.IsRequired = req;
    }

    private static void ApplyDeleteAttribute(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = doc.Entities.FirstOrDefault(e => e.Id == cmd.TargetEntityId);
        entity?.Attributes.RemoveAll(a => a.Id == cmd.TargetAttributeId);
    }

    private static void ApplyAddRelation(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var parts = cmd.Payload.Split('|');
        var relation = new BusinessRelationNode
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            FromEntityId = parts.Length > 0 ? parts[0] : string.Empty,
            ToEntityId = parts.Length > 1 ? parts[1] : string.Empty,
            RelationType = parts.Length > 2 ? parts[2] : "OneToMany",
        };
        doc.Relations.Add(relation);
    }
}
```

**Ověření:** `dotnet build` projde. ReplayEngine.Replay vrací dokument se správně aplikovanými commandy.
**Riziko:** Vysoké — toto je autoritativní rekonstrukce stavu. Chyba v replay = ztráta dat.
**Rollback:** Smaž soubor.

---

## TASK-3.5.1 — IPatchOperation + PatchEngine

**Vstup:** TASK-3.2.3 (BusinessAuthoringDocument), TASK-3.3.1 (CommandEnvelope).
**Výstup:** 2 soubory v `Patches/`.
**Soubory:**
- `Src/MetaForge.BusinessModel/Patches/IPatchOperation.cs`
- `Src/MetaForge.BusinessModel/Patches/PatchEngine.cs`

**Kód — `IPatchOperation.cs`:**

```csharp
namespace MetaForge.BusinessModel.Patches;

/// <summary>
/// Abstrakce pro patch operaci na BusinessAuthoringDocument.
/// </summary>
public interface IPatchOperation
{
    /// <summary>Typ commandu pro CommandLog.</summary>
    string CommandType { get; }

    /// <summary>Provede mutaci na dokumentu.</summary>
    void Apply(BusinessAuthoringDocument document);

    /// <summary>Vytvoří CommandEnvelope pro záznam do logu.</summary>
    CommandEnvelope ToEnvelope();
}

// Importy pro zkrácení zápisu
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
```

**Kód — `PatchEngine.cs`:**

```csharp
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches;

/// <summary>
/// Atomické mutace BusinessAuthoringDocument.
/// Každá mutace vytváří CommandEnvelope a zapisuje do CommandLog.
/// </summary>
public sealed class PatchEngine
{
    private readonly CommandLogStore _logStore;

    public PatchEngine(CommandLogStore logStore)
    {
        _logStore = logStore;
    }

    /// <summary>
    /// Aplikuje patch operaci na dokument a zaznamená do logu.
    /// </summary>
    /// <exception cref="ArgumentNullException">Pokud document nebo operation je null.</exception>
    public void Apply(BusinessAuthoringDocument document, IPatchOperation operation)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(operation);

        // 1. Aplikuj mutaci
        operation.Apply(document);

        // 2. Vytvoř a zapiš command
        var envelope = operation.ToEnvelope();
        _logStore.Append(envelope);

        // 3. Aktualizuj čas modifikace
        document.LastModified = envelope.Timestamp;
    }

    /// <summary>Vytvoří CommandEnvelope pro danou operaci (bez aplikace).</summary>
    public CommandEnvelope CreateEnvelope(IPatchOperation operation) =>
        operation.ToEnvelope();
}
```

**Ověření:** `dotnet build` projde. PatchEngine.Apply aplikuje operaci a zvýší Count v CommandLogStore.
**Riziko:** Střední — patch přes PatchEngine je jediný způsob mutace dokumentu.
**Rollback:** Smaž oba soubory.

---

## TASK-3.5.2 — Konkrétní Patch operace (AddEntityOp, UpdateEntityOp, DeleteEntityOp, AddAttributeOp)

**Vstup:** TASK-3.5.1 (IPatchOperation, PatchEngine).
**Výstup:** 4 soubory v `Patches/Operations/`.
**Soubory:**
- `Src/MetaForge.BusinessModel/Patches/Operations/AddEntityOp.cs`
- `Src/MetaForge.BusinessModel/Patches/Operations/UpdateEntityOp.cs`
- `Src/MetaForge.BusinessModel/Patches/Operations/DeleteEntityOp.cs`
- `Src/MetaForge.BusinessModel/Patches/Operations/AddAttributeOp.cs`

**Kód — `AddEntityOp.cs`:**

```csharp
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Přidá novou entitu do dokumentu.
/// </summary>
public sealed class AddEntityOp : IPatchOperation
{
    public string CommandType => "AddEntity";

    public string EntityId { get; }
    public string EntityName { get; }

    public AddEntityOp(string entityName)
    {
        EntityId = Guid.NewGuid().ToString("N")[..8];
        EntityName = entityName;
    }

    public void Apply(BusinessAuthoringDocument document)
    {
        var entity = new BusinessEntityNode
        {
            Id = EntityId,
            Name = EntityName,
        };
        document.Entities.Add(entity);
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        Payload = EntityName,
    };
}
```

**Kód — `UpdateEntityOp.cs`:**

```csharp
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Aktualizuje název existující entity.
/// </summary>
public sealed class UpdateEntityOp : IPatchOperation
{
    public string CommandType => "UpdateEntity";
    public string EntityId { get; }
    public string NewName { get; }

    public UpdateEntityOp(string entityId, string newName)
    {
        EntityId = entityId;
        NewName = newName;
    }

    public void Apply(BusinessAuthoringDocument document)
    {
        var entity = document.Entities.FirstOrDefault(e => e.Id == EntityId);
        if (entity is not null)
            entity.Name = NewName;
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        Payload = NewName,
    };
}
```

**Kód — `DeleteEntityOp.cs`:**

```csharp
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Smaže entitu a všechny její relace.
/// </summary>
public sealed class DeleteEntityOp : IPatchOperation
{
    public string CommandType => "DeleteEntity";
    public string EntityId { get; }

    public DeleteEntityOp(string entityId)
    {
        EntityId = entityId;
    }

    public void Apply(BusinessAuthoringDocument document)
    {
        document.Entities.RemoveAll(e => e.Id == EntityId);
        document.Relations.RemoveAll(r => r.FromEntityId == EntityId || r.ToEntityId == EntityId);
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        Payload = EntityId,
    };
}
```

**Kód — `AddAttributeOp.cs`:**

```csharp
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Přidá atribut k entitě.
/// </summary>
public sealed class AddAttributeOp : IPatchOperation
{
    public string CommandType => "AddAttribute";
    public string EntityId { get; }
    public string AttributeId { get; }
    public string AttributeName { get; }
    public string AttributeType { get; }
    public bool IsRequired { get; }

    public AddAttributeOp(string entityId, string attributeName, string attributeType = "string", bool isRequired = false)
    {
        EntityId = entityId;
        AttributeId = Guid.NewGuid().ToString("N")[..8];
        AttributeName = attributeName;
        AttributeType = attributeType;
        IsRequired = isRequired;
    }

    public void Apply(BusinessAuthoringDocument document)
    {
        var entity = document.Entities.FirstOrDefault(e => e.Id == EntityId);
        if (entity is null)
            throw new InvalidOperationException($"Entita s Id '{EntityId}' neexistuje.");

        var attr = new BusinessAttributeNode
        {
            Id = AttributeId,
            Name = AttributeName,
            Type = AttributeType,
            IsRequired = IsRequired,
        };
        entity.Attributes.Add(attr);
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        TargetAttributeId = AttributeId,
        Payload = $"{AttributeName}|{AttributeType}|{IsRequired}",
    };
}
```

**Ověření:** `dotnet build` projde. Lze vytvořit AddEntityOp, aplikovat přes PatchEngine a entita se objeví v dokumentu.
**Riziko:** Nízké.
**Rollback:** Smaž všechny 4 soubory.

---

## Souhrn Epic 3 — Co musí existovat po dokončení

```
Src/MetaForge.BusinessModel/
├── MetaForge.BusinessModel.csproj
├── Models/
│   ├── BusinessEntityNode.cs
│   ├── BusinessAttributeNode.cs
│   ├── BusinessBehaviorNode.cs     (vč. BusinessParameterNode)
│   ├── BusinessRelationNode.cs
│   ├── BusinessNoteNode.cs
│   ├── PendingQuestionNode.cs
│   ├── BusinessAuthoringDocument.cs (vč. CustomTypeDefinition)
├── CommandLog/
│   ├── CommandEnvelope.cs
│   ├── CommandLogStore.cs
│   └── ReplayEngine.cs
└── Patches/
    ├── IPatchOperation.cs
    ├── PatchEngine.cs
    └── Operations/
        ├── AddEntityOp.cs
        ├── UpdateEntityOp.cs
        ├── DeleteEntityOp.cs
        └── AddAttributeOp.cs
```

**Celkem souborů:** ~16
**Build:** `dotnet build Src/MetaForge.BusinessModel/` projde bez chyb.

**Checkpoint:** `git tag checkpoint/epic-3-done`
