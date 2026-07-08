# PROP-024: Core — StrongType/ValueObject, Expression System, Record Elementy

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-002 (Core base — hotovo), PROP-020 (BusinessModel upgrade)

---

## Cíl

Rozšířit Core vrstvu o tři klíčové koncepty pro plnohodnotnou podporu ForgeBlock presetů a business model enrichmentu:
1. **StrongType/ValueObject** — typový systém pro doménové typy (Money, Email, ...)
2. **Expression System** — hierarchie výrazů pro computed properties a constrainty
3. **Record/RecordStruct elementy** — podpora C# record types

## Odůvodnění

Aktuální Core umí jen primitivní typy (`DataType` enum) a `TypeModel`. Pro ForgeBlock presety (např. `Money` jako `decimal` s 2 desetinnými místy) potřebujeme **StrongType** — typ, který nese validační pravidla a konverzní logiku.

Expression system je nutný pro:
- **Computed properties** — `FullName = FirstName + " " + LastName`
- **Constrainty** — `Price >= 0 AND Price <= 1000000`
- **AI generované výrazy** — AI enrichment navrhne výraz, Core ho musí reprezentovat

Record elementy — C# 10+ `record class` a `record struct` jsou standardní pattern pro immutable DTO. Core by je měl podporovat nativně.

---

## 1. StrongType / ValueObject (A1)

### Rozsah

```csharp
/// <summary>
/// Doménový typ s validačními pravidly — např. Money, Email, PhoneNumber.
/// StrongType je "value object" — identita = hodnota.
/// </summary>
public sealed record StrongType
{
    /// <summary>Název typu (např. "Money").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Podkladový primitivní typ (např. Decimal).</summary>
    public TypeModel Underlying { get; init; } = TypeModel.Object;

    /// <summary>Validační pravidla.</summary>
    public IReadOnlyList<ValueObjectValidationRule> ValidationRules { get; init; } = [];

    /// <summary>Konverze — např. implicitní/explicitní operátory.</summary>
    public ConversionOptions? Conversion { get; init; }
}

/// <summary>Validační pravidlo value objectu.</summary>
public sealed record ValueObjectValidationRule(
    string Kind,       // "Range", "MaxLength", "Regex", "NotNegative", "DecimalPlaces"
    string? Value = null
);

/// <summary>Konverzní možnosti.</summary>
public sealed record ConversionOptions(
    bool ImplicitFromUnderlying = false,
    bool ExplicitToUnderlying = false
);
```

### Integrace s CatalogManager

```csharp
public sealed class CatalogManager
{
    // Nové:
    public void RegisterStrongType(StrongType strongType);
    public StrongType? ResolveStrongType(string typeName);
    
    // Existující ResolveType() zůstává pro primitivní typy
}
```

### Příklad: Money StrongType

```csharp
var money = new StrongType(
    Name: "Money",
    Underlying: TypeModel.Decimal,
    ValidationRules: [
        new("DecimalPlaces", "2"),
        new("NotNegative")
    ],
    Conversion: new(ImplicitFromUnderlying: true)
);
catalog.RegisterStrongType(money);

// Použití:
var resolved = catalog.ResolveStrongType("money");
// → StrongType { Name="Money", Underlying=Decimal, Rules=[DecimalPlaces:2, NotNegative] }
```

### Výstup

| Soubor | Umístění |
|--------|----------|
| `StrongType.cs` | `Src/MetaForge.Core/ValueObjects/` |
| `ValueObjectValidationRule.cs` | `Src/MetaForge.Core/ValueObjects/` |
| `ConversionOptions.cs` | `Src/MetaForge.Core/ValueObjects/` |
| Rozšíření `CatalogManager.cs` | `Src/MetaForge.Core/Catalog/` |

---

## 2. Expression System (A2)

### Rozsah

Hierarchie výrazů inspirovaná `System.Linq.Expressions`:

```csharp
/// <summary>Abstraktní bázová třída pro všechny výrazy.</summary>
public abstract record Expression
{
    /// <summary>Typ výrazu (pro dispatch).</summary>
    public abstract ExpressionKind Kind { get; }

    /// <summary>Výsledný typ výrazu.</summary>
    public TypeModel ResultType { get; init; } = TypeModel.Object;
}

public enum ExpressionKind
{
    Constant,       // 42, "hello", true
    MemberAccess,   // entity.FirstName
    Binary,         // a + b, a > b, a AND b
    Unary,          // !a, -a
    MethodCall,     // string.IsNullOrEmpty(name)
    Lambda,         // (x) => x.FirstName
    New,            // new Customer { Name = "..." }
    Conditional,    // a ? b : c
    Default,        // default(int)
    Conversion,     // (decimal)price
}

// Konkrétní výrazy:
public sealed record ConstantExpression(object? Value) : Expression;
public sealed record MemberAccessExpression(string MemberPath) : Expression;
public sealed record BinaryExpression(Expression Left, BinaryOperator Op, Expression Right) : Expression;
public sealed record MethodCallExpression(string MethodName, IReadOnlyList<Expression> Arguments) : Expression;
// ...
```

