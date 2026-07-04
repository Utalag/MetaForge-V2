# Core — Code Elements

> Class, Interface, Enum, Struct, Property, Method, Parameter

---

## ClassElement

```csharp
public sealed class ClassElement : RootElement
{
    public override string Kind => "class";
    public string? BaseClassName { get; set; }
    public List<string> ImplementedInterfaces { get; } = new();
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }
    public bool IsStatic { get; set; }
    public bool IsPartial { get; set; }
    public List<PropertyElement> Properties { get; } = new();
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);
}
```

## InterfaceElement

```csharp
public sealed class InterfaceElement : RootElement
{
    public override string Kind => "interface";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public List<PropertyElement> Properties { get; } = new();
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);
}
```

## EnumElement

```csharp
public sealed class EnumElement : RootElement
{
    public override string Kind => "enum";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public DataType UnderlyingType { get; set; } = DataType.Int32;
    public bool IsFlags { get; set; }
    public List<EnumMemberElement> Members { get; } = new();

    public override int TotalCoin =>
        Coin + Members.Sum(m => m.Coin);
}

public sealed class EnumMemberElement
{
    public string Name { get; set; } = string.Empty;
    public object? Value { get; set; }
    public List<AttributeElement> Attributes { get; } = new();

    public int Coin { get; set; } = 1;
}
```

## StructElement

```csharp
public sealed class StructElement : RootElement
{
    public override string Kind => "struct";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsReadOnly { get; set; }
    public bool IsRecord { get; set; }
    public List<PropertyElement> Properties { get; } = new();
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);
}
```

## PropertyElement

```csharp
public sealed class PropertyElement
{
    public string Name { get; set; } = string.Empty;
    public TypeModel Type { get; set; } = TypeModel.Object;
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool HasGetter { get; set; } = true;
    public bool HasSetter { get; set; } = true;
    public bool IsInitOnly { get; set; }
    public bool IsRequired { get; set; }
    public bool IsStatic { get; set; }
    public string? DefaultValue { get; set; }

    public int Coin { get; set; } = 2;
}
```

## MethodElement

```csharp
public sealed class MethodElement
{
    public string Name { get; set; } = string.Empty;
    public TypeModel ReturnType { get; set; } = TypeModel.Void;
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsStatic { get; set; }
    public bool IsAsync { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    public List<ParameterElement> Parameters { get; } = new();
    public List<AttributeElement> Attributes { get; } = new();
    public string? Body { get; set; }

    public int Coin { get; set; } = 5;
    public int TotalCoin => Coin + Parameters.Sum(p => p.Coin);
}
```

## ParameterElement

```csharp
public sealed class ParameterElement
{
    public string Name { get; set; } = string.Empty;
    public TypeModel Type { get; set; } = TypeModel.Object;
    public bool HasDefaultValue { get; set; }
    public string? DefaultValue { get; set; }
    public ParameterModifier Modifier { get; set; } = ParameterModifier.None;

    public int Coin { get; set; } = 1;
}

public enum ParameterModifier
{
    None,
    Ref,
    Out,
    In,
    Params,
}
```
