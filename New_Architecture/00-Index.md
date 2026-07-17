# New_Architecture — Index

> Kompletní architektonická dokumentace MetaForge pro C#-first implementaci.
> Dokumenty jsou řazeny do tematických celků a vzájemně na sebe navazují.
> Poslední aktualizace: 2026-07-18 (PROP-061 — Authoring Feedback Platform)

---

## Přehled souborů

### Architektura a cíle
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 00 | `00-Index.md` | Tento index | ✅ |
| 01 | `01-Architectural-Guardrails.md` | Architektonické principy a guardrails | ✅ |
| 02 | `02-Target-Repo-Structure.md` | Cílová struktura repozitáře | ✅ |

### Core vrstva
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 03 | `03-Core-Abstractions.md` | AppRoot, RootElement, TypeModel, DataType, SemanticCollection, MetadataBag | ✅ |
| 04 | `04-Core-Elements.md` | Všechny elementy: Class, Interface, Enum, Struct, Delegate, Event, Operator, Constructor, Field, Property, Method, Parameter, IMemberElement (PROP-040), MetadataBag (PROP-038) | ✅ |
| 05 | `05-Core-Behaviors.md` | Expression System (14 druhů), Statement System (13 typů), DiagnosticBag, BuildResult\<T\>, TransformPipeline (PROP-038), InvariantDefinition + InvariantExpression AST (PROP-036), ElementMixin + ConventionRegistry (PROP-039), ElementFingerprint (PROP-039), CoreValidator (19 kódů) | ✅ |
| 06 | `06-Core-Services.md` | CatalogManager, ICatalogProvider, StrongType/ValueObject, IConstraintInferencer, ForgeBlockRegistry | ✅ |

### Business a Translator vrstva
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 07 | `07-BusinessModel.md` | BusinessAuthoringDocument, CommandLog, PatchEngine, ReplayEngine, Workflow, Validace, BusinessDocumentDiffer (vč. Modify detekce ISS-013) | ✅ |
| 08 | `08-Translator.md` | Facade, Projection + ExpertProjection (PROP-018), WriteBack, IBusinessTranslator (vč. TryEnrichAsync ISS-008), DefaultBusinessTranslator, LanguageCapabilityProfile | ✅ |
| 09 | `09-AI-Layer.md` | IAiBackendAdapter, OllamaAdapter, PromptRegistry, PromptEvaluator, AiTranslationService, AiRepairSuggestionService (PROP-061), AiServiceRegistration | ✅ |

### Generátory, Feedback, Infrastructure a Monetizace
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 10 | `10-Generators.md` | CodeGenerator (sealed), Scriban šablony, ExpressionRenderer (58 unit testů), StatementRenderer (13 unit testů), TemplateManager (vč. RegisterInlineTemplate), 13 E2E scénářů, ForgeBlock Packaging, TieredCodeGenerator. **PROP-061:** MapType Array/Nullable, IsKnownPrimitive, AsyncLocal diagnostika, DiagnosticInfo.Code | ✅ |
| **—** | **PROP-061 — Authoring Feedback Platform** | `MetaForge.Feedback` projekt: `IAuthoringFeedbackService`, `AuthoringFeedbackRecord` (wrapper nad `Diagnostic` Core), `RepairRecommendation`, `ActiveFeedbackCache`, `FeedbackLearningArchive`, `FeedbackLearningExporter`, `IFeedbackCacheRepository`, `IFeedbackLearningRepository`, `IRepairSuggestionService` (Translator), `AiRepairSuggestionService` (AI), CLI `list-feedback`, MCP `get_feedback`/`dismiss_feedback`. `StorageOptions` rozšířen. `DiagnosticInfo.Code` přidán. | ✅ |
| 11 | `11-Infrastructure.md` | Persistence: JsonCommandLogRepository (true async I/O ISS-002), JsonDocumentRepository, InMemoryCommandLogRepository, IOptions konfigurace, CheckpointProjectionCache, FileSystemProvider, InfrastructureServiceRegistration. **PROP-061:** JsonFeedbackCacheRepository, JsonFeedbackLearningRepository, StorageOptions.FeedbackCachePath/LearningArchivePath | ✅ |
| 12 | `12-Host-Surfaces.md` | CLI: 9 commandů (add-entity, list-entities, projection, add-attribute, delete-entity, info, generate, save, list-feedback), per-command IServiceScope (ISS-009), persistence v DI (CODE-002), generate pipeline (CODE-001). MCP: JSON-RPC + discovery + feedback tools. WebApi: odloženo. | ✅ |
| 29 | `29-Monetization.md` | Kreditový systém, tier licence, MCP-ready billing gate. Implementace odložena (CODE-003). | ⏳ Odloženo |

