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

    /// <summary>
    /// Typové parametry pro generické třídy.
    /// Např. <c>class Repository&lt;T, TKey&gt;</c> → ["T", "TKey"].
    /// Prázdný seznam = negenerická třída.
    /// </summary>
    public List<string> TypeParameters { get; init; } = [];

    /// <summary>
    /// Generic constrainty pro typové parametry.
    /// Např. <c>where T : class, new()</c>, <c>where TKey : IComparable&lt;TKey&gt;</c>.
    /// </summary>
    public List<GenericConstraint> TypeConstraints { get; init; } = [];

    /// <summary>
    /// Parametry primary konstruktoru (C# 12+).
    /// Např. <c>record Point(int X, int Y)</c> → [ParameterElement("X", Int32), ParameterElement("Y", Int32)].
    /// null = bez primary konstruktoru.
    /// </summary>
    public List<ParameterElement>? PrimaryConstructorParameters { get; set; }

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

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);

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

    /// <summary>C9: public record class Foo(int X, int Y); — primary constructor.</summary>
    public static ClassElement PrimaryRecord(string name, params ParameterElement[] parameters) => new()
    {
        Name = name,
        IsRecord = true,
        PrimaryConstructorParameters = parameters.ToList(),
    };

    /// <summary>
    /// C10: public class Repository&lt;T, TKey&gt; where T : class, new() where TKey : IComparable&lt;TKey&gt;.
    /// Vytvoří generickou třídu s typovými parametry a volitelnými constrainty.
    /// </summary>
    public static ClassElement Generic(string name, string[] typeParameters, GenericConstraint[]? constraints = null) => new()
    {
        Name = name,
        TypeParameters = typeParameters.ToList(),
        TypeConstraints = constraints?.ToList() ?? [],
    };

    // === Fluent rozšiřovací metody (bez konfliktů, vždy bezpečné) ===

    /// <summary>Nastaví access modifier (A1,A2,A6).</summary>
    public ClassElement WithAccess(AccessModifier access)
    {
        AccessModifier = access;
        return this;
    }

    /// <summary>Nastaví namespace elementu.</summary>
    public new ClassElement WithNamespace(string? ns)
    {
        Namespace = ns;
        return this;
    }

    /// <summary>Nastaví XML documentation summary.</summary>
    public new ClassElement WithXmlSummary(string? summary)
    {
        XmlSummary = summary;
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
}
