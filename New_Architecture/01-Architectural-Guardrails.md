# Architektonické guardraily

> Tyto principy jsou neporušitelné. Žádná implementační změna je nesmí obejít.

---

## Authoring-kernel invarianty

| # | Invariant | Vysvětlení |
|---|-----------|------------|
| 1 | BusinessAuthoringDocument je source of truth | Veškerý stav systému je odvoditelný z tohoto dokumentu |
| 2 | CommandLog je append-only | Historie změn se nikdy nemaže ani nepřepisuje |
| 3 | Replay je autoritativní rekonstrukce stavu | Stav se rekonstruuje přehráním commandů, ne čtením cache |
| 4 | Write-back vrací enrichment do business vrstvy | Obohacení z Translatoru se zapisuje zpět do BusinessAuthoringDocument |
| 5 | Host surfaces jsou tenké | CLI, MCP, WebApi — žádná business logika v host vrstvě |
| 6 | Core je čisté a stabilní | Žádné závislosti na vyšších vrstvách, žádný framework coupling |
| 7 | AI je volitelná s graceful fallback | Systém musí fungovat bez AI, AI nesmí být single point of failure |
| 8 | Tier 2 AI vrací strukturovaná data | Nikdy volný text pro uživatele |
| 9 | AI pracuje nad synchronizovaným stavem | Ne nad prázdným promptem |
| 10 | ForgeBlocky jsou capability balíky | Ne jen codegen pluginy |
| 11 | Discovery metadata jsou federovaná | Každý ForgeBlock nese vlastní discovery metadata |

---

## C#-first architektura

| Princip | Důsledek |
|---------|----------|
| C# je jediný podporovaný výstupní jazyk | Generators obsahuje pouze `CSharpGenerator` (`CSharpGenerator.LanguageId`) |
| Core je jazykově orientované na C# | Typový model může obsahovat C#-specifika tam, kde to zjednodušuje návrh — striktní jazyková agnosticita se nevyžaduje |
| Nové capability se navrhují C#-first | Mapping na jiné jazyky je future-scope |

---

## Hranice vrstev

```
Host Surface (CLI, MCP, WebApi)
    │ volá pouze
    ▼
Facade (BusinessAuthoringHostFacade)
    │ orchestruje
    ▼
Business Model (BusinessAuthoringDocument + CommandLog)
    │ překládá
    ▼
Translator (DefaultBusinessTranslator + enrichment)
    │ používá
    ▼
Core (typový model, katalog, ForgeBlock metadata)
    │ exportuje přes
    ▼
Generators (CSharpGenerator — jediný cílový jazyk)
```

**Pravidla:**
- Žádná vrstva nesmí záviset na vrstvě nad ní.
- Facade je jediný vstupní bod pro host surfaces.
- Business Model nezná Core přímo — Translator je prostředník.
- Core nezná Generators — Generators závisí na Core.

---

## Vývojové guardraily

> Přejmenováno z "Workflow guardraily" — workflow jako doménový koncept byl odstraněn (PROP-063, 2026-07-18).

| Guardrail | Popis |
|-----------|-------|
| Markdown-first | Návrhy, backlog, governance žijí v markdown souborech |
| PROPOSALS.md je master checklist | Bez schválení návrhu neimplementovat |
| Progress.md je chronologický log | Každá dokončená změna se zapíše |
| Memories.md je provozní knowledge | Opakované chyby, guardraily, lessons learned |
| Komentáře česky | Veškerý dokumentační text v kódu je česky |

---

## Co se nesmí stát

- Přímá mutace BusinessAuthoringDocument bez CommandLog záznamu.
- Host surface obsahující business logiku.
- Core závisející na Translator nebo Business Model.
- AI bez fallbacku.
- Rozsáhlá změna bez rollback plánu.
- Changelog workflow místo markdown-first governance.
- Znovuzavedení explicitního workflow modelu bez schváleného PROP (odstraněno PROP-063).
- Export invalidního modelu jako vykonatelný artefakt.
