# MetaForge — Vrstvy platformy

Datum: 2026-04-17
Status: Živý dokument — aktualizováno 2026-05-14 (Plán 35-37 — reorganizace Core, migrace renderingu, WebApi cleanup)

---

## Přehled vrstev

```
┌─────────────────────────┐
│   Host Surface           │  MCP / CLI / WebApi
├─────────────────────────┤
│   Facade                 │  BusinessAuthoringHostFacade
├─────────────────────────┤
│   Business Model         │  BusinessAuthoringDocument + CommandLog
├─────────────────────────┤
│   Translator             │  Business → Core překlad + Expert projekce
├─────────────────────────┤
│   Core                   │  Jazykově agnostický typový model
├─────────────────────────┤
│   Generators             │  Jazykově specifická generace kódu
└─────────────────────────┘
```

Závislosti tečou **shora dolů**. Žádná vrstva nesmí záviset na vrstvě nad ní.

### Nové čtení vrstev

Tyto vrstvy už nemají být čteny jen jako pipeline pro codegen. Správné čtení je:

- Host Surface sbírá authoring intent a vydává read nebo write operace.
- Facade orchestrace drží jednotný vstup do authoring kernelu.
- Business Model je source of truth pro business detail, workflow, pending questions a write-back detail.
- Translator zajišťuje enrichment, diagnostiku a přípravu výstupních profilů.
- Core nese jazykově neutrální sémantiku, capability metadata a typový model.
- Generators a další exporty jsou až výstupní adaptéry nad stejným authoring základem.

Podrobné rozpracování viz `09-Authoring-Kernel-and-Multi-Output-Model.md`.

---

## Vrstva 1 — Host Surface

**Projekty:** `MetaForge.Mcp`, `MetaForge.Cli`, `MetaForge.WebApi`

### Odpovědnost
- Vstupní bod pro uživatele nebo AI klienta
- Tenká vrstva — žádná business logika
- Mapuje vstupy na volání `BusinessAuthoringHostFacade`
- Formátuje výstupy pro daný povrch (JSON pro MCP, text pro CLI)
- Vystavuje authoring kernel jako bezpečný host surface pro člověka i pro AI klienta
- Drží telemetry wiring, resource atributy a volitelné OTLP nebo Aspire napojení

### MCP tools (dnes)
| Tool | Popis |
|------|-------|
| `GetBusinessProjection` | Replay projekce dokumentu |
| `GetBusinessExpertProjection` | Expert projekce s diagnostikou a suggestions |
| `AddEntity`, `UpdateEntity` | Mutace entit |
| `AddAttribute`, `UpdateAttribute` | Mutace atributů (včetně presetId, constraints) |
| `GetBusinessCoreProjection` | Přeložený Core model |
| `GetDiscoveryHelp`, `QueryDiscovery` | Discovery — categories, capabilities, tools (help pro AI klienta) |
| `SuggestNodePresets` | Deterministický návrh presetů pro nový node |
| `AssistNode` | AI asistence pro existující node — preview návrh |
| `ApplyNodeAssist` | Explicitní aplikace AI-navržených operací |

### WebApi endpointy (dnes)
| Endpoint | Popis |
|----------|-------|
| `GET /document` | Aktuální dokument jako JSON |
| `GET /document/projection` | Structured projection s volitelnými sekcemi |
| `POST /entities`, `/attributes`, `/behaviors`, `/relations` | CRUD operace |
| `POST /assist` | AI node-level asistence |
| `POST /assist/apply` | Explicitní apply AI operací |
| `POST /presets/suggest` | Deterministický preset suggestor |
| `POST /operations` | Batch apply BusinessPatchOperation |

### Plánované změny
- Sloučení `GetBusinessProjection` + `GetBusinessExpertProjection` → jedna tool s `detail` parametrem
- Přidání `ApplyEnrichment(attributeId, presetId)` — semi-automatický write-back
- Každý ForgeBlock balíček může přidat vlastní MCP tools přes `IForgeBlockMcpPackage.RegisterMcp()`
- Kategorie `tools` v Discovery — AI klient se dozví dostupné tools a kdy je použít
- Observability export zůstane opt-in a host-specific; host jen připojuje export, ale nedefinuje business telemetry taxonomii

---

## Vrstva 2 — Facade

**Projekt:** `MetaForge.Translator` — `BusinessAuthoringHostFacade`

### Odpovědnost
- Sdílená vstupní brána pro MCP i CLI — jeden kód, dvě rozhraní
- Orchestruje read path (projekce) a write path (patching, AI, shadow log)
- Izoluje hosty od interní složitosti `AuthoringConversationService`
- Rozlišuje mezi authoring operací, enrichment/write-back tokem a výstupním profilem
- Je primární hranicí pro platformní use-case metriky a nízkokardinalitní telemetry tagy

