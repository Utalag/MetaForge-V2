# IDEA-023 ExpertProjection Revival — PROP-018

Stav: Idea
Oblast: Translator, Host
Zdroj: For_Inspiration/Architecture-Define/04-OpenQuestions.md, PROP-018
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

PROP-018 (ExpertProjection a ProjectionOptions) je odložený návrh — v `PROPOSALS_NEXT.md` označen jako "nejsem si jist správnou funkcionalitou pro host surfaces". Původní koncept ale měl ExpertProjection jako klíčovou komponentu pro AI kontext a diagnostiku.

Nápad vychází z PROP-018 a původních OpenQuestions (OQ-003), kde bylo rozhodnuto o `ProjectionOptions` a `ProjectionView`.

## 2. Problém dnes

- PROP-018 je odložen — ExpertProjection existuje v základní formě (PROP-020), ale bez ProjectionOptions.
- `GetBusinessProjection(detail?)` v MCP je implementováno, ale chybí jemnozrnná kontrola (diagnostics, typeResolution, suggestions, relationAnalysis).
- AI kontext je buď "basic" (málo informací) nebo "expert" (moc informací) — chybí střední cesta.
- ProjectionOptions byly rozhodnuty (OQ-003), ale neimplementovány jako první-class objekt.

## 3. Předběžný směr řešení

- Oživit PROP-018: implementovat `ProjectionOptions` jako first-class objekt
- `ProjectionOptions.Basic()`, `.Expert()`, `.Custom(diagnostics, typeResolution, suggestions, relationAnalysis)`
- ExpertProjectionBuilder rozšířit o sekce: Diagnostics, TypeResolution, Suggestions, RelationAnalysis
- MCP tool `GetBusinessProjection(detail?)` parsovat detail do ProjectionOptions
- AI klient může požádat o přesně takový kontext, jaký potřebuje

Dotčené vrstvy: Translator (ProjectionOptions, ExpertProjectionBuilder), Host (MCP tool).

## 4. Signál hodnoty

- AI klienti dostávají přesně takový kontext, jaký potřebují — ne víc, ne míň.
- Jemnozrnná kontrola nad výstupem — performance optimalizace (basic pro rychlé odpovědi).
- Návaznost na OQ-003 — rozhodnutí už padlo, jen chybí implementace.

## 5. Rizika a nejasnosti

- PROP-018 byl odložen z důvodu nejistoty — je potřeba znovu ověřit, zda je aktuální.
- ProjectionOptions částečně existuje v kódu — je potřeba zkontrolovat, co už je hotovo.

## 6. Doporučený další krok

Ověřit aktuálnost PROP-018 — pokud je rozhodnutí OQ-003 stále platné, převést na Candidate. Pokud ne, vytvořit Open Question.

Vazby: PROP-018 (odložený), OQ-003 (rozhodnutí), PROP-020 (ExpertProjection základ)
