# PROP-045 Generator E2E Completeness

Typ výsledku: Candidate Proposal
Zdroj podnětu: E2E Stress Testing (5 scénářů)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-09

Priorita: 🟡 Vysoká
Oblast: Generators / Tests
Owner:
Datum vytvoření: 2026-07-09
Aktualizováno: 2026-07-09

Navazuje na:
- PROP-043 Generator Completeness — renderery pro Expression/Statement typy zavedeny
- PROP-037 C# Completeness — EventElement, OperatorElement, DelegateElement

Blokuje:
- PROP-025 (Generators scaffolding) — před scaffoldingem musíme vědět, co všechno generátor umí

Související soubory:
- `Tests/MetaForge.Generators.Tests/CSharp/EndToEndScenariosTests.cs`
- `Src/MetaForge.Generators/CodeGenerator.cs`
- `Src/MetaForge.Generators/ExpressionRenderer.cs`

## 1. Kontext

Po dokončení PROP-043 (Generator Completeness) byly implementovány renderery pro všechny typy Expression (15/15) a Statement (13/13). Bylo připraveno 5 E2E scénářů simulujících reálné použití: QuadraticEquation, AutoRepairShop, OrderSystem, UserManagement, Calculator. Všech 5 scénářů prošlo — generátor produkuje syntakticky validní C# ověřené přes Roslyn `SyntaxValidator`.

Scénáře však pokrývají pouze podmnožinu implementovaných rendererů:
- **Expression typy otestované**: BinaryExpression, UnaryExpression (jen Negate), MethodCallExpression, MemberAccessExpression (jen single-segment), ConstantExpression — **5/15**
- **Statement typy otestované**: BlockStatement, AssignmentStatement, IfStatement, ReturnStatement — **4/13**
- **Element typy otestované**: ClassElement (static + instance), EnumElement, InterfaceElement, PropertyElement, FieldElement (readonly), ConstructorElement, MethodElement — základní pokrytí
- **Element typy NETESTOVANÉ**: EventElement, OperatorElement, DelegateElement, StructElement

## 2. Problém dnes

- Renderery existují, ale nevíme, zda fungují v kombinaci (např. `AwaitExpression` uvnitř `BlockStatement` uvnitř `async` metody)
- EventElement, OperatorElement, DelegateElement mají generátor (PROP-043), ale nebyly nikdy otestovány v E2E pipeline
- Multi-segment `MemberAccessExpression` ("Customer.Address.City") netestován
- `NewExpression` s object initializerem netestován
- Lambda výrazy, ternary operátory, pattern matching — renderery hotové, ale nulové pokrytí
- Async metody s tělem (ne jen signaturou) netestovány

## 3. Cíl

Rozšířit E2E testovací matici tak, aby pokrývala **všechny implementované renderery** v reálných kombinacích. Cílový stav: každý Expression/Statement typ je alespoň jednou použit v E2E testu, který projde přes `SyntaxValidator`.

## 4. Architektonické invarianty

- BusinessAuthoringDocument zůstává source of truth.
- Core nesmí nést logiku, která patří do vyšší vrstvy.
- Generátor zůstává oddělený od Core — vstupem jsou Core elementy, výstupem string.

## 5. Scope

### In scope
- Rozšířit `EndToEndScenariosTests.cs` o scénáře 6-12 pokrývající zbylé renderery
- Ověřit EventElement, OperatorElement, DelegateElement generování
- Ověřit async metody s `await` v těle
- Ověřit `for`/`foreach`/`switch`/`while`/`try-catch`/`using` statementy
- Ověřit LambdaExpression, NewExpression, ConditionalExpression, SwitchExpression
- Ověřit multi-segment MemberAccessExpression
- Ověřit StructElement generování
- Dokumentovat případné selhání jako Issues

