# PROP-XXX Název návrhu

Typ výsledku: Candidate Proposal | Follow-up | Open Question Follow-up
Zdroj podnětu: User | AI | Bug | Architektura | Trh
Stav životního cyklu: Candidate | Approved | In Progress | Done | Dropped
Rozhodovací owner:
Poslední revize: YYYY-MM-DD

Priorita: Very High | High | Medium | Low
Oblast: Core / BusinessModel / Translator / Generators / MCP / CLI / AI / Workflow / Docs
Owner:
Datum vytvoření: YYYY-MM-DD
Aktualizováno: YYYY-MM-DD

Navazuje na:
- PROP-xxx ...
- OQ-xxx ...

Blokuje:
- PROP-xxx ...

Související soubory:
- `Src/...`
- `Docs/...`

## 1. Kontext

Popiš, proč návrh vznikl, jaký problém dnes řeší a kde je vidět jeho dopad.

## 2. Problém dnes

- Jak se problém projevuje.
- Co je root cause.
- Proč nestačí malý workaround.

## 3. Cíl

- Jak vypadá cílový stav.
- Co bude po změně čistší, stabilnější nebo lépe rozšiřitelné.
- Jaký konkrétní přínos to má pro platformu.

## 4. Architektonické invarianty

- BusinessAuthoringDocument zůstává source of truth.
- CommandLog zůstává append-only.
- Replay zůstává autoritativní rekonstrukční cesta.
- Core nesmí nést logiku, která patří do vyšší vrstvy.
- Host surface má zůstat tenká.
- AI je volitelná vrstva, ne podmínka základní funkčnosti.

Doplň sem jen ty invarianty, které jsou pro tento návrh skutečně relevantní.

## 5. Scope

### In scope
- ...
- ...
- ...

### Out of scope
- ...
- ...
- ...

## 6. Návrh řešení

### Cílový návrh
Popiš cílové řešení po vrstvách nebo komponentech.

### Rozdělení odpovědností
- Core: ...
- BusinessModel: ...
- Translator: ...
- Generators: ...
- Host / MCP / CLI: ...

### Proč je tento návrh správný
Stručně vysvětli, proč je návrh lepší než rychlý fix nebo workaround.

## 7. Implementační dopad

### Změněné projekty nebo soubory
- `Src/...`
- `Tests/...`
- `Docs/...`

### API a kontrakty
- Jaké public nebo internal kontrakty se mění.
- Co se musí migrovat u volajících.

### Testy
- Jaké testy se musí doplnit nebo upravit.

### Dokumentace
- Jaké docs se musí aktualizovat.

## 8. Implementační fáze

### Fáze 1
- ...
- ...

### Fáze 2
- ...
- ...

### Fáze 3
- ...
- ...

## 9. Otevřené otázky

- OQ-xxx_PROP-aktualni ...
- OQ-yyy_PROP-aktualni ...

Pokud návrh naráží na neuzavřenou architektonickou otázku, neřeš ji skrytě tady; založ nebo odkaž samostatný OQ záznam.

## 10. Rizika a trade-offy

- Riziko regrese: ...
- Riziko nedokončené migrace: ...
- Riziko příliš širokého řezu: ...
- Vědomý kompromis: ...

## 11. Validace

- Build:
- Testy:
- Smoke scénáře:
- Ruční kontrola:
- Jak poznáme, že je návrh opravdu hotový:

## 12. Výsledek po dokončení

Vyplnit až při uzavření návrhu.

- Co bylo skutečně dodáno:
- Co se změnilo oproti původnímu plánu:
- Co bylo odloženo do follow-up:
- Jaké další dokumenty bylo potřeba upravit: