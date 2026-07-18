# MetaForge V2 — Stavová analýza k 2026-07-18

> **Účel:** Upřímný přehled toho, co platforma v této fázi umí, co jí chybí a co brání produkčnímu spuštění.
> **Autor:** GitHub Copilot (DeepSeek V4 Pro)
> **Kontext:** Kompletní revize kódu, dokumentace, testů a issue trackeru.
> **Aktualizace:** 2026-07-18 — PROP-062 (FlowGraphSection), PROP-063 (odstranění explicitního workflow modelu), PROP-064 (post-removal cleanup). Workflow nahrazen odvozenou grafovou vizualizací z entit a relací.

---

## 1. Co platforma UMÍ (aktuální stav)

### ✅ Core vrstva — velmi solidní

- **Kompletní typový model** — `TypeModel`, `DataType`, `BaseType` (13 základních typů + generika + nullable)
- **Všechny elementy** — `ClassElement`, `InterfaceElement`, `EnumElement`, `StructElement`, `DelegateElement`, `EventElement`, `OperatorElement`, `ConstructorElement`, `FieldElement`, `PropertyElement`, `MethodElement`, `ParameterElement`
- **Expressions** (11+ typů) — Lambda, New, Default, Conversion, Await, Switch, IsPattern, NullCoalescing, Binary, Unary, MemberAccess, MethodCall, Constant
- **Statements** (13 typů) — If, For, ForEach, While, Switch, TryCatch, Using, UsingDeclaration, LocalFunction, Return, Assignment, Block, ExpressionStatement
- **Fluent Builder API** — 8 builderů + `ModelDefinition`
- **MetadataBag** — `MetadataScope`, `MergeStrategy`, zabudované klíče
- **DiagnosticBag** — `DiagnosticSeverity`, `ElementPath`, `Diagnostic`, `IDiagnosticCollector`, `BuildResult<T>` s monadickým `.Then()`/`.Map()`, 3 reportéry
- **TransformPipeline** — `IModelTransform`, `TransformContext`, `PipelineOptions`, `AttributeReflectionTransform`
- **Specification Layer** — `InvariantDefinition`, `InvariantExpression` (7 typů boolean AST), `ReflectionBasedInvariantEvaluator`, 12 BuiltInInvariantů
- **Composability** — `ElementMixin` (s `ConflictStrategy`: Skip/Throw/Replace), `ConventionRegistry` (3 vestavěné konvence), `ElementFingerprint` (SHA256 dirty-tracking, `IEquatable`)
- **StrongType/ValueObject systém** — Vogen konverze, `ValueObjectElement`, `ValueObjectValidationRule`
- **CatalogManager** — s provider registry (`BuiltInCatalogProvider`, `ICatalogProvider`)
- **CoreValidator** — 19 validačních kódů (C1–C14, M13–M14, P9–P10, G11–G12)
- **ForgeBlockRegistry** — `IForgeBlockPackage`, `IForgeBlockCapabilityPackage`, discovery metadata
- **AppRoot → Project → RootElement** hierarchie
- **IMemberElement** konzistence napříč Property, Method, Event, Operator
- **IMemberElement.Id** (Guid) — stabilní identita pro member elementy (PROP-060)
- **IElementIdResolver** — Core abstrakce pro Business→Core ID mapping, Core nezávisí na Translatoru (PROP-060)
- **DiRegistrationAttribute** — deklarativní DI registrace pro ForgeBlocky (PROP-054)
- **ForgeBlockRegistry.ApplyToDi()** — reflection-based DI registrace služeb (PROP-054)
- **ReferenceGraph** (5 souborů) — ID-based graf závislostí, Kahnův topologický sort + DFS detekce cyklů, `ReferenceKind`, `ReferenceCycle`, `UnresolvedReference`, `GetLayers()` (PROP-055)
- **~402 unit testů** (Core.Tests) + **52 integračních snapshot testů** (Core.Integration.Tests)

### ✅ BusinessModel — kompletní

- **BusinessAuthoringDocument** — entity, atributy, chování, relace, notes, pending questions, custom types
- **CommandLog** — append-only, `CommandEnvelope` s `Provenance`, `MutationId`, `SchemaVersion`, `StreamId`
- **ReplayEngine** — plný i inkrementální replay, `CommandMigrationEngine` pro schema migrace, default skip pro neznámé commandy
- **PatchEngine** — `IPatchOperation` operace (AddEntity, UpdateEntity, DeleteEntity, AddAttribute, SetCoreDetail, UpdateSyncState)
- **BusinessDocumentValidator** — `BusinessValidationIssue`, validační pipeline
- ~~Workflow model~~ — 6 typů odstraněno PROP-063 (2026-07-18). Náhrada: FlowGraphSection (PROP-062)
- **CoreDetail**, **SyncState**, **Provenance** systém
- **BusinessIdAllocator** — generování ID + human-readable slug
- **BusinessParameterNode.Id** — konzistentní identity napříč všemi BusinessModel typy (PROP-060)
- **SyncState** record — typovaný state machine místo enum. Exhaustive switch. `Conflict` nese kontext (PROP-060)
- **Telemetrie** — `ActivitySource` pro tracing

### ✅ Translator — solidní

- **BusinessAuthoringHostFacade** — jediný entry point, thread-safe (lock na `_document`)
- **ProjectionReadService** + **ProjectionView** + **ExpertProjectionView** (PROP-018)
- **ProjectionOptions** — Basic/Full s volitelnými sekcemi (Expert, AuthoringContext, DiscoveryContext)
- **FlowGraphSection** (PROP-062) — odvozená grafová vizualizace z entit a relací nad DocumentProjection. FlowNode, FlowEdge, FlowGraphBuilder. JsonCrack-kompatibilní.
- **DefaultBusinessTranslator** — business → TypeModel, `TranslateDocument()` pro celý dokument, StrongType mapping (PROP-047)
- **WriteBackService** — enrichment → `SetCoreDetailOp` přes PatchEngine
- **IAiTranslator** + **OllamaAiTranslator**
- **IBusinessTranslator.TryEnrichAsync()** — async enrichment v rozhraní (ISS-008)
- **ElementIdMapping** — Business→Core traceabilita. `Resolve()` pro PROP-055/056 (PROP-060)
- **~32 unit testů** (8 nových FlowGraphSection, 12 workflow testů odstraněno PROP-063)

### ✅ Generators — nejsilnější vrstva

