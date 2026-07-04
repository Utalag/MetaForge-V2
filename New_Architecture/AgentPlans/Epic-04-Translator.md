# Epic 4 — Translator vrstva

> **Cíl:** Vytvořit projekt `MetaForge.Translator` s Facade, ProjectionReadService, DefaultBusinessTranslator a WriteBack.
> **Výstup:** Plně funkční Translator — propojuje BusinessModel s Core, orchestruje operace.
> **Závislosti:** Epic 2 (Core), Epic 3 (BusinessModel).

---

## DŮLEŽITÉ: Translator propojuje BusinessModel ↔ Core

Translator je PROSTŘEDNÍK mezi vrstvami:
- BusinessModel NEPOUŽÍVÁ Core typy přímo.
- Translator PŘEKLÁDÁ BusinessModel → Core.
- Translator ZAPISUJE enrichment zpět do BusinessModelu.

---

## TASK-4.1.1 — Založení projektu MetaForge.Translator

**Vstup:** `MetaForge.slnx`, Epic 2 a Epic 3 dokončeny.
**Výstup:** Nový class library projekt `Src/MetaForge.Translator/MetaForge.Translator.csproj` s referencemi na Core a BusinessModel.
**Soubory:** `Src/MetaForge.Translator/MetaForge.Translator.csproj`, `MetaForge.slnx`

**Kód — `MetaForge.Translator.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.Translator</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\MetaForge.Core\MetaForge.Core.csproj" />
    <ProjectReference Include="..\MetaForge.BusinessModel\MetaForge.BusinessModel.csproj" />
  </ItemGroup>
</Project>
```

**Aktualizace `MetaForge.slnx`** — přidej za `MetaForge.BusinessModel`:

```xml
    <Project Path="Src/MetaForge.Translator/MetaForge.Translator.csproj" />
```

**Ověření:** `dotnet build Src/MetaForge.Translator/` projde.
**Riziko:** Nízké.
**Rollback:** Odeber projekt ze slnx, smaž složku.

---

## TASK-4.2.1 — IBusinessTranslator rozhraní

**Vstup:** TASK-4.1.1 (projekt existuje).
**Výstup:** Soubor `Src/MetaForge.Translator/Translation/IBusinessTranslator.cs`.
**Soubory:** `Src/MetaForge.Translator/Translation/IBusinessTranslator.cs`

**Kód:**

```csharp
using MetaForge.BusinessModel.Models;
using MetaForge.Core.DataTypes;

namespace MetaForge.Translator.Translation;

/// <summary>
/// Překládá business atributy na Core TypeModel.
/// </summary>
public interface IBusinessTranslator
{
    /// <summary>Přeloží BusinessAttributeNode na TypeModel.</summary>
    TypeModel Translate(BusinessAttributeNode attribute);

    /// <summary>Pokusí se o enrichment (AI/deterministický). Vrací null pokud nic.</summary>
    EnrichmentResult? TryEnrich(BusinessAttributeNode attribute);
}

/// <summary>
/// Výsledek enrichmentu — dodatečné informace o atributu.
/// </summary>
public sealed record EnrichmentResult(
    string AttributeId,
    string? SuggestedCSharpType = null,
    IReadOnlyList<string>? ValidationRules = null,
    string? DefaultValue = null,
    int? MaxLength = null
);
```

**Ověření:** `dotnet build` projde.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## TASK-4.4.1 — DefaultBusinessTranslator

**Vstup:** TASK-4.2.1 (IBusinessTranslator), CatalogManager (z Core), TypeModel (z Core).
**Výstup:** Soubor `Src/MetaForge.Translator/Translation/DefaultBusinessTranslator.cs`.
**Soubory:** `Src/MetaForge.Translator/Translation/DefaultBusinessTranslator.cs`

**Kód:**