### Plánování a proces
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 13 | `13-Epics-and-Slices.md` | User stories, slices 1-7 | ✅ |
| 14 | `14-Atomic-Tasks.md` | Detailní atomické tasky | ✅ |

### Kvalita a testování
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 15 | `15-Test-Scaffold.md` | Struktura testů (~500+ unit, 48 snapshot, 13 E2E), SnapshotComparer s UPDATE_SNAPSHOTS, SyntaxValidator, Support Matrix YAML (73 položek) | ✅ |
| 16 | `16-Risks-and-Rollback.md` | Rizika, rollback strategie | ✅ |

### Agents a governance
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 17 | `17-Skills-and-Agents.md` | Definice skills a agentů | ✅ |
| 18 | `18-Ready-to-Run-Prompts.md` | Prompty pro malý model | ✅ |
| 19 | `19-Error-Handling.md` | Error handling, exception politika | ✅ |
| 20 | `20-Security.md` | Bezpečnostní model | ✅ |
| 21 | `21-Telemetry.md` | Telemetrie a observabilita | ✅ |
| 22 | `22-CI-CD.md` | CI/CD pipeline — `.github/workflows/build.yml` ✅ Build+testy, Docker a NuGet ⏳ odloženo | ✅ build.yml |
| 23 | `23-Governance.md` | Decision log, ADRs, dokumentační standardy | ✅ |
| 24 | `24-Markdown-First-Workflow.md` | Pravidla pro markdown-first vývoj | ✅ |

### DI a scaffold
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 25 | `25-DI-and-Composition-Root.md` | DI registrace, lifetime, Composition Root. **ForgeBlock DI extension methods naplánovány (PROP-054).** | ⏳ PROP-054 |
| 26 | `26-Scaffold-Projects-and-Folders.md` | Scaffold projektů a složek | ✅ |

### ForgeBlock knihovny
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 27 | `27-ForgeBlock-External-Libraries.md` | Katalog externích C#/.NET knihoven pro ForgeBlock integraci | ✅ |

### Konceptuální přehled
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 30 | `ReadMe-Architecture-Summary.md` | Konceptuální přehled platformy, editační matice, principy, data flow | ✅ |

---

## Navigace podle vrstev

```
┌─────────────────────────────────────────────┐
│           12-Host-Surfaces.md                │  CLI (8 cmd) · MCP · ~~WebApi~~ (odloženo)
├─────────────────────────────────────────────┤
│           08-Translator.md                   │  Facade · Projection · ExpertProjection · WriteBack
│           09-AI-Layer.md                     │  AI inference · Translation
├─────────────────────────────────────────────┤
│           07-BusinessModel.md                │  Document · CommandLog · Replay · Diff
├─────────────────────────────────────────────┤
│  03-Abstractions  04-Elements                │
│  05-Behaviors     06-Services                │  Core vrstva
├─────────────────────────────────────────────┤
│           11-Infrastructure.md               │  Persistence · Config · FileSystem
├─────────────────────────────────────────────┤
│           10-Generators.md                   │  Code export · Templates · Packaging
├─────────────────────────────────────────────┤
│           27-ForgeBlock-External-Libraries.md│  EF Core · AutoMapper · FluentValidation
└─────────────────────────────────────────────┘
```

## Stav implementace — klíčové milestone

