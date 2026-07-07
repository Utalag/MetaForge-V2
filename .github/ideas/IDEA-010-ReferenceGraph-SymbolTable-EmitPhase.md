# IDEA-010 ReferenceGraph + SymbolTable + EmitPhase

Stav: Idea
Oblast: Core, Generators
Zdroj: Koumák — Perplexity konverzace e0609fe1 (návrhy 1,2,4)
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

Perplexity navrhlo 3 koncepty pro sémantickou vrstvu a generátorovou pipeline:
- **ReferenceGraph** — orientovaný graf závislostí mezi ClassElement, InterfaceElement a TypeModel pro detekci cirkulárních referencí.
- **SymbolTable/ScopeChain** — sdílený registry pojmenovaných symbolů pro validaci Expression a Statement (inspirace Roslyn SemanticModel).
- **EmitPhase** — fázovaný output model (Syntax → Text → File) s hooky před/po každé fázi.

## 2. Problém dnes

- Cirkulární reference se detekují až při generování, ne v modelu.
- Expression a Statement nemají kontext pro validaci — např. `a * a` funguje, i když `a` není deklarované.
- Generátor nemá fáze — vše se renderuje najednou.

## 3. Předběžný směr řešení

- ReferenceGraph jako `Dictionary<TypeModel, HashSet<TypeModel>>`.
- SymbolTable jako `Dictionary<string, SymbolInfo>` per-scope.
- EmitPhase pipeline: `SyntaxPhase → TextPhase → FilePhase` s `IEmitHook`.

## 4. Signál hodnoty

- Včasná detekce cirkulárních referencí.
- Validace výrazů v kontextu.
- Rozšiřitelná generátorová pipeline.

## 5. Rizika a nejasnosti

- SymbolTable vyžaduje plnou sémantickou analýzu — komplexní.
- EmitPhase je užitečné, ale až po stabilizaci generátoru.

## 6. Doporučený další krok

- Zatím jen zapisovat, neaktivovat.
- Po dokončení PROP-037 (Roslyn importer) znovu zvážit — SymbolTable se váže na importer.
