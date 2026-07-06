# 01-Integration-Test-Matrix — Decision Matrix pro integrační testy

> **Zdroj pravdivosti** pro všechny varianty Core elementů a jejich očekávané chování v generátoru.
> Každý ✅ řádek → snapshot integrační test. Každý ❌ řádek → Core unit validation test.
> Stav: 📝 Navrženo — 2026-07-05

---

## Legenda

| Symbol | Význam | Typ testu |
|:------:|--------|-----------|
| ✅ | Validní kombinace — Core validace projde, generátor produkuje kód | Snapshot test v `MetaForge.Core.Integration.Tests` |
| ❌ | Nevalidní kombinace — Core validace musí vyhodit výjimku | Unit test v `MetaForge.Core.Tests` |
| ⚠️ | Technicky kompilovatelné, ale varování / redundantní | Info — netestuje se |
| — | Netýká se / není aplikovatelné | — |

---

## 1. Class — modifikátory

| # | Abstract | Sealed | Static | Partial | Record | Core validace | Očekávaný C# kód |
|:-:|:--------:|:------:|:------:|:-------:|:------:|:-------------:|-------------------|
| C1 | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | `public class Foo { }` |
| C2 | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | `public abstract class Foo { }` |
| C3 | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | `public sealed class Foo { }` |
| C4 | ❌ | ❌ | ✅ | ❌ | ❌ | ✅ | `public static class Foo { }` |
| C5 | ❌ | ❌ | ❌ | ✅ | ❌ | ✅ | `public partial class Foo { }` |
| C6 | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | `public record class Foo { }` |
| C7 | ✅ | ❌ | ❌ | ❌ | ✅ | ✅ | `public abstract record class Foo { }` |
| C8 | ❌ | ✅ | ❌ | ❌ | ✅ | ✅ | `public sealed record class Foo { }` |
| C9 | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | `abstract sealed` — konfliktní |
| C10 | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | `abstract static` — konfliktní |
| C11 | ❌ | ✅ | ✅ | ❌ | ❌ | ⚠️ | redundantní (`static` je implicitně `sealed`) |
| C12 | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ | `static record` — nelze |

---

## 2. Class — access modifiers

| # | AccessModifier | Kontext | Core validace | Očekávaný C# kód |
|:-:|:--------------|--------|:-------------:|-------------------|
| A1 | Public | Top-level | ✅ | `public class Foo { }` |
| A2 | Internal | Top-level | ✅ | `internal class Foo { }` |
| A3 | Private | Top-level | ❌ | `private` nelze na top-level třídě |
| A4 | Protected | Top-level | ❌ | `protected` nelze na top-level třídě |
| A5 | PrivateProtected | Top-level | ❌ | `private protected` nelze na top-level třídě |
| A6 | ProtectedInternal | Top-level | ✅ | `protected internal class Foo { }` |

---

## 3. Class — dědičnost

| # | BaseClassName | ImplementedInterfaces | Core validace | Očekávaný kód |
|:-:|:-------------|:----------------------|:-------------:|---------------|
| I1 | `null` | `[]` | ✅ | `public class Foo { }` |
| I2 | `"Person"` | `[]` | ✅ | `public class Foo : Person { }` |
| I3 | `null` | `["IDisposable"]` | ✅ | `public class Foo : IDisposable { }` |
| I4 | `"Person"` | `["IDisposable", "IComparable"]` | ✅ | `public class Foo : Person, IDisposable, IComparable { }` |
| I5 | `"string"` | `[]` | ❌ | `string` je sealed — nelze dědit |
| I6 | `null` | `["MyClass"]` | ⚠️ | technicky validní, ale interface začíná `I` |

---

## 4. Enum — varianty

| # | UnderlyingType | IsFlags | Core validace | Očekávaný C# kód |
|:-:|:--------------|:-------:|:-------------:|-------------------|
| E1 | Int32 | ❌ | ✅ | `public enum Status { }` |
| E2 | Byte | ❌ | ✅ | `public enum Status : byte { }` |
| E3 | Int64 | ❌ | ✅ | `public enum Status : long { }` |
| E4 | Int32 | ✅ | ✅ | `[Flags] public enum Status { }` |
| E5 | String | ❌ | ❌ | `string` není validní underlying typ pro enum |
| E6 | Bool | ❌ | ❌ | `bool` není validní underlying typ pro enum |

---

## 5. Struct — modifikátory

| # | IsReadOnly | IsRecord | Core validace | Očekávaný C# kód |
|:-:|:----------:|:--------:|:-------------:|-------------------|
| S1 | ❌ | ❌ | ✅ | `public struct Point { }` |
| S2 | ✅ | ❌ | ✅ | `public readonly struct Point { }` |
| S3 | ❌ | ✅ | ✅ | `public record struct Point { }` |
| S4 | ✅ | ✅ | ✅ | `public readonly record struct Point { }` |

---

## 6. Property — modifikátory

