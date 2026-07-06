using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje C# interface.
/// </summary>
public sealed class InterfaceElement : RootElement
{
    public override string Kind => "interface";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    public List<PropertyElement> Properties { get; } = new();
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);

    // === Statické factory metody ===

    /// <summary>public interface IFoo { }</summary>
    public static InterfaceElement Basic(string name) => new() { Name = name };

    // === Fluent rozšiřovací metody ===

    /// <summary>Nastaví access modifier.</summary>
    public InterfaceElement WithAccess(AccessModifier access)
    {
        AccessModifier = access;
        return this;
    }

    /// <summary>Přidá vlastnost do interfacu.</summary>
    public InterfaceElement WithProperty(PropertyElement property)
    {
        Properties.Add(property);
        return this;
    }

    /// <summary>Přidá metodu do interfacu.</summary>
    public InterfaceElement WithMethod(MethodElement method)
    {
        Methods.Add(method);
        return this;
    }
}
