# IDEA-011 Snapshot Testing Integration + Playground/REPL Target

Stav: Idea
Oblast: Core, Tests, Host
Zdroj: Koumák — Perplexity konverzace e0609fe1 (návrhy 8,9)
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

Perplexity navrhlo 2 DX koncepty:
- **Snapshot testing integrace** — každý TypeModel serializovat do deterministického JSON/YAML snapshotu a diffovat při regresi (jako Verify.NET).
- **Playground / REPL target** — evaluovat model jako C# script přes `Microsoft.CodeAnalysis.Scripting` bez plného build cyklu.

## 2. Problém dnes

- Testy porovnávají string výstupy, ne strukturované snapshoty.
- Není rychlý způsob, jak vyzkoušet model bez buildu.

## 3. Předběžný směr řešení

- Snapshot: `TypeModel.ToSnapshot()` → JSON, porovnání s `.verified.json`.
- Playground: CLI příkaz `metaforge repl` nebo `metaforge eval "Class("X")..."`.

## 4. Signál hodnoty

- Snapshot: regresní testování celého modelu.
- Playground: rychlé prototypování a experimentování.

## 5. Rizika a nejasnosti

- Snapshot testing je částečně pokrytý PROP-032.
- Playground vyžaduje Roslyn Scripting — závislost mimo Core.

## 6. Doporučený další krok

- Snapshot: follow-up na PROP-032.
- Playground: follow-up na PROP-026 (Host Surfaces CLI).
