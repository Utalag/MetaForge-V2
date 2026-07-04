# Epic 1 — Governance a Project Scaffold

> **Cíl:** Založit nový .NET projekt s markdown-first governance od prvního dne.
> **Výstup:** Solution soubor, governance markdown soubory, README.md.
> **Závislosti:** Žádné — toto je první epic.

---

## TASK-1.1.1 — Vytvoření solution souboru MetaForge.slnx

**Vstup:** Prázdný pracovní adresář (kořen repa).
**Výstup:** Soubor `MetaForge.slnx` — prázdný .NET solution v novém slnx formátu.
**Soubory:** `MetaForge.slnx`

**Kód — vytvoř přesně tento obsah:**

```xml
<Solution>
  <Folder Name="/Src/">
  </Folder>
  <Folder Name="/Tests/">
  </Folder>
</Solution>
```

**Ověření:**
- Spusť v terminálu: `dotnet build MetaForge.slnx`
- Výstup musí být: "Build succeeded." s 0 projekty.
- Pokud selže: zkontroluj, zda je název souboru přesně `MetaForge.slnx` a zda je v kořenu repa.

**Riziko:** Nízké — čistě formátovací záležitost.
**Rollback:** Smaž soubor `MetaForge.slnx`.

---

## TASK-1.1.2 — Vytvoření PROPOSALS.md

**Vstup:** Kořen repa existuje.
**Výstup:** Soubor `PROPOSALS.md` — master checklist aktivních návrhů.
**Soubory:** `PROPOSALS.md`

**Kód — vytvoř přesně tento obsah:**

```markdown
# PROPOSALS — Master Checklist

> Aktivní návrhy a jejich stav.
> Každý návrh musí mít odkaz na detailní markdown v `Docs/Plans/`.

## Aktivní návrhy

| ID | Název | Stav | Odkaz | Poznámka |
|----|-------|------|-------|----------|
| —  | —     | —    | —     | —        |

## Dokončené návrhy

| ID | Název | Datum dokončení | Odkaz |
|----|-------|-----------------|-------|
| —  | —     | —               | —     |

## Zamítnuté návrhy

| ID | Název | Důvod zamítnutí | Datum |
|----|-------|-----------------|-------|
| —  | —     | —               | —     |

---

## Legenda stavů

- 🟡 Draft — návrh se píše
- 🟢 Schváleno — připraveno k implementaci
- 🔵 V implementaci — právě se implementuje
- ✅ Dokončeno — implementováno a otestováno
- ❌ Zamítnuto — nebude se implementovat
```

**Ověření:** Soubor existuje v kořenu repa a obsahuje všechny sekce (Aktivní, Dokončené, Zamítnuté, Legenda).
**Riziko:** Nízké.
**Rollback:** Smaž soubor `PROPOSALS.md`.

---

## TASK-1.1.3 — Vytvoření Progress.md

**Vstup:** Kořen repa existuje.
**Výstup:** Soubor `Progress.md` — chronologický log realizovaných změn.
**Soubory:** `Progress.md`

**Kód — vytvoř přesně tento obsah:**

```markdown
# Progress — Chronologický log změn

> Každá dokončená změna se zapisuje ve formátu:
> `[YYYY-MM-DD] {Epic}/{Slice} — {Popis změny} ({Autor})`

## Log

| Datum | Epic/Slice | Popis | Autor |
|-------|-----------|-------|-------|
| —     | —         | —     | —     |

---

## Checkpointy

| Datum | Epic | Tag |
|-------|------|-----|
| —     | —    | —   |
```

**Ověření:** Soubor existuje, obsahuje tabulku Log a Checkpointy.
**Riziko:** Nízké.
**Rollback:** Smaž soubor `Progress.md`.

---

## TASK-1.1.4 — Vytvoření Memories.md

**Vstup:** Kořen repa existuje.
**Výstup:** Soubor `Memories.md` — provozní knowledge file.
**Soubory:** `Memories.md`

**Kód — vytvoř přesně tento obsah:**

