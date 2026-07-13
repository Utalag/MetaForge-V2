# PROP-048 Generator Render Core — Unit Tests pro ExpressionRenderer a TemplateManager

Typ výsledku: Candidate Proposal
Zdroj podnětu: IDEA-025 (Koumák + Perplexity)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-11

Priorita: High
Oblast: Generators, Tests
Owner:
Datum vytvoření: 2026-07-11
Aktualizováno: 2026-07-11

Navazuje na:
- PROP-045 (Generator E2E Completeness — hotovo)
- PROP-043 (Generator Completeness — hotovo)
- PROP-032 (Integration Tests Core+Generators — hotovo)

Blokuje:
- —

Související soubory:
- `Src/MetaForge.Generators/ExpressionRenderer.cs`
- `Src/MetaForge.Generators/TemplateManager.cs`
- `Tests/MetaForge.Generators.Tests/`
- `Tests/MetaForge.Core.Integration.Tests/`

## 1. Kontext

Analýza test coverage (IDEA-025) odhalila, že `ExpressionRenderer` a `TemplateManager` nemají přímé unit testy — jsou testovány pouze nepřímo přes E2E scénáře (PROP-045, 13/13 scénářů). Perplexity to označila za zrádné: *"E2E scénář ti řekne, že něco prošlo, ale neřekne ti rychle a přesně, která vrstva se rozbila."*

Dokumentace `New_Architecture/10-Generators.md` počítá s `CodeGenerator`, `TemplateManager`, `ExpressionRenderer` a packagingem jako s klíčovými stavebními kameny — ale testovány jsou jen okrajově.

## 2. Problém dnes

- **ExpressionRenderer** renderuje 15 typů expressionů a 13 typů statementů, ale každý typ je testován jen nepřímo. Pokud se rozbije `RenderBinary`, E2E test kvadratické rovnice selže, ale nebude jasné, zda chyba je v rendereru, v šabloně, nebo v mapování typů.
- **TemplateManager**: klíčová komponenta (Scriban loader + cache), **žádné unit testy**. Chování při chybějící šabloně, invalidním Scriban syntaxi, souběžném přístupu z více vláken — netestováno.
- **Chybějící snapshoty**: Delegate, Constructor s parametry, Field, Flags enum — v Support Matrix označeny jako Supported, ale chybí snapshot test, který by to potvrdil.

## 3. Cíl

- Každý expression typ a statement typ má vlastní unit test, který ověří, že renderuje korektní C#.
- `TemplateManager` má testy pro LoadTemplate (existence/neexistence), cache hit/miss, Scriban syntax error handling.
- Chybějící snapshoty jsou doplněny.
- Při regresi v rendereru unit test okamžitě identifikuje "rozbil se Binary renderer" místo "nesedí E2E scénář".

## 4. Architektonické invarianty

- Generators zůstávají úzkou C#→text vrstvou.
- Testy jsou additivní — nemění stávající API.

## 5. Scope

### In scope
- Unit testy pro `ExpressionRenderer`: 15 expresních kategorií × 1-6 testů = ~40 testů
- Unit testy pro `TemplateManager`: LoadTemplate, cache, errors = ~8 testů
- Chybějící snapshoty: Delegate, Constructor, Field, Flags enum, Generic class, Primary constructor = ~8 snapshotů
- Vytvoření adresářové struktury `Tests/MetaForge.Generators.Tests/Renderer/`

### Out of scope
- Změny v `ExpressionRenderer` nebo `TemplateManager` kódu (pouze testy)
- Změny v E2E testech (PROP-045)
- Vytvoření sdílené testovací knihovny (řeší PROP-049)
- Nové expression nebo statement typy

## 6. Návrh řešení

### ExpressionRenderer unit testy

Samostatná test třída `ExpressionRendererTests` s kategoriemi:

| Kategorie | Testy | Očekávaný C# výstup |
|-----------|-------|---------------------|
| Constants | 6 | `null`, `"hello"`, `'x'`, `true`, `42`, `3.14m` |
| Binary | 15 | `a + b`, `a - b`, `a * b`, `a / b`, `a % b`, `a && b`, `a \|\| b`, `a == b`, `a != b`, `a < b`, `a > b`, `a <= b`, `a >= b`, `a ?? b`, `a + b + c` |
| Unary | 5 | `!a`, `-a`, `~a`, `++a`, `--a` |
| MethodCall | 3 | `Foo()`, `Foo(a, b)`, `obj.Foo().Bar()` |
| MemberAccess | 2 | `obj.Property`, `obj.Nested.Property` |
| Conditional | 2 | `a ? b : c`, vnořený |
| Lambda | 2 | `x => x + 1`, `(x, y) => x + y` |
| New | 2 | `new Foo()`, `new Foo { Name = "x" }` |
| Await | 1 | `await task` |
| Conversion | 2 | `(int)x`, implicitní |
| Default | 1 | `default(T)` |
| IsPattern | 2 | `x is string`, `x is not null` |
| NullCoalescing | 2 | `a ?? b`, `a ?? b ?? c` |
| SwitchExpression | 1 | `x switch { 1 => "one", _ => "other" }` |

