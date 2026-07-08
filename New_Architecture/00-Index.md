# New_Architecture — Index

> Kompletní architektonická dokumentace MetaForge pro C#-first implementaci.
> Dokumenty jsou řazeny do tematických celků a vzájemně na sebe navazují.

⚠️ **Aktuální stav: work-in-progress.** 03 je částečný (potřeba dočistit starý scaffold content ze staré 06).

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
| 03 | `03-Core-Abstractions.md` | AppRoot, RootElement, TypeModel, DataType, SemanticCollection, MetadataBag (PROP-038) | ✅ *(PROP-035, PROP-038)* |
| 04 | `04-Core-Elements.md` | Class, Interface, Enum, Struct, Property, Method, Parameter elem. + MetadataBag (PROP-038) | ✅ *(PROP-035, PROP-038)* |
| 05 | `05-Core-Behaviors.md` | Expression System (11+8 druhů), Statement System (PROP-031), DiagnosticBag, BuildResult\<T\>, TransformPipeline (PROP-038) | ✅ *(PROP-024, PROP-031, PROP-035, PROP-038)* |
| 06 | `06-Core-Services.md` | CatalogManager, ICatalogProvider, StrongType/ValueObject, IConstraintInferencer | ✅ *(PROP-024)* |

### Business a Translator vrstva
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 07 | `07-BusinessModel.md` | BusinessAuthoringDocument, CommandLog, Patches, Workflow, Validace, Identity | ✅ *(PROP-020)* |
| 08 | `08-Translator.md` | Facade, Projection, WriteBack, ITranslationService, DefaultBusinessTranslator, LanguageCapabilityProfile (PROP-035) | ✅ *(PROP-020, PROP-035)* |
| 09 | `09-AI-Layer.md` | IAiBackendAdapter, OllamaAdapter, PromptRegistry, PromptEvaluator, AiServiceRegistration | ✅ *(PROP-027)* |

### Generátory, Infrastructure a Monetizace
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 10 | `10-Generators.md` | CodeGenerator, Scriban šablony, ExpressionRenderer, Packaging | ✅ |
| 11 | `11-Infrastructure.md` | Persistence (JSONL), IOptions konfigurace, checkpoint caching, FileSystem | ✅ *(PROP-028)* |
| 12 | `12-Host-Surfaces.md` | CLI (System.CommandLine + Spectre.Console), MCP (JSON-RPC + discovery), WebApi (neimplementováno) | ✅ *(PROP-026)* |
| 29 | `29-Monetization.md` | Kreditový systém, tier licence, MCP-ready billing gate | ✅ |

### Plánování a proces
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 13 | `13-Epics-and-Slices.md` | User stories, slices 1-7 | ✅ |
| 14 | `14-Atomic-Tasks.md` | Detailní atomické tasky | ✅ |

### Kvalita a testování
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 15 | `15-Test-Scaffold.md` | Struktura testů, testování Core | ✅ |
| 16 | `16-Risks-and-Rollback.md` | Rizika, rollback strategie | ✅ |

### Agents a governance
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 17 | `17-Skills-and-Agents.md` | Definice skills a agentů | ✅ |
| 18 | `18-Ready-to-Run-Prompts.md` | Prompty pro malý model | ✅ |
| 19 | `19-Error-Handling.md` | Error handling, exception politika | ✅ |
| 20 | `20-Security.md` | Bezpečnostní model | ✅ |
| 21 | `21-Telemetry.md` | Telemetrie a observabilita | ✅ |
| 22 | `22-CI-CD.md` | CI/CD pipeline | ✅ |
| 23 | `23-Governance.md` | Decision log, ADRs, dokumentační standardy | ✅ |
| 24 | `24-Markdown-First-Workflow.md` | Pravidla pro markdown-first vývoj | ✅ |

### DI a scaffold
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 25 | `25-DI-and-Composition-Root.md` | DI registrace, lifetime, Composition Root | ✅ |
| 26 | `26-Scaffold-Projects-and-Folders.md` | Scaffold projektů a složek | ✅ |

### ForgeBlock knihovny
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 27 | `27-ForgeBlock-External-Libraries.md` | Katalog externích C#/.NET knihoven pro ForgeBlock integraci | ✅ |

### Konceptuální přehled
| # | Soubor | Obsah | Stav |
|---|--------|-------|------|
| 30 | `30-Architecture-Summary.md` | Konceptuální přehled platformy, editační matice, principy, data flow | ✅ *(2026-07-08)* |

---

## Navigace podle vrstev

```
┌─────────────────────────────────────────────┐
│           12-Host-Surfaces.md                │  CLI · MCP · WebApi
├─────────────────────────────────────────────┤
│           08-Translator.md                   │  Facade · Projection · WriteBack
│           09-AI-Layer.md                     │  AI inference · Translation
├─────────────────────────────────────────────┤
│           07-BusinessModel.md                │  Document · CommandLog · Events
├─────────────────────────────────────────────┤
│  03-Abstractions  04-Elements                │
│  05-Behaviors     06-Services                │  Core vrstva
├─────────────────────────────────────────────┤
│           11-Infrastructure.md               │  Persistence · FileSystem
├─────────────────────────────────────────────┤
│           10-Generators.md                   │  Code export · Templating
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