| # | HasGetter | HasSetter | IsInitOnly | IsRequired | IsStatic | Core validace | Očekávaný C# kód |
|:-:|:---------:|:---------:|:----------:|:----------:|:--------:|:-------------:|-------------------|
| P1 | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | `public string Name { get; set; }` |
| P2 | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | `public string Name { get; }` |
| P3 | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | `public string Name { get; init; }` |
| P4 | ✅ | ✅ | ❌ | ✅ | ❌ | ✅ | `public required string Name { get; set; }` |
| P5 | ✅ | ✅ | ❌ | ❌ | ✅ | ✅ | `public static string Name { get; set; }` |
| P6 | ✅ | ✅ | ✅ | ✅ | ❌ | ⚠️ | `init` + `required` — redundantní, ale validní |
| P7 | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | property bez getteru i setteru |
| P8 | ✅ | ❌ | ❌ | ✅ | ❌ | ✅ | `public required string Name { get; }` |

---

## 7. Property — TypeModel varianty

| # | TypeModel | Core validace | Očekávaný C# typ |
|:-:|-----------|:-------------:|-------------------|
| T1 | `TypeModel.String` | ✅ | `string` |
| T2 | `TypeModel.Int32` | ✅ | `int` |
| T3 | `TypeModel.Int32.MakeNullable()` | ✅ | `int?` |
| T4 | `TypeModel.String.MakeNullable()` | ✅ | `string?` |
| T5 | `TypeModel.Int32.MakeCollection()` | ✅ | `List<int>` |
| T6 | `TypeModel.String.MakeCollection()` | ✅ | `List<string>` |
| T7 | `TypeModel.Int32.MakeNullable().MakeCollection()` | ✅ | `List<int?>` |
| T8 | `TypeModel.Bool` | ✅ | `bool` |
| T9 | `TypeModel.Decimal` | ✅ | `decimal` |
| T10 | `TypeModel.Guid` | ✅ | `Guid` |
| T11 | `TypeModel.DateTime` | ✅ | `DateTime` |
| T12 | `TypeModel.Of(DataType.Double)` | ✅ | `double` |
| T13 | `TypeModel.Of(DataType.Int64)` | ✅ | `long` |
| T14 | `TypeModel.Of(DataType.Byte)` | ✅ | `byte` |
| T15 | `TypeModel.Of(DataType.DateOnly)` | ✅ | `DateOnly` |
| T16 | `TypeModel.Of(DataType.TimeSpan)` | ✅ | `TimeSpan` |
| T17 | `TypeModel.Object` | ✅ | `object` |
| T18 | `TypeModel.Of(DataType.Dynamic)` | ✅ | `dynamic` |
| T19 | `TypeModel.Void` | ❌ | void nelze jako property type |
| T20 | `TypeModel.Void.MakeNullable()` | ❌ | void? — nevalidní |
| T21 | `TypeModel.Void.MakeCollection()` | ❌ | `List<void>` — nevalidní |
| T22 | `TypeModel.Int32.WithCustomName("CustomerId")` | ✅ | `CustomerId` (custom type) |
| T23 | `TypeModel.String.WithGenericArg(TypeModel.Int32)` | ✅ | `Dictionary<string, int>` (přes generics) |

---

## 8. Method — modifikátory a varianty

| # | Static | Async | Abstract | Virtual | Override | ReturnType | Core validace | Očekávaný výstup |
|:-:|:------:|:-----:|:--------:|:-------:|:--------:|:-----------|:-------------:|-------------------|
| M1 | ❌ | ❌ | ❌ | ❌ | ❌ | Void | ✅ | `public void Execute() { }` |
| M2 | ✅ | ❌ | ❌ | ❌ | ❌ | Double | ✅ | `public static double Calc() { }` |
| M3 | ❌ | ✅ | ❌ | ❌ | ❌ | Task | ✅ | `public async Task Fetch() { }` |
| M4 | ❌ | ✅ | ❌ | ❌ | ❌ | `Task<List<string>>` | ✅ | `public async Task<List<string>> Get() { }` |
| M5 | ❌ | ❌ | ✅ | ❌ | ❌ | String | ✅ | `public abstract string Get();` (bez těla) |
| M6 | ❌ | ❌ | ❌ | ✅ | ❌ | Void | ✅ | `public virtual void OnEvent() { }` |
| M7 | ❌ | ❌ | ❌ | ❌ | ✅ | String | ✅ | `public override string ToString() { }` |
| M8 | ❌ | ✅ | ❌ | ❌ | ❌ | Void | ✅ | `public async Task Process() { }` |
| M9 | ❌ | ❌ | ✅ | ✅ | ❌ | — | ❌ | `abstract virtual` — konflikt |
| M10 | ❌ | ❌ | ✅ | ❌ | ✅ | — | ❌ | `abstract override` — konflikt |
| M11 | ✅ | ❌ | ✅ | ❌ | ❌ | — | ❌ | `static abstract` — nutný interface |
| M12 | ❌ | ❌ | ❌ | ✅ | ✅ | — | ❌ | `virtual override` — konflikt |
| M13 | ❌ | ❌ | ✅ | ❌ | ❌ | — | ⚠️ | Abstraktní metoda nemá tělo (Body = null) |