### Out of scope
- Oprava nalezených chyb (to budou samostatné Issues/PROPs)
- Namespace/using generování (PROP-025)
- Multi-file output (PROP-025)
- Project scaffolding (PROP-025)
- TypeModel API zjednodušení (OQ-001)

## 6. Návrh řešení

### Cílový návrh
Přidat 7 nových E2E scénářů do `EndToEndScenariosTests.cs`:

| # | Název | Testované featury |
|---|-------|-------------------|
| 6 | AsyncPipeline | AwaitExpression, async metoda s tělem, Task<T> |
| 7 | CollectionProcessor | foreach, while, List<T>, indexer |
| 8 | ErrorHandling | try-catch-finally, throw |
| 9 | EventSystem | EventElement, OperatorElement, +=/-= |
| 10 | TypeConversion | ConversionExpression, DefaultExpression, IsPatternExpression |
| 11 | LinqStyle | LambdaExpression, Where/Select pattern, extension methods |
| 12 | StructsAndDelegates | StructElement, DelegateElement, nested types |

### Rozdělení odpovědností
- Generators: již hotovo (renderery existují)
- Tests: `EndToEndScenariosTests.cs` — přidat scénáře 6-12
- Core: beze změny

### Proč je tento návrh správný
Každý scénář je samostatný test, který prochází celou pipeline: Core element → CodeGenerator → Roslyn SyntaxValidator. Selhání okamžitě ukáže, který renderer nebo kombinace nefunguje. Nejedná se o unit test rendereru — testujeme integraci rendererů v reálném use-case.

## 7. Implementační dopad

### Změněné projekty nebo soubory
- `Tests/MetaForge.Generators.Tests/CSharp/EndToEndScenariosTests.cs` — přidat 7 testovacích metod
- `Docs/Issues/ISS-0xx_*.md` — nové issues pro nalezené chyby

### API a kontrakty
- Beze změny — pouze testy

### Testy
- 5 existujících E2E testů
- 7 nových E2E testů
- Cíl: 12 E2E scénářů celkem

### Dokumentace
- `Docs/Plans/PROP-045-Generator-E2E-Completeness.md` (tento soubor)

## 8. Implementační fáze

### Fáze 1 — Základní async + řídicí struktury (1 den)
- Scénář 6: AsyncPipeline (await v těle, Task<T>)
- Scénář 7: CollectionProcessor (foreach, while)

### Fáze 2 — Error handling + Events (1 den)
- Scénář 8: ErrorHandling (try-catch-finally, throw)
- Scénář 9: EventSystem (EventElement, OperatorElement)

### Fáze 3 — Pokročilé výrazy + Struct/Delegate (1 den)
- Scénář 10: TypeConversion (Conversion, Default, IsPattern)
- Scénář 11: LinqStyle (Lambda, extension metody)
- Scénář 12: StructsAndDelegates (StructElement, DelegateElement)

## 9. Otevřené otázky

- OQ-001: Měli bychom zjednodušit TypeModel API před psaním dalších testů? (aktuálně `TypeModel.Of(DataType.Entity).WithCustomName("User")` je verbose)
- Měly by EventElement/OperatorElement generovat i `add`/`remove` accessory nebo jen deklarace?

## 10. Rizika a trade-offy

- Riziko: Některé renderery mohou mít skryté bugy, které E2E testy odhalí → vzniknou nové Issues
- Riziko: DelegateElement/EventElement/OperatorElement renderery nemusí být plně funkční (byly přidány v PROP-043 bez E2E testů)
- Vědomý kompromis: Nejdříve testujeme, pak opravujeme. Testy budou failovat, dokud se renderery neopraví.

## 11. Validace

- Build: `dotnet build Tests/MetaForge.Generators.Tests`
- Testy: `dotnet test --filter "EndToEndScenarios"` → 12/12 pass
- Smoke scénáře: Každý scénář projde `SyntaxValidator.IsValid()`
- Ruční kontrola: Vizuální inspekce vygenerovaného kódu u každého scénáře
