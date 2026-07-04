---
name: New_Architecture — Hlavní instrukce
description: "Hlavní instrukce pro vsechny agenty pracujici na New_Architecture MetaForge."
applyTo: "Src/MetaForge.*"
---

# New Architecture — Hlavní instrukce

Centrální instrukční soubor pro všechny agenty pracující na novém projektu MetaForge.

## Zdroj pravdy

| Zdroj | Role |
|-------|------|
| `New_Architecture/` | Autoritativní dokumentace cílového stavu |
| `Src/` kód | Aktuální implementace — v případě konfliktu s dokumentací platí `New_Architecture/` |
| `Docs/Architecture/` | Archivní a referenční dokumentace |
| `Docs/Plans/` | Detailní návrhy jednotlivých změn |
| `AgentPlans/` | Podrobné implementační plány pro malý model |

## Architektonické guardraily (neporušitelné)

| # | Guardrail | Popis |
|---|-----------|-------|
| 1 | **C#-first** | Core je C#-first. `DataType` enum obsahuje 32 C# typů. |
| 2 | **AppRoot → ProjectElement → RootElement** | Hierarchie vstupního bodu dokumentu |
| 3 | **CommandLog append-only** | Historie změn se nikdy nemaže ani nepřepisuje |
| 4 | **Facade je jediný entry point** | Host surfaces (CLI, MCP, WebApi) volají pouze `BusinessAuthoringHostFacade` |
| 5 | **AI je volitelná** | Graceful fallback vždy — systém musí fungovat bez AI |
| 6 | **Zero-Fault** | Invalidní model se neexportuje jako vykonatelný artefakt |
| 7 | **Tenké host surfaces** | Žádná business logika v CLI/MCP/WebApi |
| 8 | **Core nesmí záviset na vyšších vrstvách** | Žádná reference na Translator, BusinessModel atd. |
| 9 | **BusinessAuthoringDocument je source of truth** | Veškerý stav odvoditelný z dokumentu |
| 10 | **Replay je autoritativní rekonstrukce** | Stav rekonstruován přehráním commandů |

## Governance workflow

| Krok | Akce |
|------|------|
| 1 | Návrh funkcí zapsat do `PROPOSALS.md` |
| 2 | Detailní návrh do `Docs/Plans/plan-XX.md` |
| 3 | Bez schválení v PROPOSALS.md **neimplementovat** |
| 4 | Před změnou zkontrolovat `Memories.md` |
| 5 | Po dokončení zapsat do `Progress.md` |
| 6 | Nové poznatky a guardraily do `Memories.md` |
| 7 | Commit zprávy **vždy v češtině** |

## Skill navigace

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

## Pravidla pro kód

- **Komentáře česky** — veškerý dokumentační text v kódu (XML docs `///`) je česky
- **Target framework:** net9.0
- **Nullable:** enable
- **ImplicitUsings:** enable
- **Kód bez varování** — build musí projít bez warningů
- **Jeden task = jeden commit** — atomické, rollback-friendly

## Povinná kontrola před dokončením

- [ ] Je respektována C#-first architektura?
- [ ] Je Facade jediný entry point?
- [ ] Je CommandLog append-only?
- [ ] Je AI volitelná s graceful fallback?
- [ ] Jsou host surfaces tenké?
- [ ] Je PROPOSALS.md aktuální?
- [ ] Je Progress.md zapsán?
- [ ] Jsou nové poznatky v Memories.md?
- [ ] Build prochází bez chyb a varování?
- [ ] Testy prochází?
