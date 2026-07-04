# Core — Services

> CatalogManager, ICatalogProvider, PresetDefinition, StrongType/ValueObject, IConstraintInferencer

**Aktualizace:** PROP-024 (2026-07-04) — StrongType/ValueObject, CatalogManager StrongType registry, IConstraintInferencer, ICatalogProvider.

---

## ICatalogProvider + PresetDefinition

```csharp
public interface ICatalogProvider
{
    string ProviderName { get; }
    IReadOnlyList<PresetDefinition> GetAllPresets();
    PresetDefinition? ResolveType(string typeName);
}

public sealed record PresetDefinition(
    string Name,
    TypeModel Type,
    string? Description = null,
    IReadOnlyList<string>? Tags = null
);
```

### BuiltInCatalogProvider

Definuje 20+ built-in presetů: `int`, `long`, `decimal`, `double`, `float`, `string`, `text`, `bool`, `boolean`, `datetime`, `date`, `time`, `guid`, `uuid`, `email`, `phone`, `url`, `uri`, `money`, `price`.

---

## CatalogManager (PROP-024 rozšíření)

```csharp
public sealed class CatalogManager
{
    // --- Registry providerů ---
    public void RegisterProvider(ICatalogProvider provider);
    public void RegisterPreset(PresetDefinition preset);

    // --- StrongType registry (PROP-024) ---
    public void RegisterStrongType(StrongType strongType);
    public StrongType? ResolveStrongType(string typeName);
    public IReadOnlyList<StrongType> GetAllStrongTypes();

    // --- Vyhledávání ---
    public PresetDefinition? ResolveType(string typeName);  // custom → providers
    public IReadOnlyList<PresetDefinition> SearchPresets(string query);
    public IReadOnlyList<PresetDefinition> GetAllPresets();
}
```

Thread-safe (`ConcurrentDictionary` pro presety a StrongType, `lock` pro providery).

---

## StrongType / ValueObject (PROP-024)

Doménové typy s validačními pravidly — např. Money, Email, PhoneNumber. StrongType je "value object" — identita = hodnota.

```csharp
// Složka: Src/MetaForge.Core/ValueObjects/

public sealed record StrongType(
    string Name,                                    // "Money"
    TypeModel Underlying,                           // TypeModel.Decimal
    IReadOnlyList<ValueObjectValidationRule>? ValidationRules = null,
    ConversionOptions? Conversion = null
);

public sealed record ValueObjectValidationRule(
    string RuleName,        // "Range", "MaxLength", "Regex", "NotNegative", "DecimalPlaces"
    string? Parameter = null,
    string? ErrorMessage = null
);

public sealed record ConversionOptions(
    bool GenerateImplicitConversion = false,
    bool GenerateExplicitConversion = false,
    bool GenerateToString = true,
    bool GenerateEquals = true,
    bool GenerateGetHashCode = true
);
```

### Příklad: Money StrongType

```csharp
var money = new StrongType(
    Name: "Money",
    Underlying: TypeModel.Decimal,
    ValidationRules: [
        new("DecimalPlaces", "2"),
        new("NotNegative"),
    ],
    Conversion: new(GenerateImplicitConversion: true)
);
catalog.RegisterStrongType(money);

var resolved = catalog.ResolveStrongType("money");
// → StrongType { Name="Money", Underlying=Decimal, Rules=[DecimalPlaces:2, NotNegative] }
```

---

## IConstraintInferencer (PROP-024)

```csharp
// Složka: Src/MetaForge.Core/Inference/

public interface IConstraintInferencer
{
    IReadOnlyList<string> Infer(string attributeName, TypeModel type);
}
```

### RuleBasedConstraintInferencer

Rule-based implementace s pravidly pro: `email`, `phone`, `url`, `age`, `price`, `quantity`, `name`, `description`, `password`, `zipcode`, `color`, `percentage`. Fallback na prefix matching, pak type-based inference.

AI implementace: `MetaForge.Ai.Inference.AiConstraintInferencer`.

---

## IForgeBlockPackage

```csharp
public interface IForgeBlockPackage
{
    string Handle { get; }
    string Version { get; }
    IReadOnlyList<ForgeBlockCapability> Capabilities { get; }
    DiscoveryMetadata Discovery { get; }
    void Register(ForgeBlockRegistry registry);
}
```

## IForgeBlockCapabilityPackage

```csharp
public interface IForgeBlockCapabilityPackage : IForgeBlockPackage
{
    ForgeBlockPackageDescriptor Descriptor { get; }
    IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; }
}
```
