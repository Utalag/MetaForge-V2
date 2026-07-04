# Governance soubory pro nový projekt

> Návrh governance souborů, které nový projekt dostane od prvního dne.

---

## PROPOSALS.md

### Účel
Master checklist aktivních návrhů. Bez schválení návrhu neimplementovat.

### Kdo aktualizuje
- Governance Clerk (automaticky při změně stavu návrhu)
- Orchestrátor (při přidání nového návrhu)
- Implementátor (při dokončení — checkbox na ✅)

### Kdy se aktualizuje
- Při vytvoření nového návrhu
- Při schválení nebo zamítnutí návrhu
- Při dokončení implementace
- Při změně priority nebo pořadí

### Co patří
- Stav každého návrhu (📝 Návrh / 🚧 Aktivní / ✅ Hotovo)
- Odkaz na detailní markdown
- Stručná poznámka

### Co nepatří
- Detailní obsah návrhu (žije v separátním markdown)
- Implementační kód
- Chat historie

### Skeleton

```markdown
# MetaForge — PROPOSALS

> **Master checklist aktivních návrhů.** Detail každého plánu žije v `Docs/Plans/`.
> Bez schválení návrhu neimplementovat.

---

## Aktuální implementační pořadí

| Stav | Plán | Poznámka |
|------|------|----------|
| 📝 Návrh | [Plán 1 — Core typový model](Docs/Plans/plan-01-core-typovy-model.md) | Základní abstrakce |
| 📝 Návrh | [Plán 2 — BusinessModel](Docs/Plans/plan-02-business-model.md) | Source of truth |

---

## Praktické pravidlo řazení

- Core před BusinessModel.
- BusinessModel před Translator.
- Translator před Host Surfaces.
- Testy paralelně s každou vrstvou.
```

---

## PROPOSALS_NEXT.md

### Účel
Zásobník kandidátních nebo odložených návrhů. Nápady, které zatím nejsou schváleny.

### Kdo aktualizuje
- Kdokoliv může přidat nápad
- Orchestrátor rozhoduje o přesunu do PROPOSALS.md

### Kdy se aktualizuje
- Při novém nápadu
- Při přesunu do PROPOSALS.md (odstranění z NEXT)
- Při zamítnutí nápadu (poznámka proč)

### Co patří
- Nápady na nové features
- Kandidátní ForgeBlocky
- Refactoring nápady
- Technický dluh

### Co nepatří
- Schválené a aktivní plány (ty patří do PROPOSALS.md)
- Dokončené plány

### Skeleton

```markdown
# MetaForge — PROPOSALS NEXT (zásobník)

> Kandidátní návrhy. Dosud neschválené pro implementaci.
> Přesun do PROPOSALS.md vyžaduje review a schválení.

---

## Kandidáti

| Nápad | Popis | Priorita | Poznámka |
|-------|-------|----------|----------|
| WebApi host | REST API host surface | P3 | Až po CLI+MCP stabilizaci |
| TUI workspace | Terminal UI pro formulářový authoring | P4 | Nice-to-have |

---

## Zamítnuté

| Nápad | Důvod zamítnutí | Datum |
|-------|----------------|-------|
```

---

## Progress.md

### Účel
Chronologický log realizovaných implementačních kroků. Nejnovější záznamy nahoře.

### Kdo aktualizuje
- Governance Clerk po dokončení implementace
- Implementátor může zapsat přímo

### Kdy se aktualizuje
- Po dokončení každé implementace nebo refaktoringu
- Po dokončení každého slice nebo tasku

### Co patří
- Datum
- Co bylo implementováno
- Dotčené soubory/projekty
- Validační stav (build, testy)

### Co nepatří
- Plánování (to je v PROPOSALS.md)
- Nedokončené práce
- Spekulace

### Skeleton

```markdown
# MetaForge — Průběh implementace

> Chronologický záznam dokončených implementačních kroků. **Nejnovější záznamy jsou nahoře.**
> Každá dokončená implementace se přidává na začátek.

---

## Záznam implementace

### 2026-07-XX — Založení projektu a governance scaffold

- Vytvořen solution soubor
- Governance soubory: PROPOSALS.md, Progress.md, Memories.md, PROPOSALS_NEXT.md
- Markdown-first workflow instrukce

**Validace:** N/A (governance only)
```

---

## Memories.md

### Účel
Aktivní provozní knowledge file. Opakované chyby, guardraily, workflow lessons learned.

### Kdo aktualizuje
- Kdokoliv kdo narazí na opakující se problém
- Governance Clerk při review
- Orchestrátor při rozpoznání patternu

### Kdy se aktualizuje
- Při objevení opakující se chyby
- Při odhalení nového guardrail
- Při workflow lesson learned
- Při dependency drift nebo tooling problému

### Co patří
- Konkrétní problém a jeho řešení
- Guardraily specifické pro tento projekt
- Dependency gotchas
- Workflow anti-patterny

### Co nepatří
- Obecné programovací rady
- Duplicity z Architecture dokumentace
- Plánování (to je v PROPOSALS.md)

### Skeleton

```markdown
# Memories

Aktivní provozní poznatky pro agent workflow.

## Šablona

### YYYY-MM-DDTHH:MM:SSZ — Krátký nadpis
- Programový blok: Core / BusinessModel / Translator / Host / ForgeBlock
- Priorita dodržování: Very High / High / Medium / Low
- Problém: Co se stalo.
- Příčina: Proč to nastalo.
- Řešení: Co funguje teď.
- Staré řešení: Předchozí postup, pokud existuje.

## Záznamy

(Zatím prázdné — první záznamy vzniknou s implementací.)
```

---

## Docs/workflow-markdown-first.md

### Účel
Workflow instrukční soubor, který explicitně ukotví markdown-first režim v projektu.

### Skeleton viz `10-Markdown-First-Workflow.md`
