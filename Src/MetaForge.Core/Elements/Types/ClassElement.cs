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

    /// <summary>Vlastnosti (property) třídy.</summary>
    public List<PropertyElement> Properties { get; } = new();

    /// <summary>Metody třídy.</summary>
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);
}