Samostatná test třída `StatementRendererTests`:

| Kategorie | Testy |
|-----------|-------|
| Block | 1 |
| Return | 2 (s hodnotou, bez) |
| If/Else | 3 (if, if/else, vnořený) |
| For | 2 (basic, s proměnnou) |
| While | 1 |
| Assignment | 2 (=, +=) |
| ExpressionStatement | 1 |
| Switch | 1 |
| ForEach | 1 |
| TryCatch | 2 (try/catch, try/catch/finally) |
| Using | 2 (block, declaration) |
| LocalFunction | 1 |

### TemplateManager unit testy

- `LoadTemplate` — existující template, neexistující (FileNotFoundException)
- Cache hit/miss — načtení 2× vrátí stejnou instanci
- Scriban syntax error — `InvalidOperationException`
- ClearCache — ověření vyprázdnění
- Thread safety — ConcurrentDictionary

### Chybějící snapshoty

Doplnit do `Tests/MetaForge.Core.Integration.Tests/Scenarios/`:
- `DelegateSnapshots.cs` — basic, generic, s parametry
- `ConstructorSnapshots.cs` — basic, s parametry, static, private
- `FieldSnapshots.cs` — basic, readonly, static, const
- `EnumVariantsSnapshots.cs` — Flags enum (E4 již existuje, ověřit)
- `GenericClassSnapshots.cs` — jeden a více type paramů, constraints
- `PrimaryConstructorSnapshots.cs` — record class

## 7. Implementační dopad

### Změněné projekty nebo soubory
- `Tests/MetaForge.Generators.Tests/Renderer/ExpressionRendererTests.cs` — nový
- `Tests/MetaForge.Generators.Tests/Renderer/StatementRendererTests.cs` — nový
- `Tests/MetaForge.Generators.Tests/Renderer/TemplateManagerTests.cs` — nový
- `Tests/MetaForge.Core.Integration.Tests/Scenarios/*Snapshots.cs` — nové snapshot třídy
- `Tests/MetaForge.Core.Integration.Tests/Snapshots/*.expected.cs` — nové expected soubory

### API a kontrakty
- Žádné změny — pouze additivní testy.

### Testy
- ~40 unit testů pro ExpressionRenderer
- ~20 unit testů pro StatementRenderer
- ~8 unit testů pro TemplateManager
- ~8 snapshot testů

### Dokumentace
- Žádná změna — testy dokumentují samy sebe.

## 8. Implementační fáze

### Fáze 1: ExpressionRenderer unit testy (~2 dny)
- Konstanty, Binary, Unary, MemberAccess, MethodCall
- Conditional, Lambda, New, Await, Conversion, Default
- IsPattern, NullCoalescing, SwitchExpression
- Statement testy (Block, Return, If/Else, For, While, Assignment, atd.)

### Fáze 2: TemplateManager unit testy (~0.5 dne)
- LoadTemplate, cache, errors, thread safety

### Fáze 3: Chybějící snapshoty (~1 den)
- Delegate, Constructor, Field, Flags enum
- Generic class, Primary constructor

## 9. Otevřené otázky

- **OQ-048-01**: Kam umístit chybějící snapshoty? Do `Integration.Tests` (stávající vzor) nebo do `Generators.Tests` (blíže ke generátoru)? Stávající vzor používá `Integration.Tests`.
- **OQ-048-02**: Mají mít TemplateManager testy mock filesystemu, nebo stačí reálné soubory s embedded test templates?

## 10. Rizika a trade-offy

- **Riziko regrese**: Žádné — testy jsou additivní.
- **Riziko scope creep**: Snapshotů může být víc, než se odhaduje — omezit na první iteraci na 8 klíčových.
- **Vědomý kompromis**: Ne všechny kombinace modifikátorů jsou pokryty — pouze reprezentativní vzorek.

## 11. Validace

- Build: `dotnet build` bez chyb a warningů
- Testy: všechny nové testy prochází; stávající testy nejsou ovlivněny
- Smoke scénáře: spuštění `dotnet test Tests/MetaForge.Generators.Tests/`
- Ruční kontrola: každý test ověřuje korektní C# syntaxi
- Jak poznáme, že je návrh hotový: 100% pokrytí expression typů (15/15) a statement typů (13/13) unit testy + TemplateManager unit testy

## 12. Výsledek po dokončení

*— vyplnit při uzavření —*