---

## 9. Statement — AST varianty (pro Method.Body)

| # | Statement typ | Popis | Core validace | Očekávaný C# kód |
|:-:|:------------|-------|:-------------:|-------------------|
| B1 | BlockStatement | Prázdný blok | ✅ | `{ }` |
| B2 | BlockStatement | Více statementů | ✅ | `{ stmt1; stmt2; }` |
| B3 | ReturnStatement | `return 42;` | ✅ | `return 42;` |
| B4 | ReturnStatement | `return;` (void) | ✅ | `return;` |
| B5 | IfStatement | `if (x > 0) { }` | ✅ | `if (x > 0) { }` |
| B6 | IfStatement | `if-else` | ✅ | `if (x > 0) { } else { }` |
| B7 | ForStatement | `for (i = 0; i < n; i++)` | ✅ | `for (int i = 0; i < n; i++) { }` |
| B8 | WhileStatement | `while (x > 0)` | ✅ | `while (x > 0) { }` |
| B9 | AssignmentStatement | `total = price * qty;` | ✅ | `total = price * qty;` |
| B10 | ExpressionStatement | `list.Add(item);` | ✅ | `list.Add(item);` |
| B11 | IfStatement | Condition není Bool type | ❌ | `if ("hello")` — nevalidní |
| B12 | ReturnStatement | Return value v `void` metodě | ❌ | `return 42;` ve `void` metodě |
| B13 | ReturnStatement | Bez value v non-void metodě | ❌ | `return;` v `int` metodě |

---

## 10. Constructor — varianty

| # | Modifikátor | Parametry | Chaining | Core validace | Očekávaný C# kód |
|:-:|:-----------|:----------|:---------|:-------------:|-------------------|
| K1 | Public | 0 | — | ✅ | `public Foo() { }` |
| K2 | Public | 2 | — | ✅ | `public Foo(string n, int a) { }` |
| K3 | Static | 0 | — | ✅ | `static Foo() { }` |
| K4 | Private | 0 | — | ✅ | `private Foo() { }` |
| K5 | Public | 1 | `this()` | ✅ | `public Foo(string n) : this(n, 0) { }` |
| K6 | Public | 1 | `base()` | ✅ | `public Foo(string n) : base(n) { }` |
| K7 | Static | 1 | — | ❌ | static konstruktor nemá parametry |
| K8 | Public | 0 | `this()` + `base()` | ❌ | nelze chainovat oba |

---

## 11. AppRoot — stromový traversal

| # | Scénář | Core validace | Očekávaný výsledek |
|:-:|--------|:-------------:|---------------------|
| R1 | AppRoot s 0 projekty → GenerateAll → 0 artifactů | ✅ | Prázdný výstup |
| R2 | AppRoot → 1 Project → 1 Class → GenerateAll → 1 artifact | ✅ | `Customer.cs` |
| R3 | AppRoot → 2 Projects → 3 Classes, 1 Enum → GenerateAll → 4 artifacty | ✅ | 4 `.cs` soubory |
| R4 | AppRoot → Project s DefaultNamespace → vygenerovaný namespace | ✅ | `namespace MyApp.Core;` |
| R5 | AppRoot → duplicitní názvy elementů → varování v diagnostice | ⚠️ | Ne fail, jen warning |

---

## Souhrn

| Kategorie | ✅ Validní (snapshot) | ❌ Nevalidní (unit test) | ⚠️ Varování | Celkem |
|-----------|:--------------------:|:----------------------:|:-----------:|:------:|
| Class — modifikátory | 8 | 4 | 1 | 13 |
| Class — access modifiers | 3 | 3 | — | 6 |
| Class — dědičnost | 3 | 1 | 1 | 5 |
| Enum — varianty | 4 | 2 | — | 6 |
| Struct — modifikátory | 4 | — | — | 4 |
| Property — modifikátory | 6 | 1 | 1 | 8 |
| Property — TypeModel | 19 | 3 | — | 22 |
| Method — modifikátory | 8 | 4 | 1 | 13 |
| Statement — AST varianty | 10 | 3 | — | 13 |
| Constructor | 6 | 2 | — | 8 |
| AppRoot | 3 | — | 1 | 4 |
| **Celkem** | **74** | **23** | **5** | **102** |

---

## Vazby

- **Tento dokument** = testovací specifikace pro PROP-032 (integrační testy)
- Validní kombinace → `Tests/MetaForge.Core.Integration.Tests/Scenarios/`
- Nevalidní kombinace → `Tests/MetaForge.Core.Tests/Elements/` (unit testy validace)
- Statement AST → závisí na PROP-031 (Statement System)
