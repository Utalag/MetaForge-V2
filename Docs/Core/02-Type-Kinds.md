# Core Type Kinds — Class, Struct, Interface, Enum, Record

> Přehled typových druhů (TypeKind) a jejich Core reprezentace.

## Přehled

| TypeKind | Core Element | Kind string | Hlavní vlastnosti |
|----------|-------------|-------------|-------------------|
| Class | `ClassElement` | `"class"` | BaseClass, Interfaces, IsAbstract, IsSealed, IsStatic, IsPartial, IsRecord |
| Struct | `StructElement` | `"struct"` | IsReadOnly, IsRecord, PrimaryConstructorParameters |
| Interface | `InterfaceElement` | `"interface"` | TypeParameters, TypeConstraints |
| Enum | `EnumElement` | `"enum"` | UnderlyingType, IsFlags, Members |
| Record | `ClassElement` (IsRecord=true) | `"class"` | Stejné jako Class + IsRecord |

---

## Class

```csharp
public class ClassElement : RootElement
{
    // Dědičnost
    public string? BaseClassName { get; init; }
    public List<string> ImplementedInterfaces { get; init; } = new();

    // Generika
    public List<string> TypeParameters { get; init; } = new();
    public List<GenericConstraint> TypeConstraints { get; init; } = new();

    // Modifikátory
    public AccessModifier AccessModifier { get; init; } = AccessModifier.Public;
    public bool IsAbstract { get; init; }
    public bool IsSealed { get; init; }
    public bool IsStatic { get; init; }
    public bool IsPartial { get; init; }
    public bool IsRecord { get; init; }

    // Členy
    public List<PropertyElement> Properties { get; init; } = new();
    public List<MethodElement> Methods { get; init; } = new();

    // Primary constructor (record)
    public List<ParameterElement>? PrimaryConstructorParameters { get; init; }
}
```

### Statické factory

| Metoda | Výsledek |
|--------|----------|
| `ClassElement.Basic("Customer")` | `public class Customer` |
| `ClassElement.Abstract("Base")` | `public abstract class Base` |
| `ClassElement.Sealed("Final")` | `public sealed class Final` |
| `ClassElement.Static("Utils")` | `public static class Utils` |
| `ClassElement.Partial("Partial")` | `public partial class Partial` |
| `ClassElement.Record("Dto")` | `public record Dto` |
| `ClassElement.AbstractRecord("BaseDto")` | `public abstract record BaseDto` |
| `ClassElement.Generic("Repo", ["T"])` | `public class Repo<T>` |

### Fluent builder

```csharp
var customer = new ClassBuilder("Customer")
    .Sealed()
    .BaseClass("EntityBase")
    .Implements("IComparable<Customer>")
    .Property("Name", TypeModel.String, p => p.GetSet().Required())
    .Method("GetId", m => m.Returns(TypeModel.Guid))
    .Build();
```

### Stav podpory: ✅ Supported (s omezeními Partial Class, Primary Constructor)

---

## Interface

```csharp
public class InterfaceElement : RootElement
{
    public AccessModifier AccessModifier { get; init; } = AccessModifier.Public;
    public List<string> TypeParameters { get; init; } = new();
    public List<GenericConstraint> TypeConstraints { get; init; } = new();
    public List<PropertyElement> Properties { get; init; } = new();
    public List<MethodElement> Methods { get; init; } = new();
}
```

### Omezení: 🔵 Partial
- Interface methods nemají výchozí implementace (C# 8 default interface methods).
- Generic variance (`in`/`out`) není podporována.

---

## Struct

```csharp
public class StructElement : RootElement
{
    public AccessModifier AccessModifier { get; init; } = AccessModifier.Public;
    public bool IsReadOnly { get; init; }
    public bool IsRecord { get; init; }
    public List<string> TypeParameters { get; init; } = new();
    public List<GenericConstraint> TypeConstraints { get; init; } = new();
    public List<ParameterElement>? PrimaryConstructorParameters { get; init; }
    public List<PropertyElement> Properties { get; init; } = new();
    public List<MethodElement> Methods { get; init; } = new();
}
```

### Statické factory

| Metoda | Výsledek |
|--------|----------|
| `StructElement.Basic("Point")` | `public struct Point` |
| `StructElement.ReadOnly("Vector")` | `public readonly struct Vector` |
| `StructElement.Record("Data")` | `public record struct Data` |

### Omezení: 🔵 Partial
- `ref struct` není podporováno.

---

## Enum

```csharp
public class EnumElement : RootElement
{
    public AccessModifier AccessModifier { get; init; } = AccessModifier.Public;
    public DataType UnderlyingType { get; init; } = DataType.Int32;
    public bool IsFlags { get; init; }
    public List<EnumMemberElement> Members { get; init; } = new();
}

public class EnumMemberElement
{
    public string Name { get; init; } = "";
    public object? Value { get; init; } // null = auto
}
```

### Statické factory

| Metoda | Výsledek |
|--------|----------|
| `EnumElement.Basic("Color")` | `public enum Color : int` |
| `EnumElement.ByteEnum("Small")` | `public enum Small : byte` |
| `EnumElement.Flags("Permissions")` | `[Flags] public enum Permissions` |

### Omezení: 🔵 Partial
- Explicitní hodnoty členů jsou podporovány, ale bez bitových operací pro Flags.

---

## GenericConstraint

```csharp
public sealed record GenericConstraint(
    string TypeParameterName,
    List<ConstraintKind> Constraints,
    string? BaseTypeName,
    List<string> InterfaceNames
);

public enum ConstraintKind
{
    Class, Struct, ParameterlessCtor, NotNull, Unmanaged, BaseType, Interface
}
```

### Statické factory

```csharp
GenericConstraint.Class("T")              // where T : class
GenericConstraint.Struct("T")             // where T : struct
GenericConstraint.ParameterlessCtor("T")  // where T : new()
GenericConstraint.NotNull("T")            // where T : notnull
GenericConstraint.Unmanaged("T")          // where T : unmanaged
GenericConstraint.BaseType("T", "Entity") // where T : Entity
GenericConstraint.Interface("T", ["IDisposable"]) // where T : IDisposable
```
