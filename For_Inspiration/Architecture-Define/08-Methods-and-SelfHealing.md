# MetaForge — Metody: architektura, AI generování a Self-Healing

Datum: 2026-04-17 (aktualizováno 2026-04-18)
Status: Cesta A implementována, **Cesta B implementována (Plán 8, 2026-04-18)**. Self-healing připraven jako navazující řez.

---

## 1. Jak metoda vzniká v platformě

MetaForge pracuje se dvěma nezávislými cestami jak metoda (Method) vznikne:

### Cesta A — Builder API (přímá, implementována)

```
Vývojář / Examples
└── MethodBuilder.Method("Solve", DataType.Custom)
    ├── .Param("a", DataType.Double)
    ├── .Doc("Solve quadratic equation...")         ← prompt kontext pro AI
    ├── .AnalyzeBoundariesRuleBasedAsync()          ← guard clauses
    ├── .GenerateBodyFromAiAsync()                  ← tělo metody
    └── .Done()
        ↓
    Core Method s BodyExpressions: ComputedExpression[]
        ↓
    CSharpGenerator → vygenerovaný kód
```

**Příklady:** `QuadraticDemo.cs`, `PriceOptimizer3000.cs`, `SmartWalletAiDemo.cs`

### Cesta B — Business překlad ✅ implementována (Plán 8, 2026-04-18)

```
Uživatel (chat / MCP)
└── "přidej metodu CalculateDiscount do Order"
    ↓
BusinessBehaviorNode { Name, Kind, Summary, Inputs, Returns, Notes }
    ↓
DefaultBusinessTranslator.Translate()
    ├── TranslateBehaviors(entity.Behaviors, language)
    │   ├── CreateBehaviorMethod → TransportMethodDto
    │   │   ├── Inputs → Parameters (CatalogManager.ResolveType → TransportParameterDto)
    │   │   ├── Returns → ReturnType (ResolveType → DataType.Void | Primitive | Custom)
    │   │   ├── Kind=Command → IsAsync=true, Kind=Query/Rule → IsAsync=false
    │   │   └── Summary + Notes → TransportCommentDto (Documentation)
    ↓
TransportClassDto.Methods plněny z Behaviors
```

`DefaultBusinessTranslator.TranslateBehaviors()` — B1–B4 implementovány. Typy se resolví přes `CatalogManager.ResolveType()`; nerozpoznané typy se mapují jako `DataType.Custom` s původním názvem (ne fallback na String).

---

## 2. Klíčový princip: metoda je testovatelná bez závislostí

Každá `Method` v Core je navržena jako čistá funkce:

- **Vstup = parametry** (`method.Parameters`) — vše co metoda potřebuje je injektováno dovnitř
- **Výstup = return type** (`method.ReturnType`) — závislost ven je pouze přes return
- Žádné side-effects mimo explicitní `ComputedOperation.Assign` na field/property
- `RoslynTestRunner` může metodu zkompilovat a otestovat izolovaně bez celé třídy

Toto je designový požadavek, ne náhoda — umožňuje AI generovat tělo metody bez znalosti okolního kódu.

---

## 3. AI generování těla metody — jak to funguje dnes

### Tok

```
MethodBuilder.GenerateBodyFromAiAsync(settings)
    ↓
AiConstraintInferencer.AnalyzeAsync(method, returnCtorHint)
    ├── BuildPromptForMethod(method)              ← vstup: signaturu, Doc(), Constraints
    ├── Jedno HTTP volání na LLM
    └── Odpověď JSON:
        ├── boundaryCases[]       → MethodConstraint[]
        └── codeGenerationActions[] → AddVar, AddIf, AddElseIf, AddElse, AddThen, AddEndIf, Return
    ↓
ApplyCodeGenerationActions(actions)
    ↓
ComputedExpression[] v method.BodyExpressions
    ↓
CSharpGenerator renderuje kód
```

### Co AI dostává v promptu

