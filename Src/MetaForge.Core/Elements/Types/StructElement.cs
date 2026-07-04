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

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);
}