```csharp
using MetaForge.BusinessModel.Models;
using MetaForge.Core.Catalog;
using MetaForge.Core.DataTypes;

namespace MetaForge.Translator.Translation;

/// <summary>
/// Výchozí deterministický překladač — business atribut → TypeModel.
/// Používá CatalogManager pro resolvování typů.
/// </summary>
public sealed class DefaultBusinessTranslator : IBusinessTranslator
{
    private readonly CatalogManager _catalog;

    public DefaultBusinessTranslator(CatalogManager catalog)
    {
        _catalog = catalog;
    }

    /// <summary>
    /// Přeloží business atribut na TypeModel.
    /// 1. Zkusí najít v katalogu.
    /// 2. Pokud nenajde, použije fallback na základě názvu typu.
    /// </summary>
    public TypeModel Translate(BusinessAttributeNode attribute)
    {
        // 1. Katalog
        var preset = _catalog.ResolveType(attribute.Type);
        if (preset is not null)
            return preset.Type;

        // 2. Fallback podle názvu typu
        var type = attribute.Type.ToLowerInvariant() switch
        {
            "string" or "text" => TypeModel.String,
            "int" or "integer" or "int32" => TypeModel.Int32,
            "long" or "int64" => TypeModel.Of(DataType.Int64),
            "decimal" or "money" or "price" => TypeModel.Decimal,
            "double" or "float" => TypeModel.Of(DataType.Double),
            "bool" or "boolean" => TypeModel.Bool,
            "datetime" => TypeModel.DateTime,
            "date" => TypeModel.Of(DataType.DateOnly),
            "guid" or "uuid" => TypeModel.Guid,
            "email" => TypeModel.String,
            "phone" => TypeModel.String,
            "url" or "uri" => TypeModel.Of(DataType.Uri),
            _ => TypeModel.Object,
        };

        // 3. Aplikuj IsRequired
        if (attribute.IsRequired && type.IsNullable)
            type = type with { IsNullable = false };

        return type;
    }

    /// <summary>
    /// Deterministický enrichment — odvodí dodatečné informace.
    /// Nepoužívá AI — jen pravidla.
    /// </summary>
    public EnrichmentResult? TryEnrich(BusinessAttributeNode attribute)
    {
        // Jen pro string atributy bez omezení
        if (attribute.Type is "string" or "text" or "email" or "phone")
        {
            var rules = new List<string>();
            int? maxLength = null;

            if (attribute.IsRequired)
                rules.Add("not_empty");

            switch (attribute.Type)
            {
                case "email":
                    rules.Add("email_format");
                    maxLength = 254;
                    break;
                case "phone":
                    rules.Add("phone_format");
                    maxLength = 20;
                    break;
                case "string" or "text":
                    maxLength = attribute.MaxLength ?? 200;
                    break;
            }

            if (rules.Count > 0 || maxLength is not null)
            {
                return new EnrichmentResult(
                    AttributeId: attribute.Id,
                    SuggestedCSharpType: "string",
                    ValidationRules: rules,
                    MaxLength: maxLength
                );
            }
        }

        return null;
    }
}
```

**Ověření:** `dotnet build` projde. Translate pro "email" vrací TypeModel.String. Translate pro "money" vrací TypeModel.Decimal.
**Riziko:** Střední — překlad musí být deterministický, žádné AI.
**Rollback:** Smaž soubor.

---

## TASK-4.3.1 — ProjectionView + ProjectionReadService

**Vstup:** TASK-4.4.1 (DefaultBusinessTranslator), TASK-3.4.1 (ReplayEngine).
**Výstup:** 2 soubory v `Host/`.
**Soubory:**
- `Src/MetaForge.Translator/Host/ProjectionView.cs`
- `Src/MetaForge.Translator/Host/ProjectionReadService.cs`

**Kód — `ProjectionView.cs`:**

