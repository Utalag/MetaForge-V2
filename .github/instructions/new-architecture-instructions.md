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
| `Docs/Plans/` | Detailní plány jednotlivých změn (PROP-xxx-*) |
| `Docs/Plans/Implementation-Roadmap.md` | Autoritativní pořadí implementace — určuje co a kdy |
| `PROPOSALS.md` | Schválené návrhy v implementaci |
| `PROPOSALS_NEXT.md` | Kandidátní návrhy — neimplementovat bez přesunu do PROPOSALS.md |
| `For_Inspiration/` | Artefakty původní implementace, readonly inspirace |
| `Progress.md` | Průběžný log implementace |
| `Memories.md` | Projektové poznatky, guardraily a konvence |

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
| 11 | **Monetizace přes GeneratorLicense** | Tierový model (Sandbox→Domain→Infrastructure→Full). Kód se negeneruje automaticky zdarma. |
| 12 | **PROP workflow** | Každá změna musí mít schválený PROP v PROPOSALS.md. Pořadí implementace dle `Implementation-Roadmap.md`. |
| 13 | **Nedokončené části PROP** | Po dokončení PROP se všechny nedokončené fáze/položky MUSÍ zapsat do `PROPOSALS_NEXT.md` → Odložené návrhy s prefixem `PROP-XXX-F#`. Nikdy nenechávat nedokončenou práci neevidovanou. |

## Governance workflow

| Krok | Akce |
|------|------|
| 1 | Zkontrolovat `PROPOSALS.md` a `PROPOSALS_NEXT.md` |
| 2 | Nový návrh → vytvořit PROP-XXX plan v `Docs/Plans/`, zapsat do `PROPOSALS_NEXT.md` |
| 3 | Schválený návrh → přesunout do `PROPOSALS.md` |
| 4 | Před implementací zkontrolovat `Implementation-Roadmap.md` pro správné pořadí |
| 5 | Před změnou zkontrolovat `Memories.md` |
| 6 | Po dokončení zapsat do `Progress.md` |
| 7 | Nové poznatky a guardraily do `Memories.md` |
| 8 | Commit zprávy **vždy v češtině**: `PROP-XXX — popis změny` |

## Skill navigace

| Potřebuješ | Použij skill |
|-----------|-------------|
| Core, DataType, TypeModel, elementy, StrongType | `new-architecture-core` |
| BusinessAuthoringDocument, CommandLog, PatchEngine | `new-architecture-business-model` |
| Facade, Projection, WriteBack, AI Translator | `new-architecture-translator` |
| AI integrace, Ollama, fallback, PromptRegistry | `new-architecture-ai` |
| CSharpGenerator, code export, monetizace, Scriban | `new-architecture-generators` |
| Persistence, JSONL, konfigurace, caching | `new-architecture-infrastructure` |
| CLI, MCP, WebApi, REPL | `new-architecture-host-surfaces` |
| ForgeBlock balíky, EF Core, AutoMapper, marketplace | `new-architecture-forgeblocks` |
| DI registrace, Composition Root | `new-architecture-di-composition` |
| Exception handling, logging | `new-architecture-error-handling` |
| Testy, FsCheck, Verify snapshots | `new-architecture-test-scaffold` |
| Scaffold projektů a složek | `new-architecture-scaffold` |
| Schema migrace, health checks, validace | `new-architecture-security-stability` |
| Orientace, který skill použít | `new-architecture-overview` |

## Pravidla pro kód

- **Komentáře česky** — veškerý dokumentační text v kódu (XML docs `///`) je česky
- **Target framework:** net10.0
- **Nullable:** enable
- **ImplicitUsings:** enable
- **Kód bez varování** — build musí projít bez warningů
- **Jeden task = jeden commit** — atomické, rollback-friendly
- **Implementace podle PROP** — každá změna musí mít schválený PROP v `PROPOSALS.md`
- **Respektovat `Implementation-Roadmap.md`** — dodržovat pořadí fází

## Povinná kontrola před dokončením

- [ ] Je respektována C#-first architektura?
- [ ] Je Facade jediný entry point?
- [ ] Je CommandLog append-only?
- [ ] Je AI volitelná s graceful fallback?
- [ ] Jsou host surfaces tenké?
- [ ] Respektuje kód monetizační model (GeneratorLicense / tier)?
- [ ] Je změna kryta schváleným PROP v PROPOSALS.md?
- [ ] Je Progress.md zapsán?
- [ ] Jsou nové poznatky v Memories.md?
- [ ] Build prochází bez chyb a varování?
- [ ] Testy prochází?