### Klíčové metody (dnes)
```csharp
GetProjectionAsync(options)         // read — sjednocená projekce
GetCurrentReadDocument()            // read — aktuální dokument
AssistNodeAsync(request)            // read — AI node-level asistence (preview)
ApplyNodeAssistOperations(path, ops)// write — explicitní apply AI operací
SuggestNodePresets(context)         // read — deterministický preset suggestor
ApplyOperations(ops)                // write — explicitní patch operace
```

### Plánované změny
- `GetCurrentProjectionView` + `GetCurrentExpertProjection` → `GetProjectionAsync(DetailLevel)`
- `ApplyEnrichment(attributeId, presetId)` — nová mutační metoda pro write-back
- Sdílený `Meter` a jednotná naming konvence pro authoring, projection, discovery a export metriky

---

## Vrstva 3 — Business Model

**Projekt:** `MetaForge.BusinessModel`

### Odpovědnost
- **Single source of truth** — `BusinessAuthoringDocument`
- Append-only CommandLog — každá změna je command
- Replay — deterministická rekonstrukce dokumentu z commandů
- Patch engine — aplikace `BusinessPatchOperation` na dokument

### Klíčové typy
```
BusinessAuthoringDocument
├── BusinessProjectInfo
├── Workflow section (cílový směr)
├── BusinessEntityNode
│   ├── BusinessAttributeNode
│   │   ├── Id, Name, Type, Required, Summary
│   │   ├── DefaultValue, Constraints, Computed
│   │   ├── PresetId (transport hint)
│   │   └── CoreDetail (plánováno — write-back sekce)
│   ├── BusinessBehaviorNode
│   └── BusinessNoteNode
├── BusinessRelationNode
└── PendingQuestionNode
```

### CommandLog
```
CommandEnvelope[]  (JSONL append-only soubor)
├── Id, StreamId, Timestamp
├── CommandName, Source (AI / CLI / Hybrid)
└── Payload (BusinessPatchOperation data)
```

Replay = `JsonlShadowCommandReader` → `BusinessProjectionReducer` → `BusinessAuthoringDocument`

### Plánované změny
- `BusinessAttributeNode.CoreDetail` — nová sekce pro write-back výsledků překladu
- `AttributeSyncState` — stavový automat computed při replay
- Nové operace: `apply_preset`, `enrich_attribute`, `update_core_detail`
- Workflow uzly a workflow commandy jako další first-class business artefakty
- `PendingQuestionNode` a workflow kontext jako stabilní vstup pro AI i expert projekce

---

## Vrstva 4 — Translator

**Projekt:** `MetaForge.Translator`

### Odpovědnost
- Překlad `BusinessAuthoringDocument` → `MetaForgeTransportDto` (Core reprezentace)
- Expert projekce — diagnostika, type resolution, suggestions
- AI klienti — authoring conversation, translation
- Orchestrace write path v `AuthoringConversationService`
- Příprava různých výstupních profilů nad jedním authoring modelem
- Enrichment business i workflow detailu před write-backem

### Klíčové komponenty
| Komponenta | Popis |
|-----------|-------|
| `DefaultBusinessTranslator` | Deterministický překlad Business → Core (bez AI) |
| `HybridTranslator` | AI-asistovaný překlad s fallback na deterministický |
| `ExpertProjectionBuilder` | Sestaví `ExpertProjectionView` s diagnostikou a type resolution |
| `AuthoringConversationService` | Orchestrátor write path — AI + patching + shadow log |
| `CatalogManager` (z Core) | Lookup presetů pro type resolution |

### Type resolution (ExpertProjectionBuilder)
```
BusinessAttributeNode.Type ("email")
  ├── CatalogManager.Lookup("email") → nalezeno → ResolutionKind: Catalog
  ├── DataType.* mapování          → primitiv → ResolutionKind: Primitive
  ├── CustomType nastaveno         → vlastní typ → ResolutionKind: Custom
  └── nic nenalezeno               → ResolutionKind: Fallback
```

### Plánované změny
- `ExpertProjectionBuilder` jako `EnrichStage` uvnitř interní pipeline
- Write-back: výsledek type resolution → `apply_preset` / `enrich_attribute` command
- Read/Write split: `AuthoringConversationService` → `ProjectionReadService` + `CommandWriteService`
- Workflow projection a workflow binding diagnostika jako další projekční sekce
- Output-neutral projection model: codegen je jeden z konzumentů, ne jediný cíl vrstvy

---

## Vrstva 5 — Core

**Projekt:** `MetaForge.Core`

