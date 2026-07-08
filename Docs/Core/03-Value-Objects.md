# Core Value Objects — StrongType, Validace, Katalog

> StrongType / ValueObject systém: definice, validace, konverze, katalog.

## StrongType

Reprezentuje uživatelsky definovaný value type (např. `CustomerId`, `Email`) s validačními pravidly.

```csharp
public sealed record StrongType(
    string Name,
    TypeModel Underlying,
    IReadOnlyList<ValueObjectValidationRule>? ValidationRules,
    ConversionOptions? Conversion
);
```

### Příklad

| C# | Core |
|----|------|
| `readonly record struct CustomerId(int Value);` | `new StrongType("CustomerId", TypeModel.Int32, rules: ..., conversion: ...)` |

---

## ValueObjectValidationRule

```csharp
public sealed record ValueObjectValidationRule(
    string RuleName,
    string? Parameter,
    string? ErrorMessage
);
```

Příklady pravidel:
- `NotEmpty` — hodnota nesmí být prázdná/výchozí
- `MinLength(3)` — `RuleName="MinLength"`, `Parameter="3"`
- `MaxLength(100)` — `RuleName="MaxLength"`, `Parameter="100"`
- `Regex("^[a-z]+$")` — `RuleName="Regex"`, `Parameter="^[a-z]+$"`
- `Range(0, 100)` — `RuleName="Range"`, `Parameter="0-100"`

---

## ConversionOptions

```csharp
public sealed record ConversionOptions(
    bool GenerateImplicitConversion,
    bool GenerateExplicitConversion,
    bool GenerateToString,
    bool GenerateEquals,
    bool GenerateGetHashCode
);
```

---

## CatalogManager — StrongType Registry

StrongType objekty se registrují v `CatalogManager`:

```csharp
catalog.RegisterStrongType(new StrongType(
    "CustomerId",
    TypeModel.Int32,
    new[] { new ValueObjectValidationRule("MinValue", "1", "CustomerId must be >= 1") },
    new ConversionOptions(true, true, true, true, true)
));
```

Zpětný lookup:
```csharp
var strongType = catalog.FindStrongType("CustomerId");
var allStrongTypes = catalog.GetAllStrongTypes();
```

---

## CoreDetail / SyncState (BusinessModel → Core write-back)

Value objects v BusinessModel vrstvě dostávají Core metadata přes `CoreDetail` a `SyncState`:

```csharp
// BusinessAttributeNode v BusinessModel vrstvě má:
public BusinessAttributeCoreDetail? CoreDetail { get; set; }
// Obsahuje: TypeModel, ValidationRules, ConversionOptions
```

Write-back z Translatoru aktualizuje `CoreDetail` na BusinessAttributeNode přes `SetCoreDetailOp`.

---

## Stav podpory: ✅ Supported

| Funkce | Stav |
|--------|------|
| StrongType definice | ✅ |
| Validační pravidla | ✅ |
| Konverzní možnosti | ✅ |
| Katalog (registrace/lookup) | ✅ |
| CoreDetail write-back | ✅ |
| Preset value objects | 🔵 (plánováno: Email, Phone, Url, atd.) |
