# Skills a Hooks pro vývoj Nové Architektury

> Tento dokument definuje kompletní sadu **skillů** (specializované znalostní oblasti pro Copilot agenty) a **hooků** (instrukční a agent konfigurace) potřebných pro vývoj MetaForge podle `New_Architecture/`.
>
> Skills a hooky zde definované jsou určeny pro **nový projekt** (cílový repozitář dle `02-Target-Repo-Structure.md`), ne pro aktuální MetaForge codebase.
> Po schválení se definice překopírují do `.github/skills/`, `.github/instructions/` a `.github/agents/` nového projektu.

---

## Navigace

- [Přehled skillů](#přehled-skillů)
- [Skill 1 — new-architecture-overview](#skill-1--new-architecture-overview)
- [Skill 2 — new-architecture-core](#skill-2--new-architecture-core)
- [Skill 3 — new-architecture-business-model](#skill-3--new-architecture-business-model)
- [Skill 4 — new-architecture-translator](#skill-4--new-architecture-translator)
- [Skill 5 — new-architecture-ai](#skill-5--new-architecture-ai)
- [Skill 6 — new-architecture-generators](#skill-6--new-architecture-generators)
- [Skill 7 — new-architecture-infrastructure](#skill-7--new-architecture-infrastructure)
- [Skill 8 — new-architecture-host-surfaces](#skill-8--new-architecture-host-surfaces)
- [Skill 9 — new-architecture-di-composition](#skill-9--new-architecture-di-composition)
- [Skill 10 — new-architecture-error-handling](#skill-10--new-architecture-error-handling)
- [Skill 11 — new-architecture-test-scaffold](#skill-11--new-architecture-test-scaffold)
- [Skill 12 — new-architecture-scaffold](#skill-12--new-architecture-scaffold)
- [Hook 1 — Hlavní instrukce](#hook-1--hlavní-instrukce-new-architecture-instructionsmd)
- [Hook 2 — Orchestrator agent](#hook-2--orchestrator-agent)
- [Hook 3 — C# Implementer agent](#hook-3--c-implementer-agent)
- [Doporučené MCP](#doporučené-mcp)
- [Mapa závislostí](#mapa-závislostí-mezi-skilly)

---

## Přehled skillů

| # | Skill | Trigger | Pokrývá |
|---|-------|---------|---------|
| 1 | `new-architecture-overview` | Orientace v New_Architecture, výběr správného skillu | Index, navigace, rozhodovací tabulka |
| 2 | `new-architecture-core` | Práce s Core vrstvou | AppRoot, RootElement, DataType/TypeModel, Elements, Behaviors, Services |
| 3 | `new-architecture-business-model` | Práce s BusinessModel | BusinessAuthoringDocument, CommandLog, PatchEngine, ReplayEngine |
| 4 | `new-architecture-translator` | Práce s Translator vrstvou | Facade, DefaultBusinessTranslator, Projection, WriteBack |
| 5 | `new-architecture-ai` | Práce s AI vrstvou | IAiBackendAdapter, graceful fallback, provider abstrakce |
| 6 | `new-architecture-generators` | Práce s Generators | CSharpGenerator, BaseCodeGenerator, TemplateManager |
| 7 | `new-architecture-infrastructure` | Práce s Infrastructure | ICommandLogRepository, IDocumentRepository, JSON persistence |
| 8 | `new-architecture-host-surfaces` | Práce s host surfaces | CLI, MCP, WebApi — tenké vrstvy |
| 9 | `new-architecture-di-composition` | DI registrace, Composition Root | Lifetime, appsettings.json, per-host registrace |
| 10 | `new-architecture-error-handling` | Error handling, logging | Exception typy, vrstvení, middleware |
| 11 | `new-architecture-test-scaffold` | Testy, test helpers | TestDocumentBuilder, xUnit, FluentAssertions |
| 12 | `new-architecture-scaffold` | Vytváření projektů a složek | Solution, project scaffold, namespace konvence |

---

## Skill 1 — `new-architecture-overview`

```yaml
---
name: new-architecture-overview
description: "Pouzij pri: orientaci v New_Architecture dokumentaci, vyberu spravneho skillu pro dany ukol, pochopeni struktury a navaznosti dokumentu v New_Architecture/."
---
```

### Účel

Vstupní bod do celé `New_Architecture/` dokumentace. Poskytuje mapu skillů, rozhodovací tabulku a navigaci mezi dokumenty. Použij tento skill vždy, když nevíš, který skill použít, nebo když potřebuješ celkový přehled.

### Kdy použít

- Při prvním kontaktu s New_Architecture
- Když si nejsi jistý, který skill je pro úkol relevantní
- Když potřebuješ pochopit návaznosti mezi vrstvami

### Index dokumentů New_Architecture

| Soubor | Obsah | Stav |
|--------|-------|------|
| `00-Index.md` | Tento index | ✅ |
| `01-Architectural-Guardrails.md` | Neporušitelné architektonické principy | ✅ |
| `02-Target-Repo-Structure.md` | Cílová struktura repozitáře | ✅ |
| `03-Core-Abstractions.md` | AppRoot, RootElement, TypeModel, DataType | ⚠️ Částečný |
| `04-Core-Elements.md` | Class, Interface, Enum, Struct, Property, Method | ✅ |
| `05-Core-Behaviors.md` | Expression, IConstraintInferencer, boundary analýza | ✅ |
| `06-Core-Services.md` | CatalogManager, ForgeBlockRegistry, Discovery, StrongType | ✅ |
| `07-BusinessModel.md` | BusinessAuthoringDocument, CommandLog, PatchEngine, Replay | ✅ |
| `08-Translator.md` | Facade, ProjectionReadService, WriteBackService | ✅ |
| `09-AI-Layer.md` | AI kontrakty, provider abstrakce, graceful fallback | ✅ |
| `10-Generators.md` | CSharpGenerator, BaseCodeGenerator, TemplateManager | ✅ |
| `11-Infrastructure.md` | Persistence, ICommandLogRepository, IDocumentRepository | ✅ |
| `12-Host-Surfaces.md` | CLI, MCP, WebApi — struktura a zodpovědnosti | ✅ |
| `13-Epics-and-Slices.md` | User stories, slices 1-7 | ✅ |
| `14-Atomic-Tasks.md` | Detailní atomické tasky pro malý model | ✅ |
| `15-Test-Scaffold.md` | Testovací strategie, helpers, scénáře | ✅ |
| `16-Risks-and-Rollback.md` | Rizika, rollback strategie | ✅ |
| `17-Skills-and-Agents.md` | Původní návrh skillů a agentů | ✅ |
| `18-Ready-to-Run-Prompts.md` | Prompty pro malý model | ✅ |
| `19-Error-Handling.md` | Error handling, exception politika | ✅ |
| `20-Security.md` | Bezpečnostní model | ✅ |
| `21-Telemetry.md` | Telemetrie a observabilita | ✅ |
| `22-CI-CD.md` | CI/CD pipeline | ✅ |
| `23-Governance.md` | Decision log, ADRs, dokumentační standardy | ✅ |
| `24-Markdown-First-Workflow.md` | Pravidla pro markdown-first vývoj | ✅ |
| `25-DI-and-Composition-Root.md` | DI registrace, Composition Root | ✅ |
| `26-Scaffold-Projects-and-Folders.md` | Scaffold projektů a složek | ✅ |
| `27-ForgeBlock-External-Libraries.md` | Katalog externích knihoven | ✅ |
| **`28-Skills-and-Hooks.md`** | **Tento soubor — definice skillů a hooků** | ✅ |

### Rozhodovací tabulka — který skill použít

| Úkol | Skill |
|------|-------|
| Orientace, výběr skillu | `new-architecture-overview` |
| Core typy, elementy, DataType, TypeModel | `new-architecture-core` |
| BusinessAuthoringDocument, CommandLog | `new-architecture-business-model` |
| Facade, projekce, write-back | `new-architecture-translator` |
| AI integrace, fallback, provider | `new-architecture-ai` |
| CSharpGenerator, code export | `new-architecture-generators` |
| Persistence, soubory, JSON | `new-architecture-infrastructure` |
| CLI, MCP, WebApi | `new-architecture-host-surfaces` |
| DI registrace, Composition Root | `new-architecture-di-composition` |
| Exception handling, logging | `new-architecture-error-handling` |
| Test helpers, test scénáře | `new-architecture-test-scaffold` |
| Vytvoření projektu, složek | `new-architecture-scaffold` |

### Návaznosti mezi dokumenty

```
03-06 Core ← 07 BusinessModel ← 08 Translator ← 09 AI Layer
                                        ↓
                             10 Generators   11 Infrastructure
                                        ↓
                                  12 Host Surfaces
```

### Výstupní checklist

- [ ] Vím, který skill pro můj úkol použít
- [ ] Rozumím návaznostem mezi vrstvami
- [ ] Identifikoval jsem správný dokument v New_Architecture/

---

## Skill 2 — `new-architecture-core`

```yaml
---
name: new-architecture-core
description: "Pouzij pri: praci s Core vrstvou Nove Architektury — AppRoot, RootElement, DataType/TypeModel, ClassElement, PropertyElement, MethodElement, EnumElement, StructElement, Expression, IConstraintInferencer, IStandardLibraryTranslator, CatalogManager, ForgeBlockRegistry, Discovery, StrongType."
---
```

### Účel

Udržet změny v Core vrstvě bezpečné, konzistentní s C#-first architekturou a v souladu s dokumenty `03-Core-Abstractions.md`, `04-Core-Elements.md`, `05-Core-Behaviors.md` a `06-Core-Services.md`.

### Kdy použít

- Při práci se soubory v `Src/MetaForge.Core/`
- Při přidávání nebo změně Core abstrakcí, datových typů, elementů
- Při práci s CatalogManager, ForgeBlockRegistry, Discovery
- Při implementaci Expression, constraint inference, standard library translator

### Architektonické guardraily

| # | Invariant | Vysvětlení |
|---|-----------|------------|
| 1 | **C#-first** (ne jazykově agnostické) | Core může obsahovat C#-specifické typy — `DataType` enum obsahuje 32 C# typů (System.Int32, System.String atd.) |
| 2 | **AppRoot → ProjectElement → RootElement** | AppRoot je vstupní bod, obsahuje projekty, projekt obsahuje RootElement |
| 3 | **Core nesmí záviset na vyšších vrstvách** | Žádná reference na BusinessModel, Translator, Generators |
| 4 | **DataType je sealed enum** | Nikdy se nerozšiřuje děděním — nové typy se přidávají do enumu |
| 5 | **TypeModel je immutable record** | Všechny properties jsou init-only, používá factory metody |
| 6 | **RootElement je abstract** | Konkrétní elementy dědí (ClassElement, InterfaceElement atd.) |
| 7 | **Expression je otevřený k rozšíření** | Nové druhy výrazů se přidávají jako nové třídy dědící z `Expression` |

### Klíčové typy

#### AppRoot a ProjectElement

```csharp
public sealed class AppRoot
{
    public List<ProjectElement> Projects { get; } = new();
}

public sealed class ProjectElement
{
    public string Name { get; set; } = string.Empty;
    public string? DefaultNamespace { get; set; }
    public List<RootElement> RootElements { get; } = new();
}
```

#### DataType enum (32 C# typů)

```csharp
public enum DataType : int
{
    // Číselné: Bool, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64,
    //          Int128, Half, Single, Double, Decimal, NInt, NUInt
    // Textové: Char, String
    // Binární: Binary
    // Časové:  DateOnly, TimeOnly, DateTime, DateTimeOffset, TimeSpan
    // Speciální: Guid, Uri, Version
    // Placeholder: Entity, EnumValue, Object, Dynamic, Void, Array, Nullable, Struct, Record
}
```

#### TypeModel (factory metody)

```csharp
public sealed record TypeModel(
    DataType BaseType,
    bool IsNullable = false,
    bool IsCollection = false,
    string? CollectionType = null,
    IReadOnlyList<TypeModel>? GenericArgs = null)
{
    public static TypeModel String(bool isNullable = false) => new(DataType.String, isNullable);
    public static TypeModel Int32(bool isNullable = false) => new(DataType.Int32, isNullable);
    public static TypeModel Decimal(bool isNullable = false) => new(DataType.Decimal, isNullable);
    // ... další factory metody pro všechny DataType hodnoty
}
```

#### Core elementy

| Třída | Base | Klíčové properties |
|-------|------|--------------------|
| `ClassElement` | `RootElement` | Kind="class", BaseClassName, ImplementedInterfaces, AccessModifier, IsAbstract, IsSealed, IsStatic, IsPartial, Properties[], Methods[] |
| `InterfaceElement` | `RootElement` | Kind="interface", Properties[], Methods[] |
| `EnumElement` | `RootElement` | Kind="enum", UnderlyingType, IsFlags, Members[] |
| `StructElement` | `RootElement` | Kind="struct", IsReadOnly, IsRecord, Properties[], Methods[] |
| `PropertyElement` | — (member) | Name, Type(TypeModel), AccessModifier, HasGetter, HasSetter, IsInitOnly, IsRequired, IsStatic |
| `MethodElement` | — (member) | Name, ReturnType(TypeModel), Parameters[], IsAsync, IsAbstract, IsVirtual, IsOverride, IsStatic |

#### Behaviors

| Typ | Účel |
|-----|-------|
| `Expression` | Abstraktní báze pro výrazy v computed properties/behaviors |
| `ComputedExpression` | Expression s Operation + Operands |
| `IConstraintInferencer` | Inferuje constraints podle názvu atributu a typu |
| `IStandardLibraryTranslator` | Překládá sémantické operace na standardní knihovnu |

#### Services

| Typ | Účel |
|-----|-------|
| `CatalogManager` | Registrace a resolve presetů, vyhledávání |
| `IForgeBlockPackage` | ForgeBlock kontrakt — Handle, Version, Capabilities, Discovery |
| `StrongType` | record(Name, Underlying TypeModel, ValidationRules, Conversion) |

### Workflow

1. Identifikuj dotčenou Core oblast (abstrakce, elementy, datové typy, chování, služby)
2. Najdi odpovídající dokument v `New_Architecture/03-06`
3. Implementuj dle specifikace v dokumentu
4. Ověř architektonické guardraily
5. Přidej/uprav testy v `MetaForge.Core.Tests`

### Anti-patterny

- ❌ Přidávání C#-specifické logiky mimo Core (např. do Translatoru) — C# specifika jsou v Core povolena
- ❌ Obcházení AppRoot → ProjectElement → RootElement hierarchie
- ❌ Mutace TypeModel po vytvoření (TypeModel je immutable)
- ❌ Přidání závislosti Core na vyšší vrstvě

### Výstupní checklist

- [ ] C#-first architektura je dodržena
- [ ] AppRoot → ProjectElement → RootElement hierarchie respektována
- [ ] Core nezávisí na vyšších vrstvách
- [ ] TypeModel používá factory metody
- [ ] DataType enum je použit (ne starý BaseType)
- [ ] Exprese jsou otevřené k rozšíření
- [ ] Testy v MetaForge.Core.Tests jsou aktuální

---

## Skill 3 — `new-architecture-business-model`

```yaml
---
name: new-architecture-business-model
description: "Pouzij pri: praci s BusinessModel vrstvou — BusinessAuthoringDocument, CommandLogStore, PatchEngine, ReplayEngine, BusinessEntityNode, BusinessRelationNode, PendingQuestionNode, CustomTypeDefinition."
---
```

### Účel

Zajistit konzistentní implementaci BusinessModel vrstvy dle `07-BusinessModel.md`. Hlídat invarianty CommandLog, PatchEngine a ReplayEngine.

### Kdy použít

- Při práci se soubory v `Src/MetaForge.BusinessModel/`
- Při implementaci BusinessAuthoringDocument, CommandLogStore, PatchEngine
- Při implementaci ReplayEngine, BusinessEntityNode, relací
- Při práci s PendingQuestions a CustomTypes

### Invarianty (neporušitelné)

| # | Invariant | Důsledek |
|---|-----------|----------|
| 1 | **BusinessAuthoringDocument je source of truth** | Veškerý stav systému je odvoditelný z tohoto dokumentu |
| 2 | **CommandLog je append-only** | Historie změn se nikdy nemaže ani nepřepisuje. `Count` nikdy neklesá. |
| 3 | **Replay je autoritativní rekonstrukce** | Stav se rekonstruuje přehráním commandů, ne čtením cache |
| 4 | **Žádná přímá mutace dokumentu** | Každá změna MUSÍ projít přes PatchEngine |
| 5 | **SchemaVersion musí být konzistentní** | Při replayi se kontroluje shoda verze |

### Klíčové typy

#### BusinessAuthoringDocument

```csharp
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

#### CommandLogStore

```csharp
public class CommandLogStore
{
    public void Append(CommandEnvelope envelope) { }
    public IReadOnlyList<CommandEnvelope> GetAll() { }
    public int Count { get; }
}
```

#### PatchEngine

```csharp
public class PatchEngine
{
    public void Apply(BusinessAuthoringDocument document, IPatchOperation operation) { }
    public CommandEnvelope CreateEnvelope(IPatchOperation operation) { }
}
```

#### ReplayEngine

```csharp
public class ReplayEngine
{
    public BusinessAuthoringDocument Replay(IReadOnlyList<CommandEnvelope> commands) { }
}
```

### Workflow

```
Command → PatchEngine.Apply() → CommandLogStore.Append() → ReplayEngine.Replay() → dokument
```

### Anti-patterny

- ❌ Přímá mutace BusinessAuthoringDocument properties mimo PatchEngine
- ❌ Mazání nebo úprava commandů v CommandLog
- ❌ CommandLogStore závislý na BusinessAuthoringDocument
- ❌ Validace až po mutaci (validace musí proběhnout před Apply)

### Výstupní checklist

- [ ] BusinessAuthoringDocument není přímo mutován
- [ ] CommandLog je append-only (žádný delete/update)
- [ ] Každá mutace prošla přes PatchEngine
- [ ] Replay je deterministický — stejný log = stejný dokument
- [ ] SchemaVersion je konzistentní

---

## Skill 4 — `new-architecture-translator`

```yaml
---
name: new-architecture-translator
description: "Pouzij pri: praci s Translator vrstvou — BusinessAuthoringHostFacade, DefaultBusinessTranslator, ProjectionReadService, WriteBackService, ExpertProjection, ProjectionView."
---
```

### Účel

Zajistit konzistentní implementaci Translator vrstvy dle `08-Translator.md`. Hlídat, že Facade je jediný entry point pro host surfaces a že write/read path jsou oddělené.

### Kdy použít

- Při práci se soubory v `Src/MetaForge.Translator/`
- Při implementaci BusinessAuthoringHostFacade
- Při implementaci DefaultBusinessTranslator, ProjectionReadService
- Při implementaci WriteBackService, ExpertProjection

### Invarianty

| # | Invariant | Důsledek |
|---|-----------|----------|
| 1 | **Facade je jediný entry point** | Host surfaces (CLI, MCP, WebApi) volají pouze Facade |
| 2 | **Write path jde přes PatchEngine** | Každá mutace prochází PatchEngine → CommandLog |
| 3 | **Read path jde přes ProjectionReadService** | Každé čtení prochází replayem |
| 4 | **Facade je surface-agnostic** | Nezávisí na CLI/MCP/WebApi — žádné usingy na host projekty |
| 5 | **Translator je deterministický** | Stejný BusinessAttributeNode → stejný TypeModel. AI enrichment je volitelný overlay. |

### Klíčové typy

#### BusinessAuthoringHostFacade

```csharp
public class BusinessAuthoringHostFacade
{
    // Write operations
    public void AddEntity(string name) { }
    public void UpdateEntity(string entityId, string newName) { }
    public void DeleteEntity(string entityId) { }
    public void AddAttribute(string entityId, string name, string type) { }
    public void UpdateAttribute(string entityId, string attributeId, string? name, string? type) { }
    public void ApplyEnrichment(string entityId, string attributeId, object enrichmentData) { }

    // Read operations
    public ProjectionView GetProjection() { }
    public ExpertProjection GetExpertProjection() { }
    public BusinessAuthoringDocument GetDocument() { }
}
```

#### DefaultBusinessTranslator

```csharp
public class DefaultBusinessTranslator : IBusinessTranslator
{
    public TypeModel Translate(BusinessAttributeNode attribute) { }
    public EnrichmentResult? TryEnrich(BusinessAttributeNode attribute) { }
}
```

### Vazba na ostatní vrstvy

| Závislost | Směr | Účel |
|-----------|------|-------|
| `PatchEngine` | Translator → BusinessModel | Write path |
| `CommandLogStore` | Translator → BusinessModel | Write path |
| `ProjectionReadService` | Translator → BusinessModel | Read path |
| `CatalogManager` | Translator → Core | Resolve presetů při překladu |
| `DefaultBusinessTranslator` | Translator → Core | Překlad BusinessAttributeNode na TypeModel |

### Anti-patterny

- ❌ Host surface volající PatchEngine přímo (mimo Facade)
- ❌ Business logika v host vrstvě
- ❌ Facade závislá na konkrétním host surface
- ❌ Write-back obcházející PatchEngine

### Výstupní checklist

- [ ] Facade je jediný entry point pro host surfaces
- [ ] Write path jde přes PatchEngine → CommandLog
- [ ] Read path jde přes ProjectionReadService (replay)
- [ ] Facade neobsahuje reference na CLI/MCP/WebApi
- [ ] Translator je deterministický (AI je volitelný overlay)
- [ ] Enrichment prochází write-back servisou

---

## Skill 5 — `new-architecture-ai`

```yaml
---
name: new-architecture-ai
description: "Pouzij pri: praci s AI vrstvou — IAiBackendAdapter, ITranslationService, IConstraintInferencer (AI implementace), graceful fallback strategii, prompt building, provider abstrakci (Ollama, OpenAI, Azure)."
---
```

### Účel

Řídit implementaci AI vrstvy dle `09-AI-Layer.md`. Hlídat oddělení kontraktů (v Core/Translator) od implementací (v MetaForge.Ai) a graceful fallback.

### Kdy použít

- Při práci se soubory v `Src/MetaForge.Ai/`
- Při implementaci AI implementací Core/Translator kontraktů
- Při práci s IAiBackendAdapter a provider adaptéry
- Při návrhu prompt building a fallback strategie

### Principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **AI je volitelná** | Systém musí fungovat bez AI. Žádná feature nesmí vyžadovat AI. |
| 2 | **Kontrakty oddělené od implementací** | Rozhraní v Core/Translator, implementace v MetaForge.Ai |
| 3 | **Tier 2 AI vrací strukturovaná data** | Nikdy volný text pro uživatele — vrací `MethodConstraint[]`, `ComputedExpression[]`, `BusinessPatchOperation[]` |
| 4 | **Graceful fallback** | AI selhání → vrátit null → host surface rozhodne o hlášce |
| 5 | **AI pracuje nad synchronizovaným stavem** | Ne nad prázdným promptem — vždy dostává aktuální projekci |

### Architektura

```
Core/Translator (interface)
    ├── IConstraintInferencer (Core)
    ├── ITranslationService (Translator)
    │
    └── MetaForge.Ai (implementace)
        ├── AiConstraintInferencer : IConstraintInferencer
        ├── AiTranslationService : ITranslationService
        └── Abstractions/
            ├── IAiBackendAdapter
            ├── OllamaAdapter
            ├── OpenAIAdapter
            └── AzureAdapter
```

### DI registrace

```csharp
// Volitelné — jen když je AI nakonfigurováno
if (configuration.GetSection("MetaForge:AI:Provider").Value != "None")
{
    builder.Services.AddSingleton<IConstraintInferencer, AiConstraintInferencer>();
    builder.Services.AddSingleton<ITranslationService, AiTranslationService>();
    builder.Services.AddSingleton<IAiBackendAdapter>(sp => new OllamaAdapter("http://localhost:11434"));
}

// Deterministická cesta jako fallback (vždy registrována)
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();
```

### Fallback strategie

| Scénář | AI chování | Fallback |
|--------|-----------|----------|
| AI nedostupné (timeout) | Vrátit null | Deterministický inferencer |
| AI vrací invalidní data | Vrátit null | Původní hodnota beze změny |
| AI není nakonfigurováno | Neregistrovat | Deterministická cesta jako default |
| AI vrací strukturovaná data | Aplikovat enrichment | — |

### Anti-patterny

- ❌ AI jako povinná závislost (žádný graceful fallback)
- ❌ AI kontrakty definované v MetaForge.Ai (patří do Core/Translator)
- ❌ Tier 2 AI vracející volný text pro uživatele
- ❌ AI volaná bez kontextu (synchronizovaného stavu)

### Výstupní checklist

- [ ] AI je volitelná — systém funguje bez AI
- [ ] Fallback je graceful (žádné šíření výjimek)
- [ ] Tier 2 AI vrací strukturovaná data
- [ ] Kontrakty jsou v Core/Translator, ne v MetaForge.Ai
- [ ] Provider abstrakce je rozšiřitelná (Ollama, OpenAI, Azure)

---

## Skill 6 — `new-architecture-generators`

```yaml
---
name: new-architecture-generators
description: "Pouzij pri: praci s Generators vrstvou — CSharpGenerator, BaseCodeGenerator, TemplateManager, GeneratedCodeArtifact, LanguageMapping, PackageManifestGenerator."
---
```

### Účel

Řídit implementaci Generators vrstvy dle `10-Generators.md`. Hlídat C#-first princip — jediný aktivní generátor je CSharpGenerator.

### Kdy použít

- Při práci se soubory v `Src/MetaForge.Generators/`
- Při implementaci CSharpGenerator, BaseCodeGenerator
- Při práci s TemplateManager, šablonami
- Při generování package manifestů

### Principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **C#-first** | Jediný aktivní generátor je CSharpGenerator |
| 2 | **Generátory čtou Core elementy** | Vstupem jsou ClassElement, InterfaceElement, EnumElement atd. |
| 3 | **Výstupem je GeneratedCodeArtifact** | Jednotný výstupní typ pro všechny generátory |
| 4 | **BaseCodeGenerator je abstrakce** | Společná logika (RenderTemplate, LanguageId) |
| 5 | **LanguageMapping je metadata-only** | Nejedná se o aktivní generátor |

### Klíčové typy

```csharp
public abstract class BaseCodeGenerator
{
    public abstract string LanguageId { get; }
    public abstract GeneratedCodeArtifact Generate(RootElement element);
    protected string RenderTemplate(string templateName, object model) { }
}

public class CSharpGenerator : BaseCodeGenerator
{
    public override string LanguageId => "csharp";
    public override GeneratedCodeArtifact Generate(RootElement element) { }
}

public sealed record GeneratedCodeArtifact(
    string FileName,
    string SourceCode,
    string LanguageId,
    IReadOnlyList<DiagnosticInfo>? Diagnostics = null
);

public sealed record LanguageMapping(
    string LanguageId,
    string FileExtension,
    string CommentPrefix,
    bool SupportsPartialClasses
);
```

### Workflow generování

```
RootElement → BaseCodeGenerator.Generate()
                → CSharpGenerator.Generate() (C#-first)
                    → TemplateManager.Render()
                        → GeneratedCodeArtifact
```

### Anti-patterny

- ❌ Přidávání generátorů pro jiné jazyky (C#-first)
- ❌ LanguageMapping používaný jako aktivní generátor
- ❌ Generátor závislý na BusinessModel (může číst jen Core elementy)
- ❌ Generovaný kód, který není kompilabilní

### Výstupní checklist

- [ ] Generovaný C# je kompilabilní
- [ ] BaseCodeGenerator abstrakce je dodržena
- [ ] LanguageMapping je metadata-only
- [ ] CSharpGenerator.LanguageId = "csharp"
- [ ] GeneratedCodeArtifact má konzistentní formát

---

## Skill 7 — `new-architecture-infrastructure`

```yaml
---
name: new-architecture-infrastructure
description: "Pouzij pri: praci s Infrastructure vrstvou — ICommandLogRepository, IDocumentRepository, JsonCommandLogRepository, JsonDocumentRepository, InMemoryCommandLogRepository, FileSystemProvider."
---
```

### Účel

Řídit implementaci Infrastructure vrstvy dle `11-Infrastructure.md`. Hlídat oddělení persistence kontraktů (v BusinessModel) od implementací (v MetaForge.Infrastructure).

### Kdy použít

- Při práci se soubory v `Src/MetaForge.Infrastructure/`
- Při implementaci ICommandLogRepository, IDocumentRepository
- Při implementaci JSON file-based persistence
- Při implementaci in-memory repository pro testy

### Principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **Infrastructure je oddělená vrstva** | BusinessModel nezná soubory ani databázi |
| 2 | **Kontrakty v BusinessModel, implementace zde** | Rozhraní definovaná v BusinessModel/Core |
| 3 | **JSON file-based jako default** | CommandLog jako JSONL, dokument jako JSON |
| 4 | **In-memory implementace pro testy** | Rychlé, izolované, bez I/O |

### Klíčové typy

#### Kontrakty

```csharp
public interface ICommandLogRepository
{
    void Append(CommandEnvelope envelope);
    IReadOnlyList<CommandEnvelope> GetAll();
    int Count { get; }
}

public interface IDocumentRepository
{
    void Save(BusinessAuthoringDocument document);
    BusinessAuthoringDocument? Load();
    bool Exists { get; }
}
```

#### Implementace

| Třída | Účel |
|-------|-------|
| `JsonCommandLogRepository` | Append-only JSONL soubor, každý command na vlastním řádku |
| `JsonDocumentRepository` | JSON serializace celého dokumentu |
| `InMemoryCommandLogRepository` | List<CommandEnvelope> v paměti — pro testy |
| `FileSystemProvider` | Abstrakce nad IO — usnadňuje testování |

### DI registrace

```csharp
// Produkce
builder.Services.AddSingleton<ICommandLogRepository>(
    sp => new JsonCommandLogRepository("Data/commandlog.jsonl"));
builder.Services.AddSingleton<IDocumentRepository>(
    sp => new JsonDocumentRepository("Data/document.json"));

// Testy
builder.Services.AddSingleton<ICommandLogRepository, InMemoryCommandLogRepository>();
```

### Anti-patterny

- ❌ BusinessModel obsahující logiku ukládání (using System.IO)
- ❌ CommandLogRepository umožňující mazání nebo úpravu záznamů
- ❌ JSON serializace cyklických referencí (BusinessAuthoringDocument je strom)

### Výstupní checklist

- [ ] Kontrakty definované v BusinessModel/Core, ne v Infrastructure
- [ ] CommandLog je append-only (JSONL formát)
- [ ] In-memory implementace existuje pro testy
- [ ] JSON serializace je otestovaná

---

## Skill 8 — `new-architecture-host-surfaces`

```yaml
---
name: new-architecture-host-surfaces
description: "Pouzij pri: praci s host surfaces — CLI, MCP, WebApi. Tenké vrstvy bez business logiky, volaji pouze BusinessAuthoringHostFacade."
---
```

### Účel

Zajistit, že host surfaces (CLI, MCP, WebApi) zůstanou tenké — žádná business logika, volají pouze `BusinessAuthoringHostFacade`.

### Kdy použít

- Při práci se soubory v `Src/MetaForge.Cli/`, `Src/MetaForge.Mcp/`, `Src/MetaForge.WebApi/`
- Při přidávání nových CLI commandů, MCP tools, WebApi endpointů
- Při návrhu DTO a formátování výstupu

### Principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **Host surfaces volají pouze Facade** | Žádná přímá práce s PatchEngine, CommandLogStore atd. |
| 2 | **Žádná business logika v host vrstvě** | Validace, překlad, transformace patří do Translatoru |
| 3 | **Každý host má vlastní Composition Root** | Program.cs s DI registracemi |
| 4 | **Error handling na hranici host vrstvy** | CLI → exit code, MCP → JSON-RPC error, WebApi → ErrorResponse |

### Struktura per host

#### CLI (`Src/MetaForge.Cli/`)

```
├── Program.cs                    # Composition Root
├── Commands/
│   ├── AddEntityCommand.cs
│   ├── ProjectionCommand.cs
│   ├── TranslateCommand.cs
│   └── ExportCommand.cs
└── Formatting/
    └── CliOutputFormatter.cs
```

#### MCP (`Src/MetaForge.Mcp/`)

```
├── Program.cs                    # Composition Root
└── Tools/
    ├── AddEntityTool.cs
    ├── GetProjectionTool.cs
    ├── TranslateTool.cs
    └── ExportTool.cs
```

#### WebApi (`Src/MetaForge.WebApi/`)

```
├── Program.cs                    # Composition Root
├── Controllers/
│   ├── AuthoringController.cs
│   ├── ProjectionController.cs
│   └── ExportController.cs
├── Dtos/
│   ├── AddEntityRequest.cs
│   ├── AddEntityResponse.cs
│   ├── ProjectionResponse.cs
│   └── ErrorResponse.cs
└── Middleware/
    ├── ExceptionHandlingMiddleware.cs
    └── RequestLoggingMiddleware.cs
```

### Anti-patterny

- ❌ Business logika v CLI command handleru
- ❌ MCP tool volající PatchEngine přímo (mimo Facade)
- ❌ WebApi controller obsahující doménovou validaci
- ❌ Sdílený kód mezi host surfaces (patří do Translatoru)

### Výstupní checklist

- [ ] Host surface volá pouze BusinessAuthoringHostFacade
- [ ] Žádná business logika v host vrstvě
- [ ] Error handling je na hranici host vrstvy
- [ ] DTO jsou oddělená od doménových modelů

---

## Skill 9 — `new-architecture-di-composition`

```yaml
---
name: new-architecture-di-composition
description: "Pouzij pri: DI registraci, Composition Root, lifetime managementu — Singleton/Scoped/Transient rozhodovani, konfigurace, appsettings.json."
---
```

### Účel

Řídit DI registrace a Composition Root napříč host projekty dle `25-DI-and-Composition-Root.md`.

### Kdy použít

- Při nastavování Program.cs v host projektech
- Při volbě lifetime (Singleton/Scoped/Transient) pro služby
- Při konfiguraci appsettings.json
- Při přidávání nové služby do DI

### Životnost služeb

| Vrstva | Lifetime | Služby |
|--------|----------|--------|
| **Core** | **Singleton** | CatalogManager, ForgeBlockRegistry, MethodBoundaryAnalyzer, ExpressionRendererRegistry, StandardLibraryTranslatorRegistry, IConstraintInferencer, ICatalogProvider |
| **BusinessModel** | **Scoped** | CommandLogStore, PatchEngine, ReplayEngine |
| **Translator** | **Scoped** | BusinessAuthoringHostFacade, ProjectionReadService, DefaultBusinessTranslator, ICommandHandler |
| **CLI specific** | **Singleton** | CliOutputFormatter |
| **MCP specific** | **Singleton / Transient** | McpToolRegistry (Singleton), Tools (Transient) |
| **WebApi specific** | **Singleton / Scoped** | ExceptionHandlingMiddleware (Singleton), Controllers (Scoped) |

### Konfigurace (appsettings.json)

```json
{
  "MetaForge": {
    "Catalog": {
      "BuiltInPresetsPath": "Data/Presets",
      "EnableFileSystemProvider": true,
      "FileSystemCatalogPath": "Data/Catalog"
    },
    "AI": {
      "Provider": "None",
      "Endpoint": "",
      "Model": "",
      "ApiKey": ""
    },
    "Persistence": {
      "CommandLogPath": "Data/commandlog.json",
      "AutoSave": true,
      "AutoSaveIntervalSeconds": 30
    },
    "Logging": {
      "Level": "Information",
      "Console": true,
      "File": {
        "Enabled": false,
        "Path": "Logs/metaforge.log"
      }
    }
  }
}
```

### Proměnné prostředí (pro citlivé údaje)

| Proměnná | Mapování |
|----------|----------|
| `MetaForge__AI__ApiKey` | API klíč pro AI providera |
| `MetaForge__AI__Endpoint` | Vlastní endpoint (např. Azure OpenAI) |

### Anti-patterny

- ❌ Scoped služba používaná v Singleton službě (captive dependency)
- ❌ BusinessModel služby jako Singleton (nesdílet stav mezi requesty)
- ❌ Host-specific služby registrované v nesprávném host projektu
- ❌ Konfigurační hodnoty hardcodované místo appsettings.json

### Výstupní checklist

- [ ] Lifetime je správně zvolen (Singleton/Scoped/Transient)
- [ ] Composition Root je v Program.cs host projektu
- [ ] appsettings.json existuje a obsahuje správnou strukturu
- [ ] AI konfigurace je volitelná (Provider: "None" = vypnuto)
- [ ] Žádné captive dependency

---

## Skill 10 — `new-architecture-error-handling`

```yaml
---
name: new-architecture-error-handling
description: "Pouzij pri: navrhu nebo implementaci error handling strategie — exception typy, vrstveni, logging, middleware, graceful degradation."
---
```

### Účel

Zajistit konzistentní error handling napříč všemi vrstvami dle `19-Error-Handling.md`.

### Kdy použít

- Při implementaci exception handlingu v jakékoli vrstvě
- Při návrhu nových exception typů
- Při implementaci middleware (WebApi), try/catch (CLI), error response (MCP)
- Při rozhodování o logging strategii

### Principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **Výjimky se chytají na hranici vrstev** | Nikdy nepropadají napříč vrstvami bez zpracování |
| 2 | **BusinessModel a Core nikdy nelogují přímo** | Logování je odpovědnost host vrstvy |
| 3 | **Facade zapouzdřuje chyby do strukturovaných výsledků** | Žádné raw exception propagation |
| 4 | **AI selhání = graceful fallback, ne výjimka** | AI vrací null, host surface rozhodne o chybové hlášce |
| 5 | **Strukturované logování přes Microsoft.Extensions.Logging** | Žádný Serilog/NLog jako povinná závislost |

### Exception typy

| Typ | Vrstva | Kdy |
|-----|--------|-----|
| `InvalidOperationException` | BusinessModel | Mutace na neexistující entitě/atributu |
| `ArgumentException` | Core/BusinessModel | Nevalidní vstup (prázdný název, null) |
| `InvalidModelException` | Translator | Export/translate selhal kvůli nevalidnímu modelu |
| `ForgeBlockRegistrationException` | Core | Duplicitní nebo nevalidní ForgeBlock registrace |

### Error handling per vrstva

| Vrstva | Chyby zachycuje | Výstup |
|--------|----------------|--------|
| **Core** | Nikdy — Core je stabilní | Assertion/exception = programátorská chyba |
| **BusinessModel** | Validace před mutací | `ValidationResult` s kolekcí chyb |
| **Translator (Facade)** | Chyby z Translate/Enrichment | Strukturovaná odpověď pro host |
| **CLI** | `try/catch` v Program.cs | Exit code + chybová hláška |
| **MCP** | `try/catch` v Tool handlerech | JSON-RPC error response |
| **WebApi** | `ExceptionHandlingMiddleware` | `ErrorResponse` JSON |

### WebApi middleware

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (ArgumentException ex) { /* 400 */ }
        catch (InvalidOperationException ex) { /* 409 */ }
        catch (Exception ex) { /* 500 */ }
    }
}

public record ErrorResponse(int StatusCode, string Message, string? Details = null);
```

### Anti-patterny

- ❌ BusinessModel nebo Core volající `ILogger` přímo
- ❌ AI výjimka propagovaná do host vrstvy (AI vrací null)
- ❌ Raw exception detaily v HTTP response (production)
- ❌ Různé formáty chybových odpovědí napříč host surfaces

### Výstupní checklist

- [ ] Výjimky se chytají na hranici vrstev
- [ ] BusinessModel a Core nelogují přímo
- [ ] AI selhání = gracefully degradováno (ne výjimka)
- [ ] Každý host surface má konzistentní error response
- [ ] Logování je strukturované (Microsoft.Extensions.Logging)

---

## Skill 11 — `new-architecture-test-scaffold`

```yaml
---
name: new-architecture-test-scaffold
description: "Pouzij pri: navrhu a implementaci testu — TestDocumentBuilder, TestCommandLogBuilder, testovaci konvence, xUnit, FluentAssertions, test helpery."
---
```

### Účel

Zajistit konzistentní testování napříč všemi vrstvami dle `15-Test-Scaffold.md`. Testy jsou first-class citizen — vznikají s každou vrstvou.

### Kdy použít

- Při psaní nových testů
- Při refaktoringu existujících testů
- Při návrhu test helperů (TestDocumentBuilder, TestCommandLogBuilder)
- Při kontrole test coverage

### Testovací principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **Testy jsou first-class citizen** | Vznikají s každou vrstvou, ne až na konci |
| 2 | **Unit testy preferovány** | Rychlé, izolované, deterministické |
| 3 | **Integration testy pro facade** | Ověření orchestrace přes vrstvy |
| 4 | **Žádné testy závislé na AI** | AI path má graceful fallback, testuje se deterministická cesta |
| 5 | **Replay testy** | Ověření, že replay N commandů = expected state |
| 6 | **Generator output testy** | Ověření, že generovaný C# je kompilabilní |

### Test projekty

| Projekt | Co testuje | Typ testů |
|---------|-----------|-----------|
| `MetaForge.Core.Tests` | Typový model, katalog, ForgeBlock registrace, discovery | Unit |
| `MetaForge.BusinessModel.Tests` | Dokument, CommandLog, replay, patches, validation | Unit |
| `MetaForge.Translator.Tests` | Facade, projekce, překlad, write-back, enrichment | Unit + Integration |
| `MetaForge.Generators.Tests` | C# output, template rendering, kompilabilnost | Unit + Snapshot |

### Test helpery

#### TestDocumentBuilder

```csharp
public class TestDocumentBuilder
{
    public TestDocumentBuilder WithEntity(string name) { return this; }
    public TestDocumentBuilder WithAttribute(string entityName, string attrName, string type) { return this; }
    public TestDocumentBuilder WithRelation(string from, string to, string type) { return this; }
    public BusinessAuthoringDocument Build() { return new(); }
}
```

#### TestCommandLogBuilder

```csharp
public class TestCommandLogBuilder
{
    public TestCommandLogBuilder AddEntity(string name) { return this; }
    public TestCommandLogBuilder AddAttribute(string entityId, string name, string type) { return this; }
    public IReadOnlyList<CommandEnvelope> Build() { return new List<CommandEnvelope>(); }
}
```

### Testovací konvence

- **Naming**: `{Třída}_{Metoda}_{Scénář}` nebo `{Třída}_{Scénář}_{Výsledek}`
- **Arrange-Act-Assert** pattern vždy
- **Jeden assert per test** (preferenčně)
- **TestDocumentBuilder** pro setup — ne ruční konstrukce
- **xUnit** jako framework
- **FluentAssertions** pro čitelnost
- **Žádné mocking frameworky pokud nejsou nutné** — preferuj fakes a in-memory implementace

### Anti-patterny

- ❌ Testy závislé na AI (AI path se testuje jen deterministická)
- ❌ Sdílený stav mezi testy (každý test má fresh instanci)
- ❌ Testy bez AAA patternu
- ❌ Mockování všeho (preferuj in-memory implementace)

### Výstupní checklist

- [ ] Testy jsou first-class citizen (součást stejného PR jako implementace)
- [ ] Unit testy preferovány
- [ ] TestDocumentBuilder/TestCommandLogBuilder použit pro setup
- [ ] xUnit + FluentAssertions
- [ ] Žádné testy závislé na AI
- [ ] AAA pattern dodržen

---

## Skill 12 — `new-architecture-scaffold`

```yaml
---
name: new-architecture-scaffold
description: "Pouzij pri: vytvareni novych projektu a slozek dle cilove struktury Nove Architektury — MetaForge.slnx, projekty v Src/, Testy/, ForgeBlocks/."
---
```

### Účel

Poskytnout referenci pro vytváření projektů a složek dle `02-Target-Repo-Structure.md` a `26-Scaffold-Projects-and-Folders.md`.

### Kdy použít

- Při zakládání nového projektu (solution, csproj)
- Při přidávání nového projektu do existujícího solution
- Při vytváření adresářové struktury pro novou vrstvu
- Při kontrole namespace konvencí

### Cílová struktura

```
MetaForge/
├── .github/
│   ├── agents/
│   ├── instructions/
│   │   └── new-architecture-instructions.md
│   ├── skills/
│   │   ├── new-architecture-overview/
│   │   ├── new-architecture-core/
│   │   ├── new-architecture-business-model/
│   │   ├── ... (všechny skills)
│   └── workflows/
├── Src/
│   ├── MetaForge.Abstractions/
│   ├── MetaForge.Core/
│   ├── MetaForge.BusinessModel/
│   ├── MetaForge.Translator/
│   ├── MetaForge.Infrastructure/
│   ├── MetaForge.Cli/
│   ├── MetaForge.Mcp/
│   ├── MetaForge.WebApi/
│   ├── MetaForge.Generators/
│   ├── MetaForge.Ai/
│   └── ForgeBlocks/
│       ├── Math/
│       ├── String/
│       └── Validation/
├── Tests/
│   ├── MetaForge.Core.Tests/
│   ├── MetaForge.BusinessModel.Tests/
│   ├── MetaForge.Translator.Tests/
│   ├── MetaForge.Generators.Tests/
│   └── MetaForge.WebApi.Tests/
├── Docs/
│   ├── Architecture/
│   └── Plans/
├── PROPOSALS.md
├── PROPOSALS_NEXT.md
├── Progress.md
├── Memories.md
├── README.md
└── MetaForge.slnx
```

### Solution struktura

```xml
<Solution>
  <Folder Name="/Src/">
    <Project Path="Src/MetaForge.Abstractions/MetaForge.Abstractions.csproj" />
    <Project Path="Src/MetaForge.Core/MetaForge.Core.csproj" />
    <Project Path="Src/MetaForge.BusinessModel/MetaForge.BusinessModel.csproj" />
    <Project Path="Src/MetaForge.Translator/MetaForge.Translator.csproj" />
    <Project Path="Src/MetaForge.Infrastructure/MetaForge.Infrastructure.csproj" />
    <Project Path="Src/MetaForge.Cli/MetaForge.Cli.csproj" />
    <Project Path="Src/MetaForge.Mcp/MetaForge.Mcp.csproj" />
    <Project Path="Src/MetaForge.WebApi/MetaForge.WebApi.csproj" />
    <Project Path="Src/MetaForge.Generators/MetaForge.Generators.csproj" />
    <Project Path="Src/MetaForge.Ai/MetaForge.Ai.csproj" />
  </Folder>
  <Folder Name="/Src/ForgeBlocks/">
    <Project Path="Src/ForgeBlocks/Math/MetaForge.ForgeBlocks.Math.csproj" />
    <Project Path="Src/ForgeBlocks/String/MetaForge.ForgeBlocks.String.csproj" />
  </Folder>
  <Folder Name="/Tests/">
    <Project Path="Tests/MetaForge.Core.Tests/MetaForge.Core.Tests.csproj" />
    <Project Path="Tests/MetaForge.BusinessModel.Tests/MetaForge.BusinessModel.Tests.csproj" />
    <Project Path="Tests/MetaForge.Translator.Tests/MetaForge.Translator.Tests.csproj" />
    <Project Path="Tests/MetaForge.Generators.Tests/MetaForge.Generators.Tests.csproj" />
    <Project Path="Tests/MetaForge.WebApi.Tests/MetaForge.WebApi.Tests.csproj" />
  </Folder>
</Solution>
```

### Namespace konvence

```yaml
MetaForge.Core:                 "MetaForge.Core"
MetaForge.Core.Abstractions:    "MetaForge.Core.Abstractions"
MetaForge.Core.DataTypes:       "MetaForge.Core.DataTypes"
MetaForge.Core.Elements:        "MetaForge.Core.Elements"
MetaForge.Core.Elements.Types:  "MetaForge.Core.Elements.Types"
MetaForge.Core.Elements.Members:"MetaForge.Core.Elements.Members"
MetaForge.Core.Elements.Expressions: "MetaForge.Core.Elements.Expressions"
MetaForge.BusinessModel:        "MetaForge.BusinessModel"
MetaForge.Translator:           "MetaForge.Translator"
MetaForge.Generators:           "MetaForge.Generators"
MetaForge.Infrastructure:       "MetaForge.Infrastructure"
MetaForge.Ai:                   "MetaForge.Ai"
MetaForge.ForgeBlocks.{Name}:  "MetaForge.ForgeBlocks.{Name}"
```

### Anti-patterny

- ❌ Vytváření projektů mimo definovanou strukturu
- ❌ Špatné namespace (např. `MetaForge.Core` místo `MetaForge.Core.Abstractions`)
- ❌ Chybějící test project k novému projektu
- ❌ Projekt bez odpovídajícího skillu v `.github/skills/`

### Výstupní checklist

- [ ] Solution odráží cílovou strukturu z `02-Target-Repo-Structure.md`
- [ ] Všechny projekty mají správné namespace
- [ ] Každý Src projekt má odpovídající test project
- [ ] ForgeBlocky jsou ve vlastní složce `Src/ForgeBlocks/`
- [ ] Governance soubory (PROPOSALS.md atd.) existují

---

## Hook 1 — Hlavní instrukce (`new-architecture-instructions.md`)

```yaml
---
name: New_Architecture — Hlavní instrukce
description: "Hlavní instrukce pro vsechny agenty pracujici na New_Architecture MetaForge."
applyTo: "Src/MetaForge.*"
---
```

### Účel

Centrální instrukční soubor pro všechny agenty pracující na novém projektu. Definuje zdroj pravdy, guardraily a governance pravidla.

### Zdroj pravdy

| Zdroj | Role |
|-------|------|
| `New_Architecture/` | Autoritativní dokumentace cílového stavu |
| `Src/` kód | Aktuální implementace — v případě konfliktu s dokumentací platí `New_Architecture/` (pro nový projekt) |
| `Docs/Architecture/` | Archivní a referenční dokumentace |
| `Docs/Plans/` | Detailní návrhy jednotlivých změn |

### Architektonické guardraily (výtah)

| # | Guardrail | Popis |
|---|-----------|-------|
| 1 | **C#-first** | Core je C#-first (ne jazykově agnostické). `DataType` enum obsahuje C# typy. |
| 2 | **AppRoot → ProjectElement → RootElement** | Hierarchie vstupního bodu |
| 3 | **CommandLog append-only** | Historie změn se nikdy nemaže |
| 4 | **Facade je jediný entry point** | Host surfaces volají pouze Facade |
| 5 | **AI je volitelná** | Graceful fallback vždy |
| 6 | **Zero-Fault** | Invalidní model se neexportuje |
| 7 | **Tenké host surfaces** | Žádná business logika v CLI/MCP/WebApi |
| 8 | **Core nesmí záviset na vyšších vrstvách** | Žádná reference na Translator, BusinessModel atd. |

### Governance workflow

| Krok | Akce |
|------|------|
| 1 | Návrh funkcí do `PROPOSALS.md` |
| 2 | Detail do `Docs/Plans/plan-XX.md` |
| 3 | Bez schválení **neimplementovat** |
| 4 | Před změnou zkontrolovat `Memories.md` |
| 5 | Po dokončení zapsat do `Progress.md` |
| 6 | Nové poznatky do `Memories.md` |
| 7 | Commit zprávy **vždy v češtině** |

### Skill navigace

| Potřebuješ | Použij skill |
|-----------|-------------|
| Core, DataType, TypeModel, elementy | `new-architecture-core` |
| BusinessAuthoringDocument, CommandLog | `new-architecture-business-model` |
| Facade, projekce, write-back | `new-architecture-translator` |
| AI integrace, fallback | `new-architecture-ai` |
| CSharpGenerator, code export | `new-architecture-generators` |
| Persistence, JSON | `new-architecture-infrastructure` |
| CLI, MCP, WebApi | `new-architecture-host-surfaces` |
| DI registrace | `new-architecture-di-composition` |
| Exception handling | `new-architecture-error-handling` |
| Testy | `new-architecture-test-scaffold` |
| Scaffold projektů | `new-architecture-scaffold` |
| Orientace, který skill použít | `new-architecture-overview` |

### Povinná kontrola před dokončením

- [ ] Je respektována C#-first architektura?
- [ ] Je Facade jediný entry point?
- [ ] Je CommandLog append-only?
- [ ] Je AI volitelná s graceful fallback?
- [ ] Jsou host surfaces tenké?
- [ ] Je PROPOSALS.md aktuální?
- [ ] Je Progress.md zapsán?
- [ ] Jsou nové poznatky v Memories.md?

---

## Hook 2 — Orchestrator agent

```yaml
---
name: New_Architecture Platform Orchestrator
description: "Pouzij pri: vedeni vyvoje Nove Architektury MetaForge, delegaci na specializovane subagenty, rozhodovani o vrstvach a prioritach."
tools: [read, search, edit, web/fetch, agent/runSubagent, vscode/askQuestions]
agents:
  - New_Architecture C# Implementer
user-invocable: true
---
```

You are the main orchestration agent for New_Architecture MetaForge platform development.

### Primary Mission

- Řídit vývoj nového MetaForge projektu dle `New_Architecture/` dokumentace.
- Delegovat na specializované agenty tak, aby každá vrstva řešila jen svou část.
- Držet C#-first architekturu, vrstvení a governance pravidla.

### Delegace

| Úkol | Agent/Skill |
|------|-------------|
| C# implementace Core, BusinessModel, Translator, Generators | `New_Architecture C# Implementer` |
| Orientace v dokumentaci | `new-architecture-overview` skill |
| Core vrstva | `new-architecture-core` skill |
| BusinessModel | `new-architecture-business-model` skill |
| Translator | `new-architecture-translator` skill |
| AI integrace | `new-architecture-ai` skill |
| Generators | `new-architecture-generators` skill |
| Infrastructure | `new-architecture-infrastructure` skill |
| Host surfaces | `new-architecture-host-surfaces` skill |
| DI | `new-architecture-di-composition` skill |
| Error handling | `new-architecture-error-handling` skill |
| Testy | `new-architecture-test-scaffold` skill |
| Scaffold | `new-architecture-scaffold` skill |

### Mandatory Rules

- Před implementací zkontroluj `PROPOSALS.md` a `Memories.md`.
- Každá změna musí respektovat `01-Architectural-Guardrails.md`.
- Deleguj implementaci na specializovaného agenta, neimplementuj sám.
- Po implementaci aktualizuj `Progress.md` a `Memories.md`.

### Workflow

1. Rozpoznej dotčenou vrstvu (Core, BusinessModel, Translator, atd.)
2. Aktivuj odpovídající skill
3. Deleguj implementaci na `New_Architecture C# Implementer`
4. Zkontroluj výstup proti guardrailům
5. Zajisti governance follow-up (Progress.md, Memories.md)

---

## Hook 3 — C# Implementer agent

```yaml
---
name: New_Architecture C# Implementer
description: "Pouzij pri: implementaci C# kodu dle Nove Architektury — Core elementy, BusinessModel, Translator, Generators, Host surfaces, Infrastructure."
tools: [read, search, edit]
agents: []
user-invocable: false
---
```

You are the C# implementation specialist for New_Architecture MetaForge.

### Mission

- Implementovat schválené změny v C# dle `New_Architecture/` specifikace.
- Dodržovat C#-first architekturu (DataType s 32 C# typy, AppRoot hierarchie).
- Respektovat vrstvení a architektonické guardraily.

### Mandatory Rules

- Před implementací si aktivuj odpovídající skill (`new-architecture-core`, `new-architecture-business-model`, atd.)
- Dodržuj C#-first — Core může obsahovat C#-specifické typy
- AppRoot → ProjectElement → RootElement hierarchie je povinná
- TypeModel je immutable record s factory metodami
- Nepřidávej business logiku do host surfaces
- Neobcházej Facade — host surfaces volají pouze Facade
- CommandLog je append-only — žádný delete/update
- AI je volitelná — vždy implementuj deterministickou cestu

### Approach

1. Aktivuj skill pro dotčenou vrstvu
2. Přečti odpovídající dokument v `New_Architecture/`
3. Implementuj nejmenší rozumný řez
4. Zachovej kompatibilitu kontraktů a naming
5. Připrav testy (nebo deleguj na test sentinela)

### Output

- `Dotčené soubory`
- `Implementační řez`
- `Otevřené body / rizika`

---

## Mapa závislostí mezi skilly

```
new-architecture-overview
    │
    ├── new-architecture-core
    │       ├── new-architecture-business-model
    │       │       ├── new-architecture-translator
    │       │       │       ├── new-architecture-ai
    │       │       │       ├── new-architecture-host-surfaces
    │       │       │       └── new-architecture-di-composition
    │       │       └── new-architecture-infrastructure
    │       └── new-architecture-generators
    │
    ├── new-architecture-error-handling (průřezový)
    ├── new-architecture-test-scaffold (průřezový)
    ├── new-architecture-scaffold (inicializační)
    │
    └── Hook 1: new-architecture-instructions.md
        ├── Hook 2: orchestrator.agent.md
        └── Hook 3: csharp-implementer.agent.md
```

---

## Doporučené MCP

> Model Context Protocol (MCP) servery, které agentům rozšiřují schopnosti při vývoji Nové Architektury.
> MCP servery jsou volitelné — každý přidává specifickou schopnost (čtení souborů, analýza kódu, práce s balíčky atd.).

### Přehled

| MCP server | Účel | Kdy použít |
|-----------|------|-----------|
| `@modelcontextprotocol/filesystem` | Čtení a zápis souborů, adresářové operace | Vždy — základní MCP pro práci se soubory |
| `@modelcontextprotocol/github` | GitHub API — issues, PR, commity, repos | Práce s GitHub repozitářem, code review |
| `@modelcontextprotocol/web-search` | Webové vyhledávání | Hledání dokumentace, NuGet balíčků, best practices |
| `@modelcontextprotocol/sqlite` | SQLite databáze | Testování persistence, lokální úložiště |
| `domchristie/humpback-mcp` | Transformace mezi naming konvencemi | Převod PascalCase/camelCase/snake_case při mapování |
| `kagisearch/mcp-dotnet` | .NET/C# specifické operace | Práce s .NET projekty, NuGet, build |
| `upstash/context7` | Aktuální dokumentace knihoven a API pro LLM | Hledání dokumentace .NET balíčků, ForgeBlock knihoven, API referencí |

### Detailní popis

#### `@modelcontextprotocol/filesystem`

- **Typ**: Základní (core)
- **Instalace**: `npx @modelcontextprotocol/filesystem`
- **Schopnosti**:
  - Čtení a zápis souborů
  - Vytváření a mazání adresářů
  - Vyhledávání v souborech
  - Práce s glob patterny
- **Využití v projektu**: Čtení .csproj, .slnx, JSON konfigurací, zápis vygenerovaného kódu
- **Doporučení**: **Povinný** — bez něj agent nepracuje se soubory

#### `@modelcontextprotocol/github`

- **Typ**: Integrační
- **Instalace**: `npx @modelcontextprotocol/github`
- **Schopnosti**:
  - Vytváření a správa issues/PR
  - Čtení repozitářů, souborů, commit historie
  - Code review, komentáře
  - Vyhledávání v repozitáři
- **Využití v projektu**:
  - Vytváření PR s novými implementacemi
  - Code review před mergem
  - Sledování issues a feature requestů
  - Kontrola git historie pro ADR a rozhodnutí
- **Doporučení**: **Doporučený** — governance workflow vyžaduje PR a review

#### `@modelcontextprotocol/web-search`

- **Typ**: Vyhledávací
- **Instalace**: `npx @modelcontextprotocol/web-search`
- **Schopnosti**:
  - Vyhledávání na webu
  - Hledání dokumentace
  - Hledání NuGet balíčků
  - Hledání best practices a řešení problémů
- **Využití v projektu**:
  - Hledání vhodných NuGet balíčků pro ForgeBlocky
  - Hledání API dokumentace .NET knihoven
  - Hledání vzorových implementací
- **Doporučení**: **Doporučený** — užitečný při výběru knihoven a řešení problémů

#### `@modelcontextprotocol/sqlite`

- **Typ**: Databázový
- **Instalace**: `npx @modelcontextprotocol/sqlite`
- **Schopnosti**:
  - Vytváření a správa SQLite databází
  - SQL dotazy
  - Import/export dat
- **Využití v projektu**:
  - Testování persistence (InMemoryCommandLogRepository je in-memory, ale SQLite varianta je reálnější)
  - Lokální úložiště pro telemetrii
  - Cache pro CatalogManager
- **Doporučení**: **Volitelný** — užitečný pro integration testy a lokální úložiště

#### `kagisearch/mcp-dotnet`

- **Typ**: Jazykově specifický (.NET)
- **Instalace**: `npx @kagisearch/mcp-dotnet`
- **Schopnosti**:
  - Vyhledávání v NuGet balíčcích
  - Informace o .NET verzích
  - Hledání API referencí
- **Využití v projektu**:
  - Hledání NuGet balíčků pro ForgeBlock implementace
  - Ověření kompatibility .NET verzí
  - Hledání API vzorů pro generátory
- **Doporučení**: **Doporučený** — specializovaný na .NET ekosystém

#### `domchristie/humpback-mcp`

- **Typ**: Utility
- **Instalace**: `npx domchristie/humpback-mcp`
- **Schopnosti**:
  - Převod mezi naming konvencemi (PascalCase, camelCase, snake_case, kebab-case)
  - Detekce konvence
- **Využití v projektu**:
  - Převod názvů při mapování business → Core (BusinessEntityNode → ClassElement)
  - Generování kódu s různými konvencemi
  - Konzistence názvů napříč vrstvami
- **Doporučení**: **Volitelný** — užitečný při mapování a generování kódu

#### `upstash/context7`

- **Typ**: Dokumentační (library docs)
- **Web**: [context7.com](https://context7.com/) — 58.5k ⭐ na GitHubu
- **Instalace**: `npx ctx7 setup` (CLI + skill) nebo manuálně jako MCP server `https://mcp.context7.com/mcp`
- **Schopnosti**:
  - Vyhledávání aktuální dokumentace knihoven a frameworků
  - Verze-specifické API reference
  - Přímé vkládání dokumentace do promptu agenta
  - Podpora 113 000+ knihoven
- **MCP nástroje**:
  - `resolve-library-id` — převod názvu knihovny na Context7 ID
  - `query-docs` — získání dokumentace pro konkrétní knihovnu
- **Využití v projektu**:
  - Hledání dokumentace .NET / C# knihoven pro ForgeBlock implementace
  - Ověření aktuálního API .NET balíčků (System.Text.Json, Microsoft.Extensions.DependencyInjection atd.)
  - Hledání dokumentace externích knihoven z katalogu `27-ForgeBlock-External-Libraries.md`
  - Rychlé dohledání API referencí při implementaci Translator, Generators, Host surfaces
  - Získání aktuálních příkladů pro NuGet balíčky
- **Doporučení**: **Doporučený** — klíčový pro práci s externími knihovnami a aktuální dokumentací

### Mapa MCP k vrstvám projektu

| Vrstva | Užitečné MCP |
|--------|-------------|
| **Core** | `filesystem`, `dotnet`, `humpback`, `context7` |
| **BusinessModel** | `filesystem`, `sqlite` |
| **Translator** | `filesystem`, `humpback`, `context7` |
| **Generators** | `filesystem`, `dotnet`, `humpback`, `web-search`, `context7` |
| **Infrastructure** | `filesystem`, `sqlite`, `context7` |
| **Host surfaces** | `filesystem`, `github`, `context7` |
| **ForgeBlocks** | `dotnet`, `web-search`, `context7` |
| **Testy** | `filesystem`, `sqlite` |
| **Governance** | `github`, `filesystem` |

### Instalační konfigurace

```json
{
  "mcpServers": {
    "filesystem": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/filesystem", "Src", "Tests", "Docs", "New_Architecture"]
    },
    "github": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/github"],
      "env": {
        "GITHUB_TOKEN": "${GITHUB_TOKEN}"
      }
    },
    "web-search": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/web-search"]
    },
    "sqlite": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/sqlite", "Data/metaforge.db"]
    },
    "dotnet": {
      "command": "npx",
      "args": ["-y", "@kagisearch/mcp-dotnet"]
    },
    "humpback": {
      "command": "npx",
      "args": ["-y", "domchristie/humpback-mcp"]
    },
    "context7": {
      "command": "npx",
      "args": ["-y", "@upstash/context7-mcp"],
      "env": {
        "CONTEXT7_API_KEY": "${CONTEXT7_API_KEY}"
      }
    }
  }
}
```

### Bezpečnostní doporučení

| MCP server | Riziko | Mitigace |
|-----------|--------|----------|
| `filesystem` | Zápis na libovolné místo | Omezit allowed directories na `Src/`, `Tests/`, `Docs/`, `New_Architecture/` |
| `github` | Token exfiltrace | Používat `GITHUB_TOKEN` s minimálními právy (repo scope) |
| `context7` | API key exfiltrace, závislost na externí službě | Používat `CONTEXT7_API_KEY` env proměnnou, ne hardcode. Volitelný — systém funguje i bez něj. |
| `sqlite` | Datová ztráta | Pravidelný backup, soubor v `.gitignore`? |
| `web-search` | Externí API volání | Rate limiting, monitoring |

---

## Rejstřík

- `28-Skills-and-Hooks.md` — Tento dokument
- `00-Index.md` — Hlavní index New_Architecture
- `01-Architectural-Guardrails.md` — Neporušitelné principy
- `02-Target-Repo-Structure.md` — Cílová struktura repa
- `03-Core-Abstractions.md` až `06-Core-Services.md` — Core vrstva
- `07-BusinessModel.md` — BusinessModel
- `08-Translator.md` — Translator
- `09-AI-Layer.md` — AI vrstva
- `10-Generators.md` — Generators
- `11-Infrastructure.md` — Infrastructure
- `12-Host-Surfaces.md` — Host surfaces
- `15-Test-Scaffold.md` — Testování
- `19-Error-Handling.md` — Error handling
- `25-DI-and-Composition-Root.md` — DI
- `26-Scaffold-Projects-and-Folders.md` — Scaffold
