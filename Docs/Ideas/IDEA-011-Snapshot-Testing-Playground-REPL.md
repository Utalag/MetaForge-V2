# IDEA-011 Structured Projection + JSON Snapshot (původně Snapshot Testing + Playground/REPL)

Stav: ✅ Rozpracováno → PROP-056
Oblast: Translator, BusinessModel, Host Surfaces
Zdroj: Koumák — Perplexity konverzace e0609fe1 (návrhy 8,9)
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-14

## 1. Původní koncept (Perplexity)

Perplexity navrhlo 2 DX koncepty:
- **Snapshot testing integrace** — každý TypeModel serializovat do deterministického JSON/YAML snapshotu a diffovat při regresi.
- **Playground / REPL target** — evaluovat model jako C# script přes Roslyn Scripting.

## 2. Vývoj diskuse (2026-07-14)

Během analýzy se ukázalo:

- `ToSnapshot()` na **Core elementech** nedává smysl — Core je bez AI, snapshot testy už pokryté PROP-032, fingerprint pro dirty-tracking existuje (PROP-039).
- Mnohem větší hodnota je **sjednotit duplicitní projekce** (`ProjectionView` + `ExpertProjectionView`) do jednoho unifikovaného `DocumentProjection` s řiditelným `ProjectionFilter`.
- Strukturovaný JSON výstup z projekce má reálné use case: **AI enrichment kontext**, **CLI inspect**, **custom views**.

## 3. Výsledný směr

- **Místo `TypeModel.ToSnapshot()`** → unifikovaná projekce `DocumentProjection` v Translator vrstvě
- **Místo snapshot testů modelu** → strukturovaný JSON jako AI enrichment input
- **Playground/REPL** → odloženo, není teď priorita
- YAML není potřeba — JSON je kanonický formát

## 4. Řešení

Viz **PROP-056**:
- Sjednocení `ProjectionView` a `ExpertProjectionView` do `DocumentProjection`
- `ProjectionFilter` pro řiditelnou úroveň detailu (basic, expert, ai-enrichment, custom)
- `ToJson(filter)` pro strukturovaný JSON výstup
- Custom views definované uživatelem (`.metaforge/views.json`)
- Pročištění codebase po refaktoringu

## 5. Návaznost na nové návrhy (2026-07-16)

PROP-056 (Projection Unification + JSON Snapshot) má **volnou vazbu** na nově navržený PROP-057 (ElementContract):
- `ElementContract` může být serializován přes `DocumentProjection.ToJson()` pro AI enrichment kontext
- Oba koncepty sdílejí myšlenku "strukturovaný výstup z Core pro další zpracování"
- PROP-056 zůstává samostatný — PROP-057 ho **neblokuje ani nenahrazuje**

Viz [PROP-056 detail](../Plans/PROP-056-Projection-Unification-JsonSnapshot.md) a [PROP-057 detail](../Plans/PROP-057-ElementContract-VerificationModel.md).
