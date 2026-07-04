# AgentPlans — Přehled implementačních plánů

> Tyto plány jsou navrženy jako podrobné podklady pro malý lokální model (Gemma 4 12B).
> Každý plán je self-contained — model nepotřebuje číst ostatní soubory.

---

## Jak používat tyto plány

1. **Začni vždy od `00-Overview.md`** — obsahuje DAG závislostí a pořadí implementace.
2. **Každý Epic plán je nezávislý** — obsahuje vše potřebné pro implementaci dané oblasti.
3. **Postupuj podle pořadí** — Epic 1 → Epic 2 → Epic 3 → Epic 4 → Epic 5/6/7/8 paralelně → Epic 9 průběžně.
4. **Každý task v plánu** = jeden commit. Rollback = `git revert {commit}`.

---

## DAG závislostí mezi Epicy

```
┌──────────────────────────────────────────────────────────────────┐
│                      Epic 1 — Governance                        │
│                    (Solution, governance soubory)                │
└──────────────────────────┬───────────────────────────────────────┘
                           │
           ┌───────────────┼───────────────┐
           ▼               ▼               ▼
   ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
   │  Epic 2      │ │  Epic 3      │ │  Epic 9      │
   │  Core vrstva │ │  BusinessModel│ │  Testy       │
   └──────┬───────┘ └──────┬───────┘ │  (průběžně)  │
          │                │         └──────────────┘
          │                │
   ┌──────┼──────┐    ┌────┼────────┐
   ▼      ▼      ▼    ▼    ▼        ▼
┌─────┐ ┌─────┐ ┌─────┐ ┌──────────┐
│Epic │ │Epic │ │Epic │ │  Epic 4  │
│  7  │ │  8  │ │  9  │ │Translator│
│Gen. │ │Forge│ │Testy│ └────┬─────┘
└─────┘ │Block│ └─────┘      │
        └─────┘         ┌────┼────┐
                        ▼    ▼    ▼
                   ┌─────┐ ┌─────┐ ┌─────┐
                   │Epic │ │Epic │ │Epic │
                   │  5  │ │  6  │ │  9  │
                   │Host │ │ AI  │ │Testy│
                   └─────┘ └─────┘ └─────┘
```

---

## Pořadí implementace

| Pořadí | Epic | Soubor plánu | Co vznikne |
|--------|------|-------------|------------|
| 1 | Epic 1 | `Epic-01-Governance.md` | Solution, PROPOSALS.md, Progress.md, Memories.md |
| 2 | Epic 2 | `Epic-02-Core.md` | MetaForge.Core projekt — typový model, elementy, katalog |
| 3 | Epic 3 | `Epic-03-BusinessModel.md` | MetaForge.BusinessModel — dokument, CommandLog, replay |
| 4 | Epic 4 | `Epic-04-Translator.md` | MetaForge.Translator — facade, projekce, překlad |
| 5 | Epic 5 | `Epic-05-Host-Surfaces.md` | CLI + MCP host surfaces |
| 6 | Epic 6 | `Epic-06-AI.md` | MetaForge.Ai — AI integrace (volitelná) |
| 7 | Epic 7 | `Epic-07-Generators.md` | MetaForge.Generators — CSharpGenerator |
| 8 | Epic 8 | `Epic-08-ForgeBlocks.md` | ForgeBlock balíky (Math, String, Validation) |
| 9 | Epic 9 | `Epic-09-Tests.md` | Testovací infrastruktura a testy |

> Epic 5, 6, 7, 8 lze implementovat paralelně po dokončení Epic 4.
> Epic 9 (Testy) běží průběžně s každým Epicem.

---

## Formát tasků v plánech

Každý task v plánu má tuto strukturu:

```
### TASK-{epic}.{slice}.{číslo} — Název tasku

**Vstup:** Co musí existovat před spuštěním tasku
**Výstup:** Co task vytvoří nebo změní
**Soubory:** Seznam dotčených souborů (max 3)
**Kód:** Přesná specifikace kódu k vytvoření
**Ověření:** Jak poznat, že je task hotový
**Riziko:** Co se může pokazit
**Rollback:** Jak vrátit změnu
```

---

## Pravidla pro malý model

1. **Jeden task = jeden prompt.** Nikdy nekombinuj více tasků do jednoho promptu.
2. **Implementuj přesně podle specifikace.** Nepřidávej nic navíc.
3. **Komentáře v kódu piš česky.** Dokumentační komentáře (///) česky.
4. **Po každém tasku ověř build.** `dotnet build` musí projít.
5. **Commitni každý task zvlášť.** Commit message: `TASK-X.Y.Z — název tasku`.
6. **Pokud build selže:** Oprav chybu, neignoruj ji. Pokud nevíš jak, zeptej se.

---

## Technický stack

| Technologie | Verze | Použití |
|-------------|-------|---------|
| .NET SDK | 9.0 | Všechny projekty |
| C# | 13 | Implementační jazyk |
| xUnit | latest | Testovací framework |
| FluentAssertions | latest | Assertion knihovna |
| Microsoft.Extensions.DI | 9.0 | DI container |
| Microsoft.Extensions.Logging | 9.0 | Logování |
| Scriban | latest | Template engine (Generators) |

---

## Architektonické guardraily (neporušitelné)

1. **BusinessAuthoringDocument je source of truth** — veškerý stav je odvoditelný z tohoto dokumentu.
2. **CommandLog je append-only** — historie se nikdy nemaže ani nepřepisuje.
3. **Replay je autoritativní rekonstrukce stavu** — stav se rekonstruuje přehráním commandů.
4. **Host surfaces jsou tenké** — CLI, MCP, WebApi nesmí obsahovat business logiku.
5. **Core je čisté a stabilní** — žádné závislosti na vyšších vrstvách.
6. **AI je volitelná s graceful fallback** — systém musí fungovat bez AI.
7. **C# je jediný podporovaný výstupní jazyk.**
8. **Komentáře česky** — veškerý dokumentační text v kódu je česky.

---

## Cílová struktura repozitáře

```
MetaForge/
├── .github/agents/
├── Docs/Architecture/
├── Docs/Plans/
├── Src/
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
│   └── MetaForge.Generators.Tests/
├── PROPOSALS.md
├── PROPOSALS_NEXT.md
├── Progress.md
├── Memories.md
├── README.md
└── MetaForge.slnx
```