| Část | Obsah |
|------|-------|
| Signatura | `methodName`, `parameters (name: type)`, `returnType` |
| Business kontext | `method.Documentation.Text` (z `.Doc()`) — toto je nejdůležitější |
| Existující tělo | `method.ResolvedBodyFor(CSharp)` — pokud už něco existuje |
| Guard clauses | Existující `method.Constraints` — AI nesmí generovat duplikáty |
| Constructor hint | Název + pořadí parametrů konstruktoru return typu (zabrání halucinaci) |

### `returnCtorHint` — proč je kritický

Pokud return type je vlastní struct/class (např. `BreakEvenResult`), AI bez hintu halucinuje
pořadí parametrů konstruktoru. `returnCtorHint` injektuje přesnou signaturu:

```
Return type constructor: BreakEvenResult(double x1, double x2, double discriminant)
```

AI pak vygeneruje `return new BreakEvenResult(x1, x2, D)` — ve správném pořadí.

---

## 4. ApplyCodeGenerationActions — tři průchody a jejich problémy

`MethodBuilder.ApplyCodeGenerationActions()` přeloží JSON akce na `ComputedExpression` objekty.
Implementuje tři průchody kvůli opakovaně pozorovaným AI chybám:

### PRŮCHOD 0 — Guard extraction

Extrahuje počáteční `AddIf` + `AddThen(throw ...)` vzory (guard clauses).
Oddělí je od ostatního kódu, aby se mohly emitovat **po** deklaracích proměnných.

**Proč:** AI někdy generuje guards odkazující lokální proměnné (`D < 0`) dříve než
`AddVar(D, ...)` proměnnou deklaruje.

### PRŮCHOD 1 — Hoist AddVar

Přesune všechna `AddVar` před první podmínkový příkaz (`AddIf`, `AddElseIf`, `AddElse`).

**Omezení (neřešeno):** Hoist funguje jen pro `AddVar` které se nacházejí PŘED `firstConditionalIdx`.
Pokud AI vygeneruje `AddVar` za podmínkami (např. za celým `if/elseif/else` chain), hoist ho
nenajde a `AddVar` zůstane za podmínkami které ho referují. Příklad:

```json
// AI výstup — špatně:
[AddIf("D < 0"), AddThen(...), AddElseIf("D == 0"), AddThen(...), AddVar("D", ...), Return(...)]
//                              ↑ referuje D        ↑ referuje D   ↑ deklarace přijde pozdě
```

### PRŮCHOD 2 — Emit delayed guards

Po hoistingu proměnných emituje odložené guard clauses.

### PRŮCHOD 3 — Zbývající akce

Zpracuje If/ElseIf/Else chain s manuálním sledováním `indentLevel`.

---

## 5. Opakovaně pozorované chyby AI generování

| Chyba | Projev | Stav |
|-------|--------|------|
| `AddVar` za podmínkami co ho referují | `D` použit v `if(D<0)` ale deklarován za blokem | Částečně řešeno (hoist) |
| `AddVar` uvnitř `AddThen` | Deklarace proměnné uvnitř branch místo na scope level | Neřešeno |
| Chybějící `return` větve | CS0161 — not all paths return | Částečně (hasReturn check) |
| Halucinace konstruktoru | Špatné pořadí parametrů v `return new X(...)` | Řešeno (returnCtorHint) |
| Duplicitní guard clauses | AI generuje guard i přestože je v `existingConditions` | Řešeno (existingConditions set) |
| Jednoduché uvozovky v string literálech | `'text'` místo `"text"` | Řešeno (FixStringLiterals) |

---

## 6. Detekce prázdného / neúplného těla

Po `GenerateBodyFromAiAsync()` se kontroluje:

```csharp
var bodyCount = method.GetBodyExpressionsCount();
var hasReturn  = method.HasReturnExpression();

if (bodyCount == 0 || !hasReturn)
{
    method.ClearBodyExpressions();
    ApplyFallbackBody(method);   // deterministický fallback
}
```

**Gemini insight:** AI může vrátit tělo bez pádu ale bez `return` — `bodyCount > 0` ale `hasReturn == false`.
Metoda by neprošla Roslyn kompilací (CS0161). Nutné kontrolovat obojí.

---

## 7. Fallback strategie

