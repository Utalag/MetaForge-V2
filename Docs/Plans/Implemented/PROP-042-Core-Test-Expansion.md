# PROP-042 Core Test Expansion — Full Test Matrix

Typ výsledku: Candidate Proposal
Zdroj podnětu: AI — Perplexity Deep Research (konverzace 2293d4a6)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-08

Priorita: High
Oblast: Core, Tests
Owner:
Datum vytvoření: 2026-07-08
Aktualizováno: 2026-07-08

Navazuje na:
- PROP-032 (Integrační testy Core + Generators)
- PROP-036 (Core Specification Layer — invarianty)
- Perplexity test matrix: https://www.perplexity.ai/search/2293d4a6-aca7-4219-aeac-8d3285213a71

Blokuje:
- —

Související soubory:
- `Tests/MetaForge.Core.Tests/Validation/`
- `Tests/MetaForge.Core.Tests/Specifications/`
- `Tests/MetaForge.Core.Integration.Tests/Scenarios/`

## 1. Kontext

Perplexity Deep Research navrhl kompletní testovací matici pro Core vrstvu po revizi. Matice identifikuje 4 oblasti:

1. **Property-based testy** (FsCheck) — generátory a invarianty pro náhodné testování
2. **Snapshot testy** — .expected.cs pro všechny varianty elementů
3. **Guard/boundary testy** — neplatné kombinace, které CoreValidator musí chytit (ale dnes nechytá)
4. **Integrační testy s Roslyn** — Core → Generátor → Roslyn kompilace → 0 errors

## 2. Problém dnes

- **Chybí property-based testy:** Validace se testuje jen na pevně daných případech, ne na náhodných kombinacích.
- **Chybí snapshoty pro reálné scénáře:** Existuje ~48 snapshot testů, ale chybí např. generika, dědičnost, async metody.
- **CoreValidator nechytá G-02, G-10, G-11, G-12** — tyto neplatné kombinace produkují nezkompilovatelný kód.
- **Chybí Roslyn integrační testy:** Ověření, že generovaný kód je syntakticky a sémanticky korektní.

## 3. Cíl

- Implementovat kompletní testovací matici dle Perplexity návrhu.
- Přidat FsCheck property-based testy (8 testů).
- Rozšířit snapshot testy o chybějící scénáře (15+ snapshotů).
- Doplnit guard testy pro chybějící validace (12 testů).
- Vytvořit Roslyn integrační testy (6+ testů).

## 4. Architektonické invarianty

- CoreValidator zůstává volitelný — model může být nevalidní, validace je až při generování.
- Property-based testy jsou additive — žádné změny v Core.

## 5. Scope

### In scope

**Property-based testy (FsCheck):**

| # | Test | Invariant | Priorita |
|---|------|-----------|----------|
| PB-01 | ClassElement_ValidName_AlwaysPassesValidation | Valid Name + Public/Internal → Validate() empty | High |
| PB-02 | ClassElement_InvalidTopLevelAccess_AlwaysHasIssue | Private/Protected → issues A3/A4/A5 | High |
| PB-03 | TotalCoin_AlwaysGreaterOrEqualCoin | TotalCoin >= Coin vždy | High |
| PB-04 | TotalCoin_Additive | Přidání property = TotalCoin vzroste | High |
| PB-05 | ClassElement_UniqueIds | Nové ClassElement() → různá Id | Medium |
| PB-06 | MethodElement_TypeParametersCount_Consistent | TypeConstraints ≤ TypeParameters | Medium |
| PB-07 | FluentChaining_Idempotent | WithAccess(x).WithAccess(x) = WithAccess(x) | Low |
| PB-08 | ValidateProperty_NeverThrows | validate nikdy nevyhazuje | High |

**Snapshot testy:**

| # | Snapshot | Priorita |
|---|----------|----------|
| SN-01 | BasicClass | High |
| SN-02 | ClassWithProperties | High |
| SN-03 | ClassWithMethods | High |
| SN-04 | RecordPrimaryConstructor | High |
| SN-05 | GenericWithConstraints | High |
| SN-06 | ClassInheritance | Medium |
| SN-07 | ClassWithUsings | Medium |
| SN-08 | AbstractClassWithMethods | High |
| SN-09 | AsyncMethods | High |
| SN-10 | StaticClass | Medium |
| SN-11 | FlagsEnum | Medium |
| SN-12 | ReadOnlyStruct | Medium |

**Guard testy — chybějící validace:**

