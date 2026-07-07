using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje C# struct (včetně record struct).
/// </summary>
public sealed class StructElement : RootElement
{
    public override string Kind => "struct";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsReadOnly { get; set; }
    public bool IsRecord { get; set; }

    public List<PropertyElement> Properties { get; } = new();
    public List<MethodElement> Methods { get; } = new();

    /// <summary>Přetížené operátory structu.</summary>
    public List<OperatorElement> Operators { get; } = new();

    /// <summary>Generické typové parametry (např. `T` v `struct Pair&lt;T&gt;`).</summary>
    public List<TypeParameterElement> TypeParameters { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin) + Operators.Sum(o => o.Coin);

    // === Statické factory metody (S1-S4 matice) ===

    /// <summary>S1: public struct Point { }</summary>
    public static StructElement Basic(string name) => new() { Name = name };

    /// <summary>S2: public readonly struct Point { }</summary>
    public static StructElement ReadOnly(string name) => new() { Name = name, IsReadOnly = true };

    /// <summary>S3: public record struct Point { }</summary>
    public static StructElement Record(string name) => new() { Name = name, IsRecord = true };

    /// <summary>S4: public readonly record struct Point { }</summary>
    public static StructElement ReadOnlyRecord(string name) => new()
    {
        Name = name,
        IsReadOnly = true,
        IsRecord = true,
    };

    // === Fluent rozšiřovací metody ===

    /// <summary>Nastaví access modifier.</summary>
    public StructElement WithAccess(AccessModifier access)
    {
        AccessModifier = access;
        return this;
    }

    /// <summary>Přidá vlastnost do structu.</summary>
    public StructElement WithProperty(PropertyElement property)
    {
        Properties.Add(property);
        return this;
    }

    /// <summary>Přidá metodu do structu.</summary>
    public StructElement WithMethod(MethodElement method)
    {
        Methods.Add(method);
        return this;
    }

    /// <summary>Přidá přetížený operátor do structu.</summary>
    public StructElement WithOperator(OperatorElement op)
    {
        Operators.Add(op);
        return this;
    }

    /// <summary>Přidá generický typový parametr.</summary>
    public StructElement WithTypeParameter(TypeParameterElement typeParameter)
    {
        TypeParameters.Add(typeParameter);
        return this;
    }
}