```csharp
using MetaForge.Core.DataTypes;

namespace MetaForge.Translator.Host;

/// <summary>
/// Projekce business modelu pro čtení — obsahuje přeložené entity a atributy.
/// </summary>
public sealed class ProjectionView
{
    /// <summary>Název projektu.</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Entita v projekci.</summary>
    public sealed class EntityProjection
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<AttributeProjection> Attributes { get; } = new();
    }

    /// <summary>Atribut v projekci — přeložený do Core typů.</summary>
    public sealed class AttributeProjection
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TypeModel CoreType { get; set; } = TypeModel.Object;
        public bool IsRequired { get; set; }
        public int? MaxLength { get; set; }
        public string? DefaultValue { get; set; }
    }

    public List<EntityProjection> Entities { get; } = new();
}
```

**Kód — `ProjectionReadService.cs`:**

```csharp
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.Translator.Translation;

namespace MetaForge.Translator.Host;

/// <summary>
/// Vytváří projekci business modelu pro čtení.
/// Používá ReplayEngine pro rekonstrukci a DefaultBusinessTranslator pro překlad.
/// </summary>
public sealed class ProjectionReadService
{
    private readonly ReplayEngine _replayEngine;
    private readonly IBusinessTranslator _translator;

    public ProjectionReadService(ReplayEngine replayEngine, IBusinessTranslator translator)
    {
        _replayEngine = replayEngine;
        _translator = translator;
    }

    /// <summary>
    /// Vytvoří aktuální projekci — přehraje commandy a přeloží.
    /// </summary>
    public ProjectionView GetProjection(CommandLogStore logStore)
    {
        var commands = logStore.GetAll();
        var document = _replayEngine.Replay(commands);
        return BuildProjection(document);
    }

    /// <summary>
    /// Vytvoří projekci z existujícího dokumentu (bez replay).
    /// </summary>
    public ProjectionView GetProjection(BusinessAuthoringDocument document)
    {
        return BuildProjection(document);
    }

    private ProjectionView BuildProjection(BusinessAuthoringDocument document)
    {
        var view = new ProjectionView
        {
            ProjectName = document.ProjectName,
        };

        foreach (var entity in document.Entities)
        {
            var entityProj = new ProjectionView.EntityProjection
            {
                Id = entity.Id,
                Name = entity.Name,
            };

            foreach (var attr in entity.Attributes)
            {
                var coreType = _translator.Translate(attr);

                var attrProj = new ProjectionView.AttributeProjection
                {
                    Id = attr.Id,
                    Name = attr.Name,
                    CoreType = coreType,
                    IsRequired = attr.IsRequired,
                    MaxLength = attr.MaxLength,
                    DefaultValue = attr.DefaultValue,
                };

                entityProj.Attributes.Add(attrProj);
            }

            view.Entities.Add(entityProj);
        }

        return view;
    }
}
```

**Ověření:** `dotnet build` projde. ProjectionReadService.GetProjection vrací projekci s přeloženými typy.
**Riziko:** Střední — projekce musí být konzistentní se stavem dokumentu.
**Rollback:** Smaž oba soubory.

---

## TASK-4.5.1 — WriteBackService

**Vstup:** TASK-4.4.1 (DefaultBusinessTranslator), TASK-3.5.2 (AddAttributeOp).
**Výstup:** Soubor `Src/MetaForge.Translator/Translation/WriteBackService.cs`.
**Soubory:** `Src/MetaForge.Translator/Translation/WriteBackService.cs`

**Kód:**