### Odpovědnost
- **Jazykově agnostický** typový model — nesmí obsahovat C#-specific logiku
- Typový model (`TypeModel`, `DataType`, `SemanticCollection`)
- Elementy (`Class`, `Property`, `Method`, `Enum`, `Interface`, `Struct`)
- Katalog presetů a ForgeBlock pluginy
- Validace (`Zero-Fault` princip — invalidní model se nesmí exportovat)
- **ForgeBlock package registry** — bootstrap a správa pluginů
- **Discovery infrastruktura** — `IDiscoverySession`, `DiscoveryQuery`, federovaný model
- Neutrální capability metadata použitelná pro codegen, workflow binding i host export

### Klíčové typy
```
MetaProject
└── Classes[]
    ├── Name, Namespace, Summary, Icon, PresetId
    └── Properties[]
        ├── TypeModel
        │   ├── BaseType (DataType enum)
        │   ├── CustomTypeName
        │   ├── IsNullable, IsCollection
        │   ├── SemanticCollection (List/Set/Dictionary/...)
        │   └── CurrentSyntax (computed per language)
        └── EntityKind (Primitive / ValueObject / ...)

CatalogManager
├── Presety (email-vo, money-amount, phone-number, ...)
└── ICatalogProvider[] (BuiltIn, ForgeBlockRegistryCatalogProvider, FileSystem)

ForgeBlockPackageRegistry
├── IForgeBlockCapabilityProvider[] — capabilities
├── IForgeBlockGeneratorContributor[] — šablony + codegen
├── IForgeBlockCatalogContributor[] — presety, katalog
└── IForgeBlockDiscoveryContributor[] — help, handles, usage examples

DefaultDiscoverySession
├── types — DataType enum
├── presets — CatalogManager presety
├── capabilities — ForgeBlock capabilities
├── blocks — template blocks (drill-down)
└── tools — (plánováno) MCP tools a CLI příkazy
```

### ForgeBlock package model
Každý balíček implementuje `IForgeBlockPackage` a sám se zaregistruje do 4 Core facetů:
- **Capability** — co balíček umí (semantic handles, tagy, popis)
- **Generator** — šablony a codegen pro každý jazyk
- **Catalog** — presety a katalogové záznamy
- **Discovery** — help texty, usage examples, drill-down do items

Rozšířené čtení: ForgeBlock není jen codegen plugin. Je to capability balíček, který může dodat sémantiku, katalog, discovery, workflow bindingy a host-specific surface mapování.

Math, Random, Mapper — hotové built-in balíčky. ORM, CQRS, API, Vogen — plánované.

Viz [05-ForgeBlock-Package-Model.md](05-ForgeBlock-Package-Model.md) pro detailní popis včetně plánované multi-vrstvé registrace.

### Guardrails
- Žádná C#-specific logika v Core
- Jazykové mapování výhradně v `TypeModel.GetSyntax(language)` přes switch
- Nové datové typy pouze přes `DataType` enum + mapování ve všech jazycích
- Core nesmí záviset na Translator, MCP ani CLI
- Core nesmí obsahovat OpenTelemetry exporter wiring, Aspire coupling ani host telemetry bootstrap

---

## Vrstva 6 — Generators

**Projekt:** `MetaForge.Generators`

### Odpovědnost
- Generace zdrojového kódu z `MetaForgeTransportDto`
- Každý jazyk má vlastní složku s generátorem a šablonami
- Jazykově specifická logika (imports, namespaces, syntaxe) patří **výhradně sem**

Codegen je důležitá, ale nikoliv jediná výstupní vrstva. Vedle něj může platforma vydávat workflow artefakty, capability surface a diagnostické projekce nad týmž authoring modelem.

### Jazyky
| Jazyk | Složka | Status |
|-------|--------|--------|
| C# | `CSharp/` | ✅ Aktivní |
| TypeScript | `TypeScript/` | ✅ Aktivní |
| Python | `Python/` | ✅ Aktivní |
| Java | `Java/` | ✅ Aktivní |
| Go | `Go/` | ✅ Aktivní |

---

## Hranice mezi vrstvami — co nesmí

| Hranice | Co nesmí přejít |
|---------|-----------------|
| Core → Business | Core nesmí importovat BusinessModel typy |
| Core → Translator | Core nesmí importovat Translator typy |
| Core → Mcp/Cli | Core nesmí importovat MCP ani CLI typy |
| Generators → Business | Generátory pracují pouze s `MetaForgeTransportDto`, ne přímo s `BusinessAuthoringDocument` |
| Host Surface → Core | MCP/CLI nesmí volat Core přímo, jen přes Facade |
| Business Model → Translator | BusinessModel nesmí importovat Translator (jednosměrná závislost) |
| ForgeBlock balíček | Závisí na Core + cílové vrstvě (MCP/CLI/Translator) — nikdy naopak |

