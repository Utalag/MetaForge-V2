# IDEA-010 SymbolTable + EmitPhase

Stav: Idea (ReferenceGraph vyčleněn jako PROP-055)
Oblast: Core, Generators
Zdroj: Koumák — Perplexity konverzace e0609fe1 (návrhy 1,2,4)
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-13

## 1. Kontext

Původní Perplexity návrh obsahoval 3 koncepty:
- ~~**ReferenceGraph**~~ → ✅ **PROP-055** (2026-07-13)
- **SymbolTable/ScopeChain** — sdílený registr pojmenovaných symbolů pro validaci Expression a Statement (inspirace Roslyn SemanticModel)
- **EmitPhase** — fázovaný output model (Syntax → Text → File) s hooky před/po každé fázi

## 2. Problém dnes

- Expression a Statement nemají kontext pro validaci — např. `a * a` funguje, i když `a` není deklarované.
- Generátor nemá fáze — vše se renderuje najednou.

## 3. Předběžný směr řešení

- SymbolTable jako `Dictionary<string, SymbolInfo>` per-scope.
- EmitPhase pipeline: `SyntaxPhase → TextPhase → FilePhase` s `IEmitHook`.

## 4. Signál hodnoty

- Validace výrazů v kontextu.
- Rozšiřitelná generátorová pipeline.

## 5. Rizika a nejasnosti

- SymbolTable vyžaduje plnou sémantickou analýzu — komplexní.
- EmitPhase je užitečné, ale až po stabilizaci generátoru.

## 6. Doporučený další krok

- Oba koncepty zůstávají jako Idea — neaktivovat.
- Po dokončení PROP-037 (Roslyn importer) znovu zvážit — SymbolTable se váže na importer.
