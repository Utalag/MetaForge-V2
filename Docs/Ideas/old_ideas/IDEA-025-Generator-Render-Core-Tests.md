# IDEA-025 Generator Render Core — ExpressionRenderer + TemplateManager Unit Tests

Stav: Idea
Oblast: Core, Tests, Generators
Zdroj: Koumák + Perplexity konverzace db7f49c1-8f18-4f2e-8da2-b9b6e1f5c122
Datum vytvoření: 2026-07-11
Poslední revize: 2026-07-11

## 1. Kontext

Analýza test coverage odhalila, že `ExpressionRenderer` a `TemplateManager` nemají přímé unit testy — jsou testovány pouze nepřímo přes E2E scénáře. Perplexity to označila za zrádné: *"E2E scénář ti řekne, že něco prošlo, ale neřekne ti rychle a přesně, která vrstva se rozbila."*

Dokumentace `10-Generators.md` počítá s `CodeGenerator`, `TemplateManager`, `ExpressionRenderer` a packagingem jako s klíčovými stavebními kameny — ale testovány jsou jen okrajově.

## 2. Problém dnes

- **ExpressionRenderer**: renderuje 15 typů expressionů a 12 typů statementů, ale každý typ je testován jen nepřímo. Pokud se rozbije `RenderBinary`, E2E test kvadratické rovnice selže, ale nebude jasné, zda chyba je v rendereru, v šabloně, nebo v mapování typů.
- **TemplateManager**: klíčová komponenta (Scriban loader + cache), žádné unit testy. Chování při chybějící šabloně, invalidním Scriban syntaxi, souběžném přístupu z více vláken — netestováno.
- **Generic types + Primary constructors**: v Core modelu existují (`ClassElement.TypeParameters`, `ClassElement.PrimaryConstructorParameters`), šablony je podporují, ale neexistuje snapshot test, který by ověřil, že výstup je korektní C#.
- **Chybějící snapshoty**: Interface, Delegate, Constructor, Field, Flags enum, Property P6/P7.

## 3. Předběžný směr řešení

### 3.1 ExpressionRenderer unit testy
Samostatná test třída pro každou kategorii:

| Kategorie | Testy | Příklady |
|-----------|-------|----------|
| Constants | 6 | null, string, char, bool, int, decimal |
| Binary | 15 | add, subtract, multiply, divide, modulo, and/or, comparisons, concat, null-coalesce |
| Unary | 5 | not, negate, bitwise-not, increment, decrement |
| MethodCall | 3 | no args, with args, chained |
| MemberAccess | 2 | simple path, nested path |
| Conditional | 2 | simple ternary, nested ternary |
| Lambda | 2 | single param, multi param |
| New | 2 | no args, with member bindings |
| Await | 1 | await expression |
| Conversion | 2 | implicit, explicit |
| Default | 1 | default(T) |
| IsPattern | 2 | is string, is not null |
| NullCoalescing | 2 | simple, chained |
| SwitchExpression | 1 | pattern match |

Obdobně pro statementy: 12 kategorií × 2-3 testy = ~30 testů.

### 3.2 TemplateManager unit testy
- LoadTemplate — existující, neexistující, s cache hit/miss
- Render — validní model, prázdný model, chybějící proměnná v modelu
- ClearCache — ověření, že se cache vyprázdní
- Thread safety — ConcurrentDictionary chování
- Scriban syntax error — graceful handling

### 3.3 Chybějící snapshoty
- **Interface**: basic interface s property + method signatures
- **Delegate**: basic, generic, s parametry
- **Constructor**: basic, s parametry, s tělem, bez těla
- **Field**: basic, readonly, static, s default hodnotou
- **Flags enum**: `[Flags]` enum s hodnotami
- **Property P6/P7**: dohledat, které varianty v matici chybí
- **Generic class**: s jedním a více type parametry, s constrainty
- **Primary constructor**: record class s primary constructor parametry
- **Nullable collection**: `List<int>?` — kombinace nullable + collection

## 4. Signál hodnoty

- **Rychlá diagnostika regresí**: unit test řekne přesně "rozbil se binary renderer" místo "nesedí kvadratická rovnice".
- **Pokrytí kontraktu**: každý expression/statement typ bude mít explicitní test, že renderuje korektní C#.
- **Jistota pro refactoring**: TemplateManager cache nebo ExpressionRenderer přepis nebude hazard.
- **Perplexity to označila za prioritu #1** v "render core" balíku.
- Zapadá do Epic 9 (testy) a Epic 7 (Generators).

## 5. Rizika a nejasnosti

- ExpressionRenderer unit testy vyžadují vytváření AST node ručně — verbose, ale přímočaré.
- TemplateManager testy可能需要 mockování filesystemu — závislost na `TemplateManager.Instance` (singleton) může komplikovat testování.
- Snapshot testy pro Interface, Delegate atd. rozšíří celkový počet snapshotů o ~15-20 — je třeba zvážit, zda Integration.Tests je správné místo, nebo zda udělat snapshoty i v Generators.Tests.
- Hranice mezi "public supported" a "internal capability" (viz IDEA-024) ovlivní, kolik z těchto testů je povinných.

## 6. Aktuální stav

✅ Převedeno na Candidate → PROP-048

## 7. Doporučený další krok

**Candidate Proposal** — odhad pracnosti:
- ExpressionRenderer unit: ~40 testů (2 dny)
- TemplateManager unit: ~10 testů (1 den)
- Chybějící snapshoty: ~20 snapshotů (2 dny)
- Celkem: ~5 dní

Navazuje na: `10-Generators.md`, `Properties/PropertyElement.cs`, `ExpressionRenderer.cs`, `TemplateManager.cs`
Otevřená otázka: Mají snapshoty pro nové elementy být v Integration.Tests nebo v dedicated snapshot projektu?