### ExpressionRenderer

Existující `ExpressionRenderer.cs` v Generators rozšířit o podporu všech `ExpressionKind`:

```csharp
public sealed class ExpressionRenderer
{
    public string RenderCSharp(Expression expr);
    public string RenderScriban(Expression expr);
    public string RenderSql(Expression expr); // pro budoucí Dapper/EF generátory
}
```

### Výstup

| Soubor | Umístění |
|--------|----------|
| `Expression.cs` | `Src/MetaForge.Core/Elements/Expressions/` |
| `ExpressionKind.cs` | `Src/MetaForge.Core/Elements/Expressions/` |
| `ConstantExpression.cs` a další | `Src/MetaForge.Core/Elements/Expressions/` |
| Rozšíření `ExpressionRenderer.cs` | `Src/MetaForge.Generators/` |

---

## 3. Record / RecordStruct elementy (A3)

### Rozsah

Přidat `IsRecord` na `ClassElement` a `StructElement` (již existuje jako bool flag). Alternativně — dedikované elementy:

```csharp
/// <summary>
/// C# record class — immutable referenční typ s value-based equality.
/// </summary>
public sealed class RecordClassElement : ClassElement
{
    public override string Kind => "record class";
    // Dědí vše z ClassElement
}

/// <summary>
/// C# record struct — immutable hodnotový typ s value-based equality.
/// </summary>
public sealed class RecordStructElement : StructElement
{
    public override string Kind => "record struct";
    // Dědí vše z StructElement
}
```

**Rozhodnutí:** Použít `IsRecord` flag na existujících elementech (jednodušší). Dedikované elementy jen pokud by flag nestačil.

### Dopad na Generators

`CodeGenerator` musí generovat `record` keyword:

```csharp
// ClassElement s IsRecord = true → "public sealed record Customer"
private string GenerateClass(ClassElement cls)
{
    var keyword = cls.IsRecord ? "record" : "class";
    // ...
}
```

---

## 4. Source Generator integrace (A4)

### Koncept

ForgeBlock balíky distribuované jako **C# Source Generators** — běží při kompilaci, generují kód přímo do výstupní assembly. **Monetizační aspekt:** Source Generator verze ForgeBlocku je zdarma (běží u uživatele), runtime verze s plnou infrastrukturou je placená.

```csharp
// ForgeBlock jako Source Generator (FREE TIER)
[Generator]
public class MoneyForgeBlockGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Vygeneruje Money.cs do projektu uživatele
        context.AddSource("Money.g.cs", GenerateMoneyValueObject());
    }
}
```

### Implementační plán

1. Přidat `ISourceGeneratorCapability` do ForgeBlock modelu
2. Vytvořit šablonu pro Source Generator projekty
3. `PackageManifestGenerator` generuje `.csproj` s `<Generator>` referencí

---

## Odhad

| Fáze | Dny |
|------|-----|
| StrongType/ValueObject + validace | 1 den |
| CatalogManager integrace | 0,5 dne |
| Expression System (hierarchie) | 1,5 dne |
| ExpressionRenderer rozšíření | 0,5 dne |
| Record elementy (IsRecord flag) | 0,5 dne |
| Source Generator šablona | 1 den |
| Testy | 1 den |
| **Celkem** | **6 dní** |

---

## Závislosti

| Závislost | Stav |
|-----------|------|
| PROP-002 (Core base) | ✅ Hotovo |
| PROP-020 (BusinessModel upgrade) | 🟢 Schváleno |

---

## Monetizační vazba

| Tier | Co | Jak |
|------|----|-----|
| **Free** | Domain layer codegen, ForgeBlock Source Generators | Distribuováno jako NuGet Source Generator |
| **Paid** | Infrastructure/API/EF Core codegen | Runtime generování přes MetaForge platformu |
| **Sandbox** | Testovací generování s omezením | Webový sandbox bez možnosti exportu |
