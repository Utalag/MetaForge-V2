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
}
