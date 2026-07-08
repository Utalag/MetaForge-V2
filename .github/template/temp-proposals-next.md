# PROPOSALS_NEXT

Status: Kandidátní návrhy a nápady mimo aktivní implementační pořadí
Účel: Zachytit relevantní směry bez přetížení PROPOSALS.md
Pravidlo: Každá položka má být krátká; pokud návrh dozraje, dostane vlastní PROP-xxx plán a přesune se do PROPOSALS.md

## Jak číst tento soubor

- Patří sem nápady, kandidátní směry a follow-up návrhy.
- Nepatří sem detailní rozpracování přes více sekcí.
- Pokud návrh získá prioritu, založí se detail v `Docs/Plans/PROP-xxx-*.md` podle šablony `.github/template/temp-prop.md` a položka se přesune do `PROPOSALS.md`.

## Kandidáti

- Název kandidáta  
  Oblast: Core / Workflow / AI / Docs/ ...  
  Důvod: ...  
  Signál hodnoty: ...  
  Hlavní riziko: ...
  Vazby: ...
  Otevřené otázky: ...

## Kandidáti čekající na rozhodnutí

- Název kandidáta — čeká na OQ-0xx....
- Název kandidáta — čeká na stabilizaci PROP-0yy

## Issues — Známé problémy k opravě

> Problémy zjištěné při Code Review po implementaci. Každý issue má vlastní detailní soubor v `Docs/Issues/ISS-xxx_nazev.md`.
> Při opravě přesunout do `PROPOSALS.md` jako nový PROP nebo task.

| # | Datum | PROP | Soubor | Závažnost | Popis | Doporučené řešení | Issue soubor |
|---|-------|------|--------|-----------|-------|-------------------|--------------|
| 1 | YYYY-MM-DD | PROP-xxx | `cesta/k/souboru.cs` | ⚠️ Střední | Stručný popis problému. | Stručný návrh řešení. | [`ISS-xxx`](Docs/Issues/ISS-xxx_PROP-xxx_nazev.md) |

## Přesunuto do PROPOSALS

(vzor)
- PROP-*** Core Statement System — přesunuto YYYY-MM-DD