| Milestone | Stav | Datum |
|-----------|------|-------|
| Core vrstva — kompletní (všechny elementy, expressions, statements) | ✅ | 2026-07-08 |
| BusinessModel — dokument, CommandLog, PatchEngine, ReplayEngine, Workflow | ✅ | 2026-07-04 |
| Translator — Facade, Projection, WriteBack, ExpertProjection | ✅ | 2026-07-12 |
| AI Layer — OllamaAdapter, PromptRegistry, graceful fallback | ✅ | 2026-07-04 |
| Generators — CodeGenerator, ExpressionRenderer, 13 E2E scénářů | ✅ | 2026-07-11 |
| ForgeBlock Packaging — BlueprintBuilder, PackageIntegrator, plugin šablony | ✅ | 2026-07-12 |
| Testy — ~500+ unit, 48 snapshot, 13 E2E, UPDATE_SNAPSHOTS | ✅ | 2026-07-11 |
| CLI — generate command, perzistence, 8 commandů | ✅ | 2026-07-12 |
| ExpertProjection — 6 modelů, ProjectionOptions, diagnostika | ✅ | 2026-07-12 |
| Issues cleanup — 15/16 resolved | ✅ | 2026-07-12 |
| **ForgeBlock DI extension methods** | ⏳ PROP-054 plán | 2026-07-13 |
| **CI/CD pipeline** | �N | — |
| **Monetizace** | ⏳ Odloženo | — |
| **WebApi** | ⏳ Odloženo | — |
├─────────────────────────────────────────────┤
|  13-14 Planning · 15-28 Quality/Scaffold/Agents · 29 Monetizace · 30 Architecture Summary │
└─────────────────────────────────────────────┘
```

## Návaznosti mezi dokumenty

| Dokument | Předpokládá | Popis |
|----------|------------|-------|
| 03-06 | 01 | Core vrstva — žádné závislosti na vyšších vrstvách |
| 07 | 03-06 | BusinessModel staví na Core abstrakcích |
| 08 | 07 | Translator používá BusinessModel |
| 09 | 08 | AI implementace Translator kontraktů |
| 10 | 07 | Generátory čtou BusinessModel |
| 11 | 07 | Infrastructure implementuje persistence kontrakty |
| 12 | 08 | Host surfaces volají Facade |
| 13-29 | 03-12, 29 | Plánování, kvalita, scaffold, monetizace |
| 27 | 03-12, 05-ForgeBlock-Package-Model | Externí knihovny pro ForgeBlock integraci |

---

## Pravidla

1. **C#-first**: Všechny příklady a implementace jsou v C#. Žádné polyglot koncepty (ProgramLanguage, LanguageMapping v minulosti).
2. **AppRoot → Project → RootElement**: AppRoot je vstupní bod, obsahuje projekty, projekt obsahuje RootElement.
3. **Žádná business logika v host vrstvě**: CLI, MCP, WebApi volají pouze `BusinessAuthoringHostFacade`.
4. **Kontrakty v Core/Translator, implementace v Infrastructure/Ai**: Core definuje rozhraní, implementace jsou oddělené.


### PROP-057 (✅ implementováno 2026-07-17)
- 7 Core typů: ContractValue (9 sealed potomků), ElementContract, EntityContract, MethodContract, ContractScenario, ContractInvariant, ScenarioExpectation
- 2 Infrastructure typy: VerificationState, IVerificationStateStore
- ClassElement.Contract (EntityContract?), MethodElement.Contract (MethodContract?)
- ElementFingerprint.ContractHash

### PROP-056 (✅ implementováno 2026-07-17)
- DocumentProjection (unifikace ProjectionView + ExpertProjectionView)
- ProjectionFilter + ProjectionPresets (Basic/Expert/AiEnrichment)
- DependencyGraphSection (PROP-055 synergie)
- CoreId na EntityProjection, AttributeProjection, BehaviorProjection, RelationProjection

### PROP-058 (✅ kontrakty hotovy 2026-07-17)
- ISandboxExecutionService, SandboxExecutionRequest/Result
- SandboxMode (Preview/Export)
- Roslyn kompilace MVP zbývá