```csharp
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.BusinessModel.Patches.Operations;

namespace MetaForge.Translator.Translation;

/// <summary>
/// Zapisuje enrichment data zpět do business modelu.
/// Např. AI zjistí, že "Email" atribut by měl mít MaxLength=254 → zapíše do atributu.
/// </summary>
public sealed class WriteBackService
{
    private readonly PatchEngine _patchEngine;

    public WriteBackService(PatchEngine patchEngine)
    {
        _patchEngine = patchEngine;
    }

    /// <summary>
    /// Aplikuje enrichment na atribut v dokumentu.
    /// </summary>
    public void ApplyEnrichment(BusinessAuthoringDocument document, string entityId, EnrichmentResult enrichment)
    {
        var entity = document.Entities.FirstOrDefault(e => e.Id == entityId);
        var attr = entity?.Attributes.FirstOrDefault(a => a.Id == enrichment.AttributeId);
        if (attr is null) return;

        // Aplikuj enrichment data
        if (enrichment.MaxLength.HasValue)
            attr.MaxLength = enrichment.MaxLength;

        if (enrichment.DefaultValue is not null)
            attr.DefaultValue = enrichment.DefaultValue;
    }

    /// <summary>
    /// Aplikuje enrichment a zaznamená do CommandLog přes PatchEngine.
    /// </summary>
    public void ApplyEnrichmentWithLog(BusinessAuthoringDocument document, string entityId, EnrichmentResult enrichment)
    {
        var entity = document.Entities.FirstOrDefault(e => e.Id == entityId);
        if (entity is null) return;

        var attr = entity.Attributes.FirstOrDefault(a => a.Id == enrichment.AttributeId);
        if (attr is null) return;

        // Vytvoř update operaci
        var op = new UpdateAttributeOp(
            entityId: entityId,
            attributeId: enrichment.AttributeId,
            newName: null, // neměníme název
            newType: enrichment.SuggestedCSharpType,
            isRequired: null
        );

        _patchEngine.Apply(document, op);

        // Aplikuj enrichment dodatečně
        ApplyEnrichment(document, entityId, enrichment);
    }
}
```

**POZNÁMKA:** Budeš potřebovat rozšířit `UpdateAttributeOp` — přidej konstruktor, který bere `newType` a `isRequired`:

```csharp
// Přidej do UpdateAttributeOp.cs tento konstruktor:
public UpdateAttributeOp(string entityId, string attributeId, string? newName = null, string? newType = null, bool? isRequired = null)
{
    EntityId = entityId;
    AttributeId = attributeId;
    NewName = newName;
    NewType = newType;
    IsRequired = isRequired;
}

// A přidej properties:
public string? NewType { get; }
public bool? IsRequired { get; }

// A uprav ToEnvelope:
public CommandEnvelope ToEnvelope() => new()
{
    CommandType = CommandType,
    TargetEntityId = EntityId,
    TargetAttributeId = AttributeId,
    Payload = $"{(NewName ?? "")}|{(NewType ?? "")}|{IsRequired?.ToString() ?? ""}",
};
```

**Ověření:** `dotnet build` projde. WriteBackService.ApplyEnrichment aktualizuje atribut.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## TASK-4.2.2 — BusinessAuthoringHostFacade

**Vstup:** Všechny předchozí tasky Epic 4.
**Výstup:** Soubor `Src/MetaForge.Translator/Host/BusinessAuthoringHostFacade.cs`.
**Soubory:** `Src/MetaForge.Translator/Host/BusinessAuthoringHostFacade.cs`

**Kód:**