- **CodeGenerator** (C#-only, `sealed`) — element switch → Scriban šablony
- **Všechny Scriban šablony** — `Class.scriban`, `Interface.scriban`, `Enum.scriban`, `Struct.scriban`, `Constructor.scriban`, `Field.scriban`, `Property.scriban`, `Method.scriban`, `Delegate.scriban`
- **ExpressionRenderer** — 58 unit testů (PROP-048)
- **StatementRenderer** — 13 unit testů
- **TemplateManager** — cached Scriban loader + `RegisterInlineTemplate()` pro plugin šablony (ISS-011)
- **13 E2E scénářů** pokrývajících všechny renderery vč. async/await (PROP-045 + ISS-016)
- **IncrementalCodeGenerator**, **TieredCodeGenerator** — wrapper pattern (kompozice, ISS-006)
- **ForgeBlock Packaging** (PROP-017) — `BlueprintBuilder`, `ForgeBlockPackageIntegrator`, `CodePackageDependency`
- **Packaging** — `PackageManifestGenerator`, `GeneratedArtifactComposer`, `ProjectScaffoldGenerator`
- **Expression.cs** — `Kind` jako computed property (ISS-004), `ExpressionKind` jako storage
- **91 unit testů** rendererů + 14 E2E snapshot testů

### ✅ AI vrstva

- **IAiBackendAdapter** + **OllamaAdapter** (HTTP `/api/generate`)
- **AiConstraintInferencer** — AI implementace `IConstraintInferencer`
- **AiTranslationService** — AI implementace `ITranslationService`
- **PromptRegistry** — verzované `.prompt.md` šablony s YAML frontmatter
- **PromptEvaluator** / **PromptEvaluationService**
- 3 prompt templates (constraint-inference, entity-suggestion, translation-enrich)
- **Graceful fallback** — vše vrací `null` při selhání

### ✅ Infrastructure

- **JsonCommandLogRepository** — JSONL append-only soubor, thread-safe
- **JsonDocumentRepository** — JSON snapshot celého dokumentu
- **InMemoryCommandLogRepository** — pro testy
- **CheckpointProjectionCache** — snapshoty dokumentu po N commandech
- **Konfigurace** — `MetaForgeOptions`, `StorageOptions`, `AiOptions` přes `IOptions<T>`
- **FileSystemProvider** — abstrakce IO pro testovatelnost
- **DI registrace** — `InfrastructureServiceRegistration`, `AiServiceRegistration`

### ✅ Host Surfaces (částečně)

- **CLI** (MetaForge.Cli):
  - System.CommandLine + Spectre.Console
  - **8 commandů**: `add-entity`, `list-entities`, `projection`, `add-attribute`, `delete-entity`, `info`, `generate`, `save`
  - `generate --output ./dir` — plná pipeline BusinessModel→Translator→CodeGenerator→soubory (CODE-001)
  - `save` — perzistence dokumentu přes `IDocumentRepository` (CODE-002)
  - **Perzistence**: JSONL CommandLog + JSON Document přes `InfrastructureServiceRegistration`
  - DI per-command isolation (`IServiceScope`) + `appsettings.json` konfigurace
  - Formátovaný výstup (tabulky, panely, barevné zprávy)
- **MCP** (MetaForge.Mcp):
  - JSON-RPC stdin/stdout loop
  - Dynamické tool discovery podle stavu dokumentu
  - Inicializační zpráva s tool listem

### ✅ ForgeBlocks

- **Math** — základní capability
- **String** — základní capability
- **Validation** — základní capability
- **AutoMapper** — capability package (Domain tier, 3 capability) + Scriban šablona `AutoMapperProfile.scriban`
- **EF Core** — capability package (Infrastructure tier, 5 capability) + Scriban šablony `DbContext.scriban`, `EntityTypeConfig.scriban`
- **FluentValidation** — capability package (Infrastructure tier, 3 capability) + Scriban šablona `FluentValidator.scriban`
- **Plugin systém**: `IForgeBlockTemplateProvider` (ISS-011), `DiRegistrationAttribute` + `ForgeBlockRegistry.ApplyToDi()` (PROP-054 — ✅ hotovo), `ForgeBlockPackageIntegrator` (PROP-017)

### ✅ Testy celkem (~600+)

| Projekt | Počet testů | Typ |
|---------|-------------|-----|
| MetaForge.Core.Tests | 402 | Unit + FsCheck + snapshot |
| MetaForge.Core.Integration.Tests | 52 | Snapshot-based (8+ scénářů) |
| MetaForge.Generators.Tests | 91 | Renderer unit + E2E |
| MetaForge.Translator.Tests | 32 | Unit (8 FlowGraphSection, 12 workflow odstraněno) |
| MetaForge.BusinessModel.Tests | 22 | Unit |

**Celkem: 612/612 testů ✅ — build 0 chyb, 4 warningy (preexistující)**

### ✅ Dokumentace

- 30 dokumentů v `New_Architecture/` — kompletní architektonická dokumentace
- 9 referenčních dokumentů v `Docs/Core/` — typový model, podpora, příklady
- **Support Matrix** (YAML) — 73+ položek, 5 kategorií, 4 contract statusy
- **38 PROPů hotových** (vč. PROP-062/063), 1 aktivní (PROP-058), 2 na zvážení (PROP-053, PROP-023), 1 zamítnutý
- `PROPOSALS.md`, `PROPOSALS_NEXT.md`, `Progress.md`, `Memories.md`
- AuditLog — 3 soubory (stavová analýza, overkill audit, analýza proti auditům)
- 309 C# souborů ve `Src/`

---

## 2. Co CHYBÍ k produkčnímu spuštění

### 🔴 KRITICKÉ — Blokátory launch (musí být hotovo před jakýmkoli spuštěním)

| ID | Problém | Kde | Dopad |
|----|---------|-----|-------|
| ✅ B1 | **Workflow obchází PatchEngine** — Vyřešeno PROP-044 (2026-07-10). Následně celý workflow model odstraněn PROP-063 (2026-07-18) — nahrazen FlowGraphSection (PROP-062). | — | ✅ Hotovo + odstraněno |
| ✅ B2 | **CLI chybí `generate`/`export`** — Vyřešeno CODE-001 (2026-07-12). `generate --output` + `save`. | `MetaForge.Cli/Program.cs` | ✅ Hotovo |
| ✅ B3 | **Chybí perzistence v CLI** — Vyřešeno CODE-002 (2026-07-12). JSONL + JSON, `appsettings.json` konfigurace. | DI registrace | ✅ Hotovo (MCP in-memory) |
| **B4** | **Monetizace odložena** — `IGenerationCostPolicy` není implementováno. Platforma může běžet bez monetizace. | ⚪ Odloženo | Plánováno po trakci |
| **B5** | **WebApi odloženo** — REST API host surface není potřeba. CLI + MCP pro MVP stačí. | ⚪ Odloženo | Až bude poptávka |
| ✅ B6 | **ExpertProjection neimplementováno** — Vyřešeno PROP-018 (2026-07-12). 6 modelů, ProjectionOptions. | `Translator/Host/` | ✅ Hotovo |

### 🟡 VÝZNAMNÉ — Nutné opravit před GA / veřejným spuštěním

| ID | Problém | Stav | Odhad |
|----|---------|------|-------|
| ✅ B7 | Async E2E (ISS-016) | Vyřešeno 2026-07-12 | — |
| ✅ B8 | ForgeBlock šablony (ISS-011) | Vyřešeno 2026-07-12 | — |
| ✅ B9 | Diff Modify (ISS-013) | Vyřešeno 2026-07-12 | — |
| B10 | **MapType TODO (3 místa)** | `CodeGenerator.cs:543-546` | 1 den |
| B11 | **Operator generování stub** | `CodeGenerator.cs:418` | 0.5 dne |
| ✅ B12 | TryEnrichAsync (ISS-008) | Vyřešeno 2026-07-12 | — |
| ℹ️ B13 | OllamaAiTranslator duplicita (ISS-007) | By design | — |
| ✅ B14 | CLI scoped services (ISS-009) | Vyřešeno 2026-07-12 | — |
| ✅ B15 | **CI/CD pipeline** — `.github/workflows/build.yml` vytvořen (2026-07-13). Build + testy všech projektů na push/PR. | ✅ Hotovo ❌ Chybí Docker image, NuGet publish | 0 |
| B16 | **Docker konfigurace chybí** | Není | 1-2 dny |

### 🟢 DROBNÉ / NÍZKÁ PRIORITA

| ID | Problém | Poznámka |
|----|---------|----------|
| ✅ B17 | CodeGenerator sealed (ISS-006) | Vyřešeno 2026-07-12 — `sealed` + wrapper pattern |
| ✅ B18 | Kind/ExpressionKind redundantní (ISS-004) | Vyřešeno 2026-07-12 — computed property |
| B19 | Chybí end-user dokumentace | README je technické, není uživatelsky orientované |
| ✅ B20 | ForgeBlock RequiredTier (ISS-010) | Vyřešeno — implementováno na EfCoreForgeBlock |
| B21 | Chybí health checks | Runtime health endpointy neexistují |
| B22 | Chybí setup/instalační skript | Uživatel musí ručně buildit |
| B23 | Test coverage nízký pro okrajové případy | Chybí FsCheck testy pro statementy |
| B24 | CoreValidator TODO | K1, M15, G07 |
| ✅ B25 | **Chybí generování DI registrací** | Vyřešeno 2026-07-17 — `DiRegistrationAttribute` + `ForgeBlockRegistry.ApplyToDi()` (PROP-054) |

---

## 3. Souhrnný verdikt

### Co MetaForge **je dnes**:

MetaForge je **velmi propracovaný C# framework pro modelování a generování kódu** s architekturou postavenou na event sourcování (CommandLog + Replay). V současné podobě je to funkční **development toolkit / knihovna** — můžete v kódu (unit testy) sestavit model, přeložit ho do Core elementů a nechat vygenerovat C#. Testy procházejí (~500+), Core je dobře otestovaný, generátor zvládá 13/13 scénářů včetně složitých konstrukcí (lambda, switch, async, foreach, try-catch, while).

**Reálně použitelné scénáře NYNÍ:**
- Embedovat MetaForge jako knihovnu do vlastní C# aplikace
- Použít MCP rozhraní pro AI-assisted modelování (např. z Copilotu)
- Programaticky vytvářet business modely a generovat C# kód (přes unit testy)

### Co MetaForge **NENÍ dnes**:

- **Není samostatně spustitelný produkt** — Nyní už ANO: CLI umí generovat C# kód a persistovat data.
- **Není monetizovatelný** — licence ani kredity nejsou implementované (**odloženo** — platforma může běžet bez monetizace).
- **Není nasaditelný** — chybí Docker (CI/CD je hotový).
- **Není ready pro reálné uživatele** — chybí end-user dokumentace, UX, instalační skript.

### Odhad práce k MVP spuštění: **~1–2 týdny**

| Fáze | Co udělat | Odhad | Stav |
|------|-----------|-------|------|
| **0. Blokátory** | Workflow (odstraněno PROP-063), generate command, persistence | ✅ Hotovo (0) | 2026-07-18 |
| **1. Productizace** | Docker, end-user README, ukázkové projekty | 2–4 dny | ⬜ CI/CD ✅, zbytek ❌ |
| **2. ForgeBlock DI** | PROP-054: `DiRegistrationAttribute`, `ApplyToDi()` | ✅ Hotovo | 2026-07-17 |
| **3. Reference Graph** | PROP-055: ID-based graf závislostí | ✅ Hotovo | 2026-07-17 |
| **4. Element Identity** | PROP-060: `IMemberElement.Id`, `SyncState`, `ElementIdMapping` | ✅ Hotovo | 2026-07-17 |
| **5. Projection Unification** | PROP-056: `DocumentProjection`, `ProjectionFilter`, `DependencyGraphSection` | ✅ Hotovo | 2026-07-17 |
| **6. ElementContract** | PROP-057: `ContractValue`, `ElementContract`, `EntityContract`, `MethodContract` | ✅ Hotovo | 2026-07-17 |
| **7. Vylepšení** | Opravit B10 (MapType TODO), B11 (Operator stub), B19 (docs) | 2 dny | ❌ |
| **8. Monetizace** | `IGenerationCostPolicy` — **odloženo** po ověření trakce | — | ⚪ |
| **9. WebApi** | `MetaForge.WebApi` — **odloženo**, není potřeba pro MVP | — | ⚪ |

### Hlavní riziko:

Platforma je **architektonicky vyspělá a produktově dozrávající**. Všechny kritické blokátory jsou vyřešeny. CI/CD pipeline běží (GitHub Actions, build + testy na push/PR). Přibyly nové capability (ReferenceGraph, ElementContract, DI registrace, Element Identity). Hlavní riziko je **absence Docker image** (nelze snadno nasadit/demo) a **chybějící end-user dokumentace**.

### Doporučení:

1. ✅ **Blokátory vyřešeny** — B1–B3, B15 (CI/CD), B25 (DI registrace) hotovo. CLI je funkční nástroj s generováním a perzistencí.
2. ✅ **ReferenceGraph + ElementContract + Element Identity** — PROP-055/056/057/060 hotovo. Core má nyní ID-based referenční graf, kontraktní model a stabilní identity.
3. **Další krok: Productizace** — Docker image, end-user README, PROP-058 (Sandbox Preview Runner).
4. **Monetizace a WebApi** — řešit až po ověření trakce a poptávce.

---

## 4. Příloha: Přehled otevřených ISS

| ISS | PROP | Popis | Stav |
|-----|------|-------|------|
| ISS-001..016 | — | Všech 16 issues | ✅ Vyřešeno (2026-07-12) |
| ISS-007 | PROP-019 | `OllamaAiTranslator` duplicitní logika | ℹ️ By design |
