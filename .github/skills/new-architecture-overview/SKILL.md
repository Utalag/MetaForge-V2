---
name: new-architecture-overview
description: "Pouzij pri: orientaci v New_Architecture dokumentaci, vyberu spravneho skillu pro dany ukol, pochopeni struktury a navaznosti dokumentu v New_Architecture/."
---

# new-architecture-overview

Vstupní bod do celé `New_Architecture/` dokumentace. Poskytuje mapu skillů, rozhodovací tabulku a navigaci mezi dokumenty.

## Kdy použít

- Při prvním kontaktu s New_Architecture
- Když si nejsi jistý, který skill je pro úkol relevantní
- Když potřebuješ pochopit návaznosti mezi vrstvami

## Index dokumentů New_Architecture

| Soubor | Obsah | Stav |
|--------|-------|------|
| `00-Index.md` | Index a navigace | ✅ |
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
| `28-Skills-and-Hooks.md` | Definice skillů a hooků | ✅ |

## Rozhodovací tabulka — který skill použít

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

## Návaznosti mezi dokumenty

```
03-06 Core ← 07 BusinessModel ← 08 Translator ← 09 AI Layer
                                        ↓
                             10 Generators   11 Infrastructure
                                        ↓
                                  12 Host Surfaces
```

## Výstupní checklist

- [ ] Vím, který skill pro můj úkol použít
- [ ] Rozumím návaznostem mezi vrstvami
- [ ] Identifikoval jsem správný dokument v New_Architecture/
