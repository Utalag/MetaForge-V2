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
| 6 | Nedokončené části PROP evidovat v PROPOSALS_NEXT.md → Odložené | 2026-07-04 | Po PROP-020 — Fáze 5 zůstala nedokončena, zapsána jako PROP-020-F5 |

## Lessons Learned

| Datum | Kategorie | Popis | Důsledek |
|-------|-----------|-------|----------|
| 2026-07-04 | Dokumentace | Po implementaci PROP je třeba vždy aktualizovat New_Architecture/ dokumenty. Mapping: PROP-028→11-Infrastructure.md, PROP-024→05-Core-Behaviors.md+06-Core-Services.md, PROP-027→09-AI-Layer.md, PROP-020→07-BusinessModel.md+08-Translator.md, plus vždy 00-Index.md | Zapsáno do Progress.md

## Opakované chyby

| Chyba | Kolikrát | Prevence |
|-------|----------|----------|
| —     | —        | —        |
