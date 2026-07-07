# PROP-036 Core Specification Layer — Invarianty, Validace, Test Generation

Typ výsledku: Candidate Proposal
Zdroj podnětu: Koumák — Perplexity konverzace 05663298 (5 dotazů o invariantech a test generation)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-07

Priorita: High
Oblast: Core, Tests, BusinessModel, AI
Owner:
Datum vytvoření: 2026-07-07
Aktualizováno: 2026-07-07

Navazuje na:
- Perplexity konverzace: https://www.perplexity.ai/search/05663298-ed0e-4ab2-bfb9-8472bbbe455a
- PROP-035 (C#-First Core) — Specification Layer předpokládá C#-first elementy
- PROP-021 (FsCheck + Verify) — generování testů z invariantů
- PROP-031 (Core Statement System) — statementy jsou cílem invariantů

Blokuje:
- —

Související soubory:
- `Src/MetaForge.Core/Elements/Types/ClassElement.cs`
- `Src/MetaForge.Core/Elements/Members/MethodElement.cs`
- `Src/MetaForge.Core/DataTypes/StrongType.cs`
- `Tests/MetaForge.Core.Tests/`

## 1. Kontext

Elementy v MetaForge Core (např. `MethodElement`) dnes obsahují vlastnosti jako `IsAsync`, `IsAbstract`, `IsStatic`. Některé kombinace jsou nevalidní (např. `IsAsync && IsAbstract`), ale tyto invarianty nejsou nikde deklarované — jsou jen implicitně v kódu validátoru, v hlavě vývojáře, a případně duplicitně v testech.

Myšlenka: povýšit invarianty na **first-class specification artifact** — deklarativní, serializovatelná pravidla, ze kterých lze odvodit:
- runtime validaci,
- generování testů (FsCheck property-based testing),
- AI-assisted návrhy nových pravidel,
- dokumentaci.

## 2. Problém dnes

- **Žádný zdroj pravdy pro invarianty:** `IsAsync && IsAbstract => invalid` je pravidlo, které dnes není nikde formálně zapsané.
- **Duplicitní validace:** Stejné pravidlo se může opakovat v kódu, v testech, v dokumentaci — časem se rozjedou.
- **Chybějící test generation:** Bez deklarativních invariantů nelze automaticky generovat testy, které ověřují "zakázané kombinace".
- **AI nemá strukturovaný vstup:** AI model nemůže navrhovat nová pravidla, protože nevidí formální specifikaci.

## 3. Cíl

- Vytvořit `Core/Specifications/` namespace s těmito komponentami:
  - **InvariantDefinition** — deklarativní pravidlo (kód, popis, scope, AST).
  - **InvariantExpression** — serializovatelný boolean AST (And, Or, Not, Eq, Exists, PropertyRef).
  - **IInvariantEvaluator** — jednotný evaluátor pro Local/Scoped/Relational/Global invarianty.
  - **EvaluationResult** — porušení + cesta + čitelná zpráva.
  - **CompiledInvariant** — prekompilovaná runtime verze pro výkon.
- StrongType zůstává jako value-level constraints (scalar validace).
- InvariantDefinition se stane součástí BusinessModel dokumentu (uživatelské invarianty).
- Z invariantů lze generovat FsCheck testy (validní i nevalidní generátory).

## 4. Architektonické invarianty

- BusinessAuthoringDocument zůstává source of truth.
- Jeden zdroj pravdy pro invarianty — ne duplicitní reprezentace.
- Core nesmí nést logiku, která patří do vyšší vrstvy.
- AI je volitelná vrstva — invarianty fungují i bez AI.
- **Nedělat z toho general-purpose rules engine** — udržet jednoduchý boolean AST.

## 5. Scope

### In scope
- `InvariantDefinition` record s `Code`, `TargetKind`, `Description`, `Severity`, `Scope`, `When`, `Must`.
- `InvariantExpression` boolean AST: `PropertyRef`, `Constant`, `Eq`, `Not`, `And`, `Or`, `Exists`.
- `InvariantScope` enum: `Local`, `Scoped`, `Relational`, `Global`.
- `IInvariantEvaluator` s `EvaluationContext` (model, registry, katalogy).
- `EvaluationResult` s `Violations` (kód, cesta, zpráva, scope).
- `Implies(a, b)` jako authoring helper (normalizuje se do AST).
- Integrace se `StrongType` — value-level constraints.
- Napojení na `MethodElement` a `ClassElement` (první cíle).
- Generování testů z invariantů: valid generator + invalid generator.
- AI guardraily: návrh ano, aktivace ne; provenance metadata.

### Out of scope
- Plnohodnotný rule engine s forward/backward chaining.
- Expression tree serializace (`.NET Expression<T>`).
- Automatická aktivace AI-generovaných invariantů.
- `CompiledInvariant<T>` (až fáze 2).
- Cache výsledků evaluace (až fáze 2).
- FsCheck adapter (až fáze 2).
- Uživatelské invarianty v BusinessAuthoringDocument (až fáze 2).

## 6. Návrh řešení

### Klíčový princip: Jeden zdroj pravdy

```
InvariantDefinition (JSON-serializovatelný)
       │
       ├──→ Runtime validace (IInvariantEvaluator)
       ├──→ Test generation (FsCheck valid/invalid generators)
       ├──→ Dokumentace / chybové hlášky
       └──→ AI guardrail (návrhy nových invariantů)
```

### InvariantExpression — Boolean AST

```csharp
abstract record InvariantExpression;
record PropertyRef(string Path) : InvariantExpression;
record Constant(object? Value) : InvariantExpression;
record Eq(InvariantExpression Left, InvariantExpression Right) : InvariantExpression;
record Not(InvariantExpression Inner) : InvariantExpression;
record And(IReadOnlyList<InvariantExpression> Items) : InvariantExpression;
record Or(IReadOnlyList<InvariantExpression> Items) : InvariantExpression;
record Exists(string Path) : InvariantExpression;
```

JSON reprezentace:
```json
{
  "code": "MF_METHOD_001",
  "targetKind": "MethodElement",
  "description": "Abstract method must not have body",
  "severity": "Error",
  "scope": "Local",
  "when": { "eq": ["$.IsAbstract", true] },
  "must": { "not": { "exists": "$.Body" } }
}
```

### InvariantDefinition

```csharp
public sealed record InvariantDefinition(
    string Code,
    string TargetKind,
    string Description,
    InvariantSeverity Severity,
    InvariantScope Scope,
    InvariantExpression When,
    InvariantExpression Must,
    GeneratorIntent? GeneratorIntent = null
);
```

### Invariant Scopes

| Scope | Popis | Příklad |
|-------|-------|---------|
| `Local` | Jeden element sám o sobě | `IsAsync && IsAbstract => invalid` |
| `Scoped` | Element v rámci rodičovského kontextu | Všechny `PropertyElement` v `ClassElement` mají unikátní `Name` |
| `Relational` | Element + lookup do modelu | `PropertyElement.TypeModel.CustomType` odkazuje na existující `StrongType` |
| `Global` | Pravidlo nad celým dokumentem | Všechny `ClassElement` v dokumentu mají unikátní plně kvalifikovaný název |

### IInvariantEvaluator

```csharp
public interface IInvariantEvaluator
{
    EvaluationResult Evaluate(
        object target,
        InvariantEvaluationContext context,
        IReadOnlyList<InvariantDefinition> invariants);
}
```

### Test Generation z invariantů

```csharp
// Validní generátor — splňuje všechny invarianty
MethodElementSpec.ValidGenerator()

// Nevalidní generátor — porušuje konkrétní invariant
MethodElementSpec.InvalidGenerator(ruleCode: "MF_METHOD_001")

// V testu:
Prop.ForAll(MethodElementSpec.InvalidGenerator("MF_METHOD_001"), method =>
{
    var result = evaluator.Evaluate(method, context, invariants);
    Assert.Contains(result.Violations, v => v.Code == "MF_METHOD_001");
});
```

### AI Guardraily

- AI smí **navrhnout** `InvariantDefinition`, ale **nesmí** sama aktivovat.
- Každý AI-generated invariant prochází: AST validací, typovou kontrolou, impact preview.
- Provenance metadata: kdo navrhl, z jakého promptu, kdy, verze, historie false positives.

## 7. Implementační dopad

### Změněné projekty nebo soubory

- Nové: `Src/MetaForge.Core/Specifications/InvariantDefinition.cs`
- Nové: `Src/MetaForge.Core/Specifications/InvariantExpression.cs`
- Nové: `Src/MetaForge.Core/Specifications/InvariantScope.cs`
- Nové: `Src/MetaForge.Core/Specifications/InvariantSeverity.cs`
- Nové: `Src/MetaForge.Core/Specifications/IInvariantEvaluator.cs`
- Nové: `Src/MetaForge.Core/Specifications/EvaluationResult.cs`
- Nové: `Src/MetaForge.Core/Specifications/CompiledInvariant.cs` (fáze 2)
- Upravené: `Src/MetaForge.Core/DataTypes/StrongType.cs` — `+GeneratorIntent?`
- Upravené: `Src/MetaForge.Core/Elements/Members/MethodElement.cs` — `+Invariants` kolekce
- Upravené: `Src/MetaForge.Core/Elements/Types/ClassElement.cs` — `+Invariants` kolekce
- Nové: `Tests/MetaForge.Core.Tests/Specifications/` — testy invariantů

### API a kontrakty

- Nové public API: `InvariantDefinition`, `InvariantExpression`, `IInvariantEvaluator`.
- Elementy získají `IReadOnlyList<InvariantDefinition> Invariants`.
- `StrongType` získá volitelný `GeneratorIntent?`.

### Testy

- Invariant validační testy: pro každý invariant — validní případ projde, nevalidní je detekován.
- FsCheck testy: `MethodElementSpec.ValidGenerator()`, `MethodElementSpec.InvalidGenerator("MF_METHOD_001")`.
- Snapshot testy: serializace/deserializace InvariantDefinition do/z JSON.

### Dokumentace

- `Docs/Core/08-Specification-Layer.md` — popis vrstvy.
- `New_Architecture/` — aktualizace Core kapitoly.

## 8. Implementační fáze

### Fáze 1: MVP — InvariantDefinition + AST + Evaluator (Core)
- `InvariantExpression` boolean AST.
- `InvariantDefinition` record.
- `InvariantScope` enum.
- `IInvariantEvaluator` + `EvaluationResult`.
- Napojení na `MethodElement` a `ClassElement` (první sada invariantů: MF_METHOD_001-005, MF_CLASS_001-003).
- StrongType + GeneratorIntent.
- **DoD:** Lze definovat invariant, serializovat do JSON, deserializovat, vyhodnotit.

### Fáze 2: Test Generation + AI guardraily
- Test generation z invariantů: `ValidGenerator()` + `InvalidGenerator(code)`.
- FsCheck integrace.
- AI guardraily: provenance metadata, impact preview.
- Uživatelské invarianty v `BusinessAuthoringDocument`.
- `CompiledInvariant<T>` pro výkon.
- Cache kompilovaných invariantů.
- **DoD:** FsCheck test vygeneruje 1000+ testovacích případů z jednoho invariantu.

## 9. Otevřené otázky

- OQ-036-01: Má být `GeneratorIntent` na `StrongType`, nebo odvozen z invariantů? (Perplexity: nechat jako volitelný optimization hint)
- OQ-036-02: Jak granularita AI-generated invariantů — má AI navrhovat jen strukturu, nebo i konkrétní hodnoty?
- OQ-036-03: Má `IInvariantEvaluator` být součástí Core, nebo Infrastructure?

## 10. Rizika a trade-offy

- Riziko rozpadu zdroje pravdy: Pokud vzniknou separátní validační systémy (StrongType, runtime validator, FsCheck, AI prompting) — **největší riziko.**
- Riziko overengineeringu: Boolean AST musí zůstat jednoduchý — žádný Turing-kompletní rule engine.
- Riziko AI-generated chyb: Tiché zavedení špatného invariantu, který blokuje validní modely. Řešení: review + impact preview.
- Trade-off: Deklarativní AST vs kompilované lambda — AST je pomalejší, ale serializovatelný. Řešení: `CompiledInvariant<T>` pro runtime.

## 11. Validace

- Build: Nový namespace `Core/Specifications/` se zkompiluje.
- Testy: Každý invariant má validní a nevalidní test case.
- Smoke scénáře: Definovat `MF_METHOD_001`, serializovat do JSON, deserializovat, vyhodnotit, vygenerovat FsCheck test.
- Ruční kontrola: Ověřit, že `InvariantExpression` AST nepřerůstá do rule engine.

## 12. Výsledek po dokončení

Vyplnit až při uzavření návrhu.