```markdown
# Memories — Provozní knowledge file

> Opakované chyby, guardraily, lessons learned.
> Každý záznam má datum, kategorii a popis.

## Guardraily (neporušitelné)

| # | Guardrail | Datum přidání | Důvod |
|---|-----------|---------------|-------|
| 1 | BusinessAuthoringDocument je source of truth | 2026-07-04 | Architektonický invariant |
| 2 | CommandLog je append-only | 2026-07-04 | Event sourcing |
| 3 | Host surfaces jsou tenké | 2026-07-04 | Separace vrstev |
| 4 | Core je čisté, bez vyšších závislostí | 2026-07-04 | Stabilita jádra |
| 5 | AI je volitelná s graceful fallback | 2026-07-04 | Robustnost |

## Lessons Learned

| Datum | Kategorie | Popis | Důsledek |
|-------|-----------|-------|----------|
| —     | —         | —     | —        |

## Opakované chyby

| Chyba | Kolikrát | Prevence |
|-------|----------|----------|
| —     | —        | —        |
```

**Ověření:** Soubor existuje, obsahuje sekce Guardraily, Lessons Learned, Opakované chyby.
**Riziko:** Nízké.
**Rollback:** Smaž soubor `Memories.md`.

---

## TASK-1.1.5 — Vytvoření PROPOSALS_NEXT.md

**Vstup:** Kořen repa existuje.
**Výstup:** Soubor `PROPOSALS_NEXT.md` — zásobník kandidátních návrhů.
**Soubory:** `PROPOSALS_NEXT.md`

**Kód — vytvoř přesně tento obsah:**

```markdown
# PROPOSALS_NEXT — Zásobník kandidátních návrhů

> Návrhy, které jsou identifikované, ale zatím neschválené k implementaci.
> Nikdy neimplementovat přímo z tohoto souboru — vždy přesunout do PROPOSALS.md.

## Kandidátní návrhy

| ID | Název | Priorita | Odhad | Poznámka |
|----|-------|----------|-------|----------|
| —  | —     | —        | —     | —        |

## Odložené návrhy

| ID | Název | Důvod odložení | Datum |
|----|-------|----------------|-------|
| —  | —     | —              | —     |

---

## Legenda priorit

- 🔴 Kritická — musí se implementovat co nejdříve
- 🟡 Vysoká — důležité pro další vývoj
- 🟢 Nízká — nice to have
- ⚪ Odloženo — zatím se neimplementuje
```

**Ověření:** Soubor existuje, obsahuje sekce Kandidátní návrhy a Odložené návrhy.
**Riziko:** Nízké.
**Rollback:** Smaž soubor `PROPOSALS_NEXT.md`.

---

## TASK-1.1.6 — Vytvoření README.md

**Vstup:** Kořen repa existuje.
**Výstup:** Soubor `README.md` — popis projektu, quick start.
**Soubory:** `README.md`

**Kód — vytvoř přesně tento obsah:**

```markdown
# MetaForge V2

> C#-first platforma pro modelování, generování a správu business aplikací.
> Architektura: Event Sourcing + CommandLog + deterministický překlad do C#.

## Koncept

MetaForge je nástroj pro:
1. **Modelování** business entit, atributů a chování v strukturovaném dokumentu (BusinessAuthoringDocument).
2. **Překlad** business modelu do C# typového modelu přes Translator vrstvu.
3. **Generování** C# kódu z typového modelu přes CSharpGenerator.
4. **Správu** změn přes append-only CommandLog a replay mechanismsus.

## Architektura

```
Host Surface (CLI, MCP, WebApi)
    ↓
Facade (BusinessAuthoringHostFacade)
    ↓
BusinessModel (BusinessAuthoringDocument + CommandLog)
    ↓
Translator (DefaultBusinessTranslator)
    ↓
Core (typový model, katalog, ForgeBlock metadata)
    ↓
Generators (CSharpGenerator)
```

## Požadavky

- .NET SDK 9.0
- Visual Studio Code nebo JetBrains Rider

## Rychlý start

```bash
# Build všech projektů
dotnet build MetaForge.slnx

# Spuštění testů
dotnet test MetaForge.slnx

# CLI — přidání entity
dotnet run --project Src/MetaForge.Cli -- add-entity "Customer"