| # | Validace | Typ |
|---|----------|-----|
| G-01 | C13: abstract sealed class → Error (static) | CoreValidator |
| G-02 | C14: abstract sealed record → Error | CoreValidator |
| G-03 | A7A8: internal protected + public → Error | CoreValidator |
| G-04 | K1: constructor s void return → Error | CoreValidator |
| G-05 | P9: required property bez required keyword → Warning | CoreValidator |
| G-06 | P10: init-only property s private set → Warning | CoreValidator |
| G-07 | S1: struct s base class → Error | CoreValidator |
| G-08 | M13: abstract async → Error | CoreValidator |
| G-09 | M14: extension method ne static → Warning | CoreValidator |
| G-10 | M15: override metoda bez odpovídající base → Warning | CoreValidator |
| G-11 | C15: partial record → Warning | CoreValidator |
| G-12 | C16: static partial → Warning | CoreValidator |

**Roslyn integrační testy:**

| # | Test | Vstup | Priorita |
|---|------|-------|----------|
| IT-01 | BasicClass_GeneratesCompilableCode | ClassElement.Basic("Foo") | High |
| IT-02 | ClassWithProperties | Třída + 3 properties | High |
| IT-03 | GenericWithConstraints | ClassElement.Generic(...) | High |
| IT-04 | RecordWithPrimaryConstructor | ClassElement.PrimaryRecord(...) | High |
| IT-05 | AsyncMethod_ReturnsTask | MethodElement.Async(...) | High |
| IT-06 | InvalidClass_Detected | AbstractSealed → validation fail | High |

### Out of scope
- Generátorové změny pro nové elementy (ConstructorElement, FieldElement) — vlastní PROP.
- FsCheck adaptér v Core — zůstává v test projektu.

## 6. Návrh řešení

### FsCheck helpers

```csharp
// Tests/Helpers/Generators.cs
public static class Generators
{
    public static Arbitrary<string> ValidIdentifier() => ...
    public static Arbitrary<AccessModifier> AnyAccessModifier() => ...
    public static Arbitrary<AccessModifier> TopLevelAccessModifier() => ...
    public static Arbitrary<AccessModifier> InvalidTopLevelAccessModifier() => ...
}
```

### Property-based testy

```csharp
[Property]
public Property TotalCoin_AlwaysAtLeastCoin()
{
    return Prop.ForAll(
        Arb.From(Gen.Choose(0, 100)),
        Gen.Choose(0, 10).ListOf(),
        (baseCoin, propCoins) => {
            var cls = new ClassElement { Coin = baseCoin };
            foreach (var c in propCoins)
                cls.Properties.Add(new PropertyElement { Coin = c });
            return cls.TotalCoin >= baseCoin;
        });
}
```

### Roslyn integrační testy

```csharp
public class CompilationTests
{
    private static bool CompilesClean(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(syntaxTree)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Count() == 0;
    }
}
```

## 7. Implementační dopad

### Změněné projekty nebo soubory
- `Tests/MetaForge.Core.Tests/Validation/GuardValidationTests.cs` — 12 nových testů
- `Tests/MetaForge.Core.Tests/Helpers/Generators.cs` — FsCheck generátory
- `Tests/MetaForge.Core.Tests/PropertyBased/ClassPropertyTests.cs` — PB-01 až PB-08
- `Tests/MetaForge.Core.Integration.Tests/Scenarios/CompilationTests.cs` — IT-01 až IT-06
- `Tests/MetaForge.Core.Integration.Tests/Snapshots/` — SN-01 až SN-12

### Testy
- 8 nových property-based testů (FsCheck)
- 12 nových snapshot testů
- 12 nových guard testů
- 6 nových Roslyn integračních testů
- Celkem ~38 nových testů

### Dokumentace
- Update Docs/Core/00-Support-Matrix.md (nové G-kódy)
- Update New_Architecture/15-Test-Scaffold.md

## 8. Implementační fáze

### Fáze 1 — FsCheck helpers + property-based testy
- Vytvořit Generators.cs
- Implementovat PB-01 až PB-08

### Fáze 2 — Snapshot testy
- Vytvořit .expected.cs soubory pro SN-01 až SN-12
- Vytvořit testovací třídy

### Fáze 3 — Guard testy
- Implementovat G-01 až G-12 v CoreValidator
- Vytvořit testy

### Fáze 4 — Roslyn integrační testy
- Vytvořit CompilationTests.cs
- Implementovat IT-01 až IT-06