```
GenerateBodyFromAiAsync()
    ├── AI nedostupná (TestConnectionAsync = false)  → deterministický fallback
    ├── bodyCount == 0                                → deterministický fallback
    ├── !hasReturn                                    → deterministický fallback
    └── OK                                           → AI tělo použito
```

`SmartWalletAiDemo.ApplyFallbackBody()` — vzorový deterministický fallback.
`QuadraticDemo` — explicitní `If/ElseIf/Else` chain jako fallback.

---

## 8. Self-Healing — vize a problémy

### Motivace

Platforma je pro uživatele **black box** — vidí jen výsledný kód, ne proces. Pokud AI
vygeneruje strukturálně vadný kód (špatné pořadí, chybějící return větev), uživatel
nemá jak zasáhnout. Self-healing umožňuje platformě opravit chybu automaticky bez čekání
na záplatu od vývojáře.

### Správný oracle: testy, ne golden code

Porovnávat AI výstup s jiným AI výstupem je kruhový argument — oba sdílí stejnou chybovost.

**Správný oracle = AI generuje test cases z business intent:**

```
BusinessBehavior.Summary + Notes
        ↓
AI generuje: vstupy → očekávané výstupy (test cases)
        ↓
RoslynTestRunner spustí testy na vygenerované metodě
        ↓
FailedCount > 0  → JIT repair
PassedCount == total  → IsVerified = true
```

Testy jsou deterministické a zakódují business intent. Kód je proměnný.

### JIT Repair flow

```
1. RoslynTestRunner.RunAsync(method)
   → BoundaryTestResult.FailedCount > 0
        ↓
2. Uložit selhání jako MethodConstraint { Source = FromRoslynTest, IsVerified = false }
   (obsahuje: co selhalo, jaký vstup, jaký výstup byl očekáván)
        ↓
3. AiConstraintInferencer.AnalyzeAsync(method, returnCtorHint)
   — rozšířený prompt obsahuje: "Failing tests: [...]"
   — AI vrátí nové CodeGenerationActions
        ↓
4. method.ClearBodyExpressions()
   ApplyCodeGenerationActions(newActions)
        ↓
5. RoslynTestRunner.RunAsync(method) znovu
   → pass → IsVerified = true
   → fail znovu → MaxRepairAttempts vyčerpán
        ↓
6. Po vyčerpání pokusů: emit best-effort s TODO komentářem
   MethodConstraint s IsVerified = false zůstane jako "health debt"
   Uživatel vidí varování, metoda se vygeneruje ve stavu který prošel nejdál
```

### Tři reálné problémy self-healing

**P1 — Počet pokusů (MaxRepairAttempts)**

Bez limitu = nekonečná smyčka. Doporučená hodnota: 2–3 pokusy.
Po vyčerpání → trvalý `MethodConstraint` jako "health debt", emit best-effort.

**P2 — Business kontext v repair promptu**

AI při repair potřebuje vidět nejen "co selhalo" ale "proč business task tohle vyžaduje".
Prerekvizita: `BusinessBehaviorNode.Notes` + `Summary` → `method.Documentation.Text`.
Bez business kontextu AI opravuje slepě a může rozbít jinou část metody.

**P3 — Jazyková agnostičnost**

JIT repair musí opravovat `ComputedExpression` model, **ne C# string**.
Repair prompt musí striktně vyžadovat `CodeGenerationActions` JSON — stejný formát jako
`ParseCodeGenerationActions()`. AI nesmí v repair odpovědět raw C# kódem.

---

## 9. Co chybí — prerekvizity v pořadí

