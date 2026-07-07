using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje C# třídu — dědí z RootElement.
/// </summary>
public sealed class ClassElement : RootElement
{
    public override string Kind => "class";

    /// <summary>Název bázové třídy (pokud dědí).</summary>
    public string? BaseClassName { get; set; }

    /// <summary>Seznam implementovaných interfaců.</summary>
    public List<string> ImplementedInterfaces { get; } = new();

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }
    public bool IsStatic { get; set; }
    public bool IsPartial { get; set; }

    /// <summary>Určuje, zda je třída record class (C# 9+).</summary>
    public bool IsRecord { get; set; }

    /// <summary>Vlastnosti (property) třídy.</summary>
    public List<PropertyElement> Properties { get; } = new();

    /// <summary>Metody třídy.</summary>
    public List<MethodElement> Methods { get; } = new();

    /// <summary>Eventy třídy.</summary>
    public List<EventElement> Events { get; } = new();

    /// <summary>Přetížené operátory třídy.</summary>
    public List<OperatorElement> Operators { get; } = new();

    /// <summary>Generické typové parametry (např. `T` v `class Repository&lt;T&gt;`).</summary>
    public List<TypeParameterElement> TypeParameters { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin)
             + Events.Sum(e => e.Coin) + Operators.Sum(o => o.Coin);

    // === Statické factory metody pro modifikátorové kombinace (C1-C8 matice) ===

    /// <summary>C1: public class Foo { }</summary>
    public static ClassElement Basic(string name) => new() { Name = name };

    /// <summary>C2: public abstract class Foo { }</summary>
    public static ClassElement Abstract(string name) => new() { Name = name, IsAbstract = true };

    /// <summary>C3: public sealed class Foo { }</summary>
    public static ClassElement Sealed(string name) => new() { Name = name, IsSealed = true };

    /// <summary>C4: public static class Foo { }</summary>
    public static ClassElement Static(string name) => new() { Name = name, IsStatic = true };

    /// <summary>C5: public partial class Foo { }</summary>
    public static ClassElement Partial(string name) => new() { Name = name, IsPartial = true };

    /// <summary>C6: public record class Foo { }</summary>
    public static ClassElement Record(string name) => new() { Name = name, IsRecord = true };

    /// <summary>C7: public abstract record class Foo { }</summary>
    public static ClassElement AbstractRecord(string name) => new()
    {
        Name = name,
        IsAbstract = true,
        IsRecord = true,
    };

    /// <summary>C8: public sealed record class Foo { }</summary>
    public static ClassElement SealedRecord(string name) => new()
    {
        Name = name,
        IsSealed = true,
        IsRecord = true,
    };

    // === Fluent rozšiřovací metody (bez konfliktů, vždy bezpečné) ===

    /// <summary>Nastaví access modifier (A1,A2,A6).</summary>
    public ClassElement WithAccess(AccessModifier access)
    {
        AccessModifier = access;
        return this;
    }

    /// <summary>Nastaví bázovou třídu (I1-I4).</summary>
    public ClassElement WithBaseClass(string? baseClassName)
    {
        BaseClassName = baseClassName;
        return this;
    }

    /// <summary>Přidá implementované interfacy (I3,I4).</summary>
    public ClassElement WithInterfaces(params string[] interfaces)
    {
        ImplementedInterfaces.AddRange(interfaces);
        return this;
    }

    /// <summary>Přidá using direktivy.</summary>
    public ClassElement WithUsings(params string[] usings)
    {
        Usings.AddRange(usings);
        return this;
    }

    /// <summary>Přidá vlastnost do třídy.</summary>
    public ClassElement WithProperty(PropertyElement property)
    {
        Properties.Add(property);
        return this;
    }

    /// <summary>Přidá metodu do třídy.</summary>
    public ClassElement WithMethod(MethodElement method)
    {
        Methods.Add(method);
        return this;
    }

    /// <summary>Přidá event do třídy.</summary>
    public ClassElement WithEvent(EventElement evt)
    {
        Events.Add(evt);
        return this;
    }

    /// <summary>Přidá přetížený operátor do třídy.</summary>
    public ClassElement WithOperator(OperatorElement op)
    {
        Operators.Add(op);
        return this;
    }

    /// <summary>Přidá generický typový parametr.</summary>
    public ClassElement WithTypeParameter(TypeParameterElement typeParameter)
    {
        TypeParameters.Add(typeParameter);
        return this;
    }
}