# CLI — export do C#
dotnet run --project Src/MetaForge.Cli -- export
```

## Governance

- `PROPOSALS.md` — Master checklist návrhů
- `Progress.md` — Chronologický log změn
- `Memories.md` — Provozní knowledge file
- `Docs/Architecture/` — Architektonická dokumentace
- `Docs/Plans/` — Detailní implementační plány

## Licence

Proprietární — viz `29-Monetization.md`.
```

**Ověření:** Soubor existuje, obsahuje sekce Koncept, Architektura, Požadavky, Rychlý start, Governance.
**Riziko:** Nízké.
**Rollback:** Smaž soubor `README.md`.

---

## TASK-1.2.1 — Vytvoření složky Docs a workflow souboru

**Vstup:** TASK-1.1.1 až TASK-1.1.6 hotové.
**Výstup:** Složka `Docs/` s podsložkami a soubor `Docs/workflow-markdown-first.md`.
**Soubory:** `Docs/workflow-markdown-first.md`

**Krok 1 — Vytvoř složky:**
```bash
mkdir -p Docs/Architecture
mkdir -p Docs/Plans
```

**Krok 2 — Vytvoř `Docs/workflow-markdown-first.md`:**

```markdown
# Markdown-First Workflow

> Pravidla pro markdown-first vývoj v MetaForge projektu.

## Princip

Veškeré návrhy, plány a rozhodnutí jsou dokumentovány v markdown souborech.
Žádné implementační rozhodnutí nesmí existovat pouze v chat historii nebo v hlavě vývojáře.

## Workflow

1. **Návrh** — Napiš detailní návrh do `Docs/Plans/{feature-name}.md`.
2. **Schválení** — Přidej návrh do `PROPOSALS.md` jako 🟡 Draft.
3. **Změna stavu** — Po schválení změň stav na 🟢 Schváleno.
4. **Implementace** — Změň stav na 🔵 V implementaci. Implementuj.
5. **Dokončení** — Změň stav na ✅ Dokončeno. Zapiš do `Progress.md`.
6. **Reflexe** — Zapiš lessons learned do `Memories.md`.

## Pravidla

1. Každý návrh MUSÍ mít detailní markdown v `Docs/Plans/`.
2. `PROPOSALS.md` MUSÍ obsahovat odkaz na každý aktivní návrh.
3. Po dokončení implementace MUSÍ být aktualizován `Progress.md`.
4. Opakované chyby a guardraily MUSÍ být zapsány do `Memories.md`.
5. `PROPOSALS_NEXT.md` slouží jako zásobník — nikdy se neimplementuje přímo z něj.
6. Žádné implementační rozhodnutí nesmí žít pouze v chat historii.

## Anti-patterny

- ❌ Implementace bez schváleného návrhu v PROPOSALS.md
- ❌ Plán pouze v hlavě nebo v chatu
- ❌ Progress se nezapisuje
- ❌ Memories.md se neaktualizuje po chybě
- ❌ PROPOSALS.md je zastaralý

## Kontrolní otázky (před každým commitem)

- Je návrh v PROPOSALS.md?
- Existuje detail v Docs/Plans/?
- Je Progress.md aktuální?
- Jsou nové poznatky v Memories.md?
```

**Ověření:**
- Složka `Docs/Architecture/` existuje.
- Složka `Docs/Plans/` existuje.
- Soubor `Docs/workflow-markdown-first.md` existuje a obsahuje všechny sekce.
**Riziko:** Nízké.
**Rollback:** Smaž složku `Docs/` (po kontrole, že neobsahuje jiné soubory).

---

## Souhrn Epic 1 — Co musí existovat po dokončení

```
MetaForge/
├── MetaForge.slnx          ✅ Prázdný solution
├── PROPOSALS.md            ✅ Master checklist
├── PROPOSALS_NEXT.md       ✅ Zásobník návrhů
├── Progress.md             ✅ Chronologický log
├── Memories.md             ✅ Provozní knowledge
├── README.md               ✅ Popis projektu
└── Docs/
    ├── Architecture/       ✅ Prázdná složka
    ├── Plans/              ✅ Prázdná složka
    └── workflow-markdown-first.md ✅ Workflow instrukce
```

**Checkpoint po dokončení:** `git tag checkpoint/epic-1-done`