---

## Existující kód — klíčové soubory per vrstva

> Při implementaci změn začni od těchto souborů — nepřepisuj od nuly, ale rozšiřuj.

| Vrstva | Klíčový soubor | Co řeší |
|--------|---------------|---------|
| **BusinessModel** | `Src/MetaForge.BusinessModel/CommandLog/ReplayProjectionQueryService.cs` | Replay projekce z command logu |
| **BusinessModel** | `Src/MetaForge.BusinessModel/CommandLog/BusinessProjectionReducer.cs` | Aplikace commandů na dokument |
| **Translator** | `Src/MetaForge.Translator/Host/BusinessAuthoringHostFacade.cs` | Sdílená facade MCP i CLI |
| **Translator** | `Src/MetaForge.Translator/Conversation/AuthoringConversationService.cs` | Orchestrace AI + write + read |
| **Translator** | `Src/MetaForge.Translator/Host/ExpertProjection.cs` | Expert projekce s diagnostikou |
| **Core — Catalog** | `Src/MetaForge.Core/Catalog/CatalogManager.cs` | Orchestrátor presetů — `ResolveType`, `SuggestPresets` |
| **Core — Catalog** | `Src/MetaForge.Core/Catalog/NodePresetSuggester.cs` | Deterministický suggestor pro node create — ranking |
| **Core — AST** | `Src/MetaForge.Core/Elements/Expressions/Ast/AstFactory.cs` | JSON parsing AST nodů (factory) |
| **Core — AST Nodes** | `Src/MetaForge.Core/Elements/Expressions/Ast/Nodes/` | 16 AST node typů (1 typ = 1 soubor, namespace `Ast.Nodes`) |
| **Core — Testing** | `Src/MetaForge.Core/Internal/Testing/RoslynTestRunner.cs` | Orchestrátor boundary testů — deleguje na `CSharpMethodGenerator`, `BoundaryTestGenerator`, `RoslynCompiler`, `TestExecutor` |
| **Translator — Assist** | `Src/MetaForge.Translator/Host/NodeAssistContextBuilder.cs` | Builder node-scoped kontextu z ProjectionView |
| **Translator — Assist** | `Src/MetaForge.Translator/Conversation/NodeAssistService.cs` | AI orchestrátor pro node-level asistence |
| **Translator — Assist** | `Src/MetaForge.Translator/Host/NodeAssistOperationValidator.cs` | Whitelist + entity scope validace před apply |
| **Core — Catalog** | `Src/MetaForge.Core/Catalog/BuiltInCatalogProvider.cs` | Embedded Core presety (Priority 0) |
| **Core — Catalog** | `Src/MetaForge.Core/Catalog/FileSystemCatalogProvider.cs` | Uživatelské presety `~/.metaforge/presets/` (Priority 10) |
| **Core — ForgeBlocks** | `Src/MetaForge.Core/ForgeBlockPackages/IForgeBlockPackage.cs` | Registrační interface — 4 Core facety |
| **Core — Discovery** | `Src/MetaForge.Core/Discovery/DefaultDiscoverySession.cs` | Kategorie types/presets/capabilities/blocks/methods |
| **AI** | `Src/MetaForge.Ai/Configuration/AiPlatformConfiguration.cs` | Centrální konfigurace — AiSegment enum + per-segment settings |
| **AI** | `Src/MetaForge.Ai/Runtime/IAiRuntimeAdapter.cs` | HTTP abstrakce nad AI backendem |
| **AI** | `Src/MetaForge.Core/Inference/IAiConstraintInferencer.cs` | Core-side AI interface (IConstraintInferencer) |
| **Host Surface — WebApi** | `Src/MetaForge.WebApi/Program.cs` | ASP.NET Core Minimal API — CORS, Swagger, routing |
| **Host Surface — WebApi** | `Src/MetaForge.WebApi/Endpoints/DiscoveryEndpoints.cs` | Assist, presets, discovery endpointy |
| **Host Surface** | `Src/MetaForge.Cli/Commands/BusinessCliCommands.cs` | CLI příkazy pro business authoring |
| **Kredity — Core** | `Src/MetaForge.Core/Common/Package.cs` | `TotalCreditScore` + `RecalculateCreditScore()` |
| **Kredity — Core** | `Src/MetaForge.Core/Elements/Types/Class.cs` | `CreditScore` + `CalculateTotalCreditScore()` |
| **Kredity — Generators** | `Src/MetaForge.Generators/BaseCodeGenerator.cs` | `CreditCostPerElement` — jazykový multiplikátor |
| **Kredity — DTO** | `Src/MetaForge.Dto/TransportContracts.cs` | `CreditScore` přenášen v DTO |
