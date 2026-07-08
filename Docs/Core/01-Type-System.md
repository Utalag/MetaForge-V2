# Core Type System — Primitiva, Nullable, Kolekce, Generika

> Jak Core reprezentuje typový systém .NET: primitiva, složené typy, generika.

## TypeModel

Základní stavební kámen typového systému. Každá vlastnost, parametr nebo návratová hodnota má přiřazený `TypeModel`.

```csharp
public sealed record TypeModel
{
    public DataType BaseType { get; init; }
    public bool IsNullable { get; init; }
    public bool IsCollection { get; init; }
    public string? CustomTypeName { get; init; }
    public List<TypeModel> GenericArguments { get; init; } = new();
}
```

### Statické factory

| Metoda | Výsledek |
|--------|----------|
| `TypeModel.Void` | `void` |
| `TypeModel.String` | `string` |
| `TypeModel.Int32` | `int` |
| `TypeModel.Bool` | `bool` |
| `TypeModel.Of(DataType.Int32)` | `int` |
| `.MakeNullable()` | `int?` |
| `.MakeCollection()` | `List<int>` |
| `.WithCustomName("Customer")` | `Customer` |
| `.WithGenericArg(TypeModel.String)` | `List<string>` |

## DataType — Primitivní typy

```csharp
public enum DataType
{
    // Číselné
    Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Int128,
    Half, Single, Double, Decimal, NInt, NUInt,

    // Textové
    Char, String,

    // Binární
    Binary,

    // Časové
    DateOnly, TimeOnly, DateTime, DateTimeOffset, TimeSpan,

    // Speciální
    Bool, Guid, Uri, Version,

    // Placeholdery
    Entity, EnumValue, Object, Dynamic, Void, Array, Nullable, Struct, Record
}
```

Stav: ✅ Supported — celkem 32 hodnot.

## Příklady TypeModel

### C# → Core

| C# typ | Core reprezentace |
|--------|------------------|
| `int` | `TypeModel.Of(DataType.Int32)` |
| `int?` | `TypeModel.Of(DataType.Int32).MakeNullable()` |
| `string` | `TypeModel.String` |
| `List<int>` | `TypeModel.Of(DataType.Int32).MakeCollection()` |
| `Customer` | `TypeModel.Of(DataType.Entity).WithCustomName("Customer")` |
| `Task<Customer>` | `TypeModel.Of(DataType.Entity).WithCustomName("Task").WithGenericArg(TypeModel.Of(DataType.Entity).WithCustomName("Customer"))` |
| `Dictionary<string, int>` | `TypeModel.Of(DataType.Entity).WithCustomName("Dictionary").WithGenericArg(TypeModel.String).WithGenericArg(TypeModel.Int32)` |

### Omezení

- Generic typy jsou reprezentovány jako `Entity` s `CustomTypeName` — nerozlišuje se mezi `Task<T>` a `ValueTask<T>` na úrovni DataType.
- `Nullable<T>` je reprezentován přes `IsNullable` flag, ne jako `Nullable<T>` generic.
- Collection typy (`List<T>`, `IEnumerable<T>`, `T[]`) používají `IsCollection` flag.