```csharp
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.BusinessModel.Patches.Operations;
using MetaForge.Translator.Translation;

namespace MetaForge.Translator.Host;

/// <summary>
/// JEDINÝ VSTUPNÍ BOD pro host surfaces (CLI, MCP, WebApi).
/// Orchestruje write (PatchEngine) i read (ProjectionReadService) path.
/// </summary>
public sealed class BusinessAuthoringHostFacade
{
    private readonly BusinessAuthoringDocument _document;
    private readonly CommandLogStore _logStore;
    private readonly PatchEngine _patchEngine;
    private readonly ReplayEngine _replayEngine;
    private readonly ProjectionReadService _projectionService;
    private readonly WriteBackService _writeBackService;
    private readonly IBusinessTranslator _translator;

    public BusinessAuthoringHostFacade(
        BusinessAuthoringDocument document,
        CommandLogStore logStore,
        PatchEngine patchEngine,
        ReplayEngine replayEngine,
        ProjectionReadService projectionService,
        WriteBackService writeBackService,
        IBusinessTranslator translator)
    {
        _document = document;
        _logStore = logStore;
        _patchEngine = patchEngine;
        _replayEngine = replayEngine;
        _projectionService = projectionService;
        _writeBackService = writeBackService;
        _translator = translator;
    }

    // === WRITE OPERATIONS ===

    /// <summary>Přidá novou entitu.</summary>
    public string AddEntity(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Název entity nesmí být prázdný.", nameof(name));

        var op = new AddEntityOp(name);
        _patchEngine.Apply(_document, op);
        return op.EntityId;
    }

    /// <summary>Aktualizuje název entity.</summary>
    /// <exception cref="InvalidOperationException">Pokud entita neexistuje.</exception>
    public void UpdateEntity(string entityId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Nový název entity nesmí být prázdný.", nameof(newName));

        var entity = _document.Entities.FirstOrDefault(e => e.Id == entityId)
            ?? throw new InvalidOperationException($"Entita s Id '{entityId}' neexistuje.");

        var op = new UpdateEntityOp(entityId, newName);
        _patchEngine.Apply(_document, op);
    }

    /// <summary>Smaže entitu a všechny její relace.</summary>
    public void DeleteEntity(string entityId)
    {
        var entity = _document.Entities.FirstOrDefault(e => e.Id == entityId)
            ?? throw new InvalidOperationException($"Entita s Id '{entityId}' neexistuje.");

        var op = new DeleteEntityOp(entityId);
        _patchEngine.Apply(_document, op);
    }

    /// <summary>Přidá atribut k entitě.</summary>
    public string AddAttribute(string entityId, string name, string type = "string", bool isRequired = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Název atributu nesmí být prázdný.", nameof(name));

        var entity = _document.Entities.FirstOrDefault(e => e.Id == entityId)
            ?? throw new InvalidOperationException($"Entita s Id '{entityId}' neexistuje.");

        var op = new AddAttributeOp(entityId, name, type, isRequired);
        _patchEngine.Apply(_document, op);
        return op.AttributeId;
    }

    /// <summary>Aplikuje enrichment data na atribut.</summary>
    public void ApplyEnrichment(string entityId, EnrichmentResult enrichment)
    {
        _writeBackService.ApplyEnrichment(_document, entityId, enrichment);
    }

    // === READ OPERATIONS ===

    /// <summary>Vrátí aktuální projekci.</summary>
    public ProjectionView GetProjection() =>
        _projectionService.GetProjection(_document);

    /// <summary>Vrátí samotný dokument (pro debugging).</summary>
    public BusinessAuthoringDocument GetDocument() => _document;

    /// <summary>Vrátí počet commandů v logu.</summary>
    public int GetCommandCount() => _logStore.Count;
}
```

**Ověření:** `dotnet build` projde. Facade má metody AddEntity, UpdateEntity, DeleteEntity, AddAttribute, ApplyEnrichment, GetProjection, GetDocument.
**Riziko:** Střední — Facade je jediný vstupní bod, musí být stabilní.
**Rollback:** Smaž soubor.

---

## Souhrn Epic 4 — Co musí existovat po dokončení

```
Src/MetaForge.Translator/
├── MetaForge.Translator.csproj  (reference na Core + BusinessModel)
├── Translation/
│   ├── IBusinessTranslator.cs   (rozhraní + EnrichmentResult)
│   ├── DefaultBusinessTranslator.cs
│   └── WriteBackService.cs
└── Host/
    ├── ProjectionView.cs
    ├── ProjectionReadService.cs
    └── BusinessAuthoringHostFacade.cs
```

**Celkem souborů:** ~7
**Build:** `dotnet build Src/MetaForge.Translator/` projde bez chyb.

**Checkpoint:** `git tag checkpoint/epic-4-done`