| # | Co chybí | Proč je bloker |
|---|----------|---------------|
| ~~**B1**~~ | ~~`DefaultBusinessTranslator` → přeložit `entity.Behaviors` na `Methods`~~ | ✅ implementováno — `TranslateBehaviors()`, Plán 8 |
| ~~**B2**~~ | ~~`BusinessBehaviorInputNode` → `Parameter` + `_catalog.ResolveType()`~~ | ✅ implementováno — `CreateBehaviorParameter()`, Plán 8 |
| ~~**B3**~~ | ~~`BusinessBehaviorNode.Returns` (string) → `ReturnType` (TypeModel)~~ | ✅ implementováno — `ResolveReturnType()`, Plán 8 |
| ~~**B4**~~ | ~~`Summary` + `Notes` → `method.Documentation.Text`~~ | ✅ implementováno — `BuildBehaviorDocumentation()`, Plán 8 |
| **SH1** | AI generuje test cases z business popisu | Správný oracle pro self-healing |
| **SH2** | `RoslynTestRunner` jako trigger po `ApplyCodeGenerationActions` | Uzavření smyčky |
| **SH3** | JIT repair: `AnalyzeAsync` s rozšířeným promptem + failing tests | Samotný repair |
| **FIX1** | Globální závislostní hoist v `ApplyCodeGenerationActions` | Oprava nejčastější příčiny špatného pořadí |
| **FIX2** | `AddVar` uvnitř `AddThen` → přesun na scope level | Proměnná deklarovaná ve větvi místo před ní |

---

## 10. Existující infrastruktura

| Komponenta | Soubor | Využití |
|-----------|--------|---------|
| `Method` | `Src/MetaForge.Core/Elements/Members/Method.cs` | Core model metody |
| `ComputedExpression` | `Src/MetaForge.Core/Elements/Expressions/ComputedExpression.cs` | Jazykově agnostický výraz |
| `ComputedOperation` | tamtéž | Return, Assign, DeclareVariable, Conditional, IfChain, Raw... |
| `MethodConstraint` | `Src/MetaForge.Core/Elements/Members/MethodConstraint.cs` | Guard clauses, `IsVerified`, `ConstraintSource` |
| `ConstraintSource` | tamtéž | `FromRuleBased`, `FromAIBoundaryAnalysis`, `FromRoslynTest` |
| `AiConstraintInferencer` | `Src/MetaForge.Ai/AiConstraintInferencer.cs` | Prompt builder, `AnalyzeAsync()`, `ParseCodeGenerationActions()` |
| `MethodBuilder` | `Src/MetaForge.Builders/Members/MethodBuilder.cs` | Fluent API, `GenerateBodyFromAiAsync()`, `ApplyCodeGenerationActions()` |
| `RoslynTestRunner` | `Src/MetaForge.Core/Internal/Testing/RoslynTestRunner.cs` | Interní kompilace + spuštění testů |
| `RoslynSyntaxValidator` | `Src/MetaForge.Core/Validation/RoslynSyntaxValidator.cs` | Syntaktická validace vygenerovaného C# |
| `BusinessBehaviorNode` | `Src/MetaForge.BusinessModel/Models/BusinessBehaviorNode.cs` | Business popis metody |
| `DefaultBusinessTranslator` | `Src/MetaForge.Translator/DefaultBusinessTranslator.cs` | B→C překlad — `TranslateBehaviors()` implementováno ✅ |
| `TransportMethodDto` | `Src/MetaForge.Dto/TransportContracts.cs` | DTO pro Method (Methods na TransportClassDto existuje) |

---

## 11. Otevřené otázky

| OQ | Otázka |
|----|--------|
| OQ-M1 | **Globální hoist**: stačí rozšíření PRŮCHODU 1 nebo je potřeba celý dependency graph pass? |
| OQ-M2 | **Business → ReturnType**: AI má odvodit typ z volného textu Returns, nebo uživatel musí zadat explicitní typový identifikátor? |
| OQ-M3 | **Test case generování**: kde žije AI která generuje testy — `AiSegment.Healing` nebo nový segment `TestGeneration`? |
| OQ-M4 | **MaxRepairAttempts**: kde se konfiguruje — `AiPlatformConfiguration`, per-method override, nebo globální konstanta? |
| OQ-M5 | **Health debt viditelnost**: jak se "health debt" MethodConstraint zobrazí uživateli — varování v MCP odpovědi, nebo jen v ExpertProjection? |
| OQ-M6 | **Jazyková agnostičnost repair**: Roslyn kompiluje C# — pro TypeScript/Python/Go repair musí fungovat jinak. Odložit na single-language fázi? |

---

*Dokument aktualizován 2026-04-18. Prerekvizity B1–B4 implementovány v Plánu 8. Self-healing (SH1–SH3) zůstává jako otevřený navazující řez.*
