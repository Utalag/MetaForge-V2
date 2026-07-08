# Core Roundtrip Boundary — C# → Core → C#

> Definice roundtrip kontraktu: co projde překladem C# → Core → C# beze ztráty, co se ztratí a co není podporováno.

## Co je roundtrip?

**Roundtrip** = schopnost převést C# kód do Core modelu a zpět do ekvivalentního C# kódu beze změny sémantiky.

MetaForge dnes roundtrip negarantuje — Core je **authoring-first** model, ne lossless reprezentace C# kódu. Roundtrip slouží jako **užitečná vlastnost** pro testování a validaci, ne jako cílový stav.

## Kategorie roundtrip

| Kategorie | Popis | Příklad |
|-----------|-------|---------|
| **Full Roundtrip** | C# → Core → C# = identický kód (modulo whitespace) | `public class Foo { }` |
| **Lossy Roundtrip** | C# → Core → C# = ekvivalentní, ale ne identický | Pořadí members, using organizace |
| **Unsupported** | Nelze převést do Core → nelze roundtrip | `unsafe`, `ref struct`, direktivy preprocesoru |

## Full Roundtrip — garantované konstrukce

| Konstrukce | Poznámka |
|-----------|----------|
| `class` / `struct` / `interface` / `enum` deklarace | Včetně access modifiers |
| `sealed`, `abstract`, `static` modifikátory | Na typech |
| Properties (get/set/get-only) | Včetně typů a access modifiers |
| Method signatury | Název, return type, parametry, modifikátory |
| Generic typy a metody | Type parameters + constraints |
| Dědičnost (base class, interfaces) | |
| XML documentation (`<summary>`) | |
| Attributes | |

## Lossy Roundtrip — informace se ztratí

| Konstrukce | Co se ztratí |
|-----------|-------------|
| Pořadí members | Core neuchovává původní pořadí |
| Whitespace / formátování | Core neřeší formátování |
| Using direktivy (pořadí/alias) | Zachová se seznam, ne pořadí |
| `partial` class (merge sémantika) | Core nevidí ostatní partial soubory |
| `record` → `class` s immutable properties | Zachová se jako `class` s `IsRecord` flag — generátor to zase převede na `record` |
| `init` accessor | Zachová se flag `IsInitOnly` |
| Expression body (`=>`) | Zachová se jako `ExpressionBody` |

## Unsupported — nelze roundtrip

| Konstrukce | Důvod |
|-----------|--------|
| Těla metod (implementace) | Core neukládá plná těla jako AST (pouze signaturu nebo text) |
| `unsafe` code / pointery | Mimo scope Core |
| `ref struct` / `ref` fields | Mimo scope Core |
| Preprocessor directives (`#if`, `#region`) | Mimo scope Core |
| `using` alias direktivy | Mimo scope Core |
| Primary constructors (non-record) | Mimo scope Core |
| `Deconstruct` metody | Mimo scope Core |
| Local functions | V Core jako `LocalFunctionStatement`, ale ne v roundtrip |
| `stackalloc`, `fixed` | Mimo scope Core |
| `dynamic` (plná sémantika) | Jen jako `DataType.Dynamic` placeholder |

## Diagnostika roundtrip

Pro detekci ztrátových oblastí používá Core:

1. **Snapshot testy** (`SnapshotComparer`) — porovnání generovaného výstupu s `.expected.cs`
2. **Syntax validace** (`SyntaxValidator.IsValid()`) — Roslyn ověření syntaktické správnosti
3. **Invarianty** (`BuiltInInvariants`) — validace sémantických pravidel

## Budoucí směr

- `MethodBodyKind` (IDEA-004) — explicitní označení typu těla
- Roslyn Importer (PROP-037) — přímý import z C# syntax tree do Core
- Roundtrip testy jako CI gate — selhání při regresi roundtrip kontraktu
