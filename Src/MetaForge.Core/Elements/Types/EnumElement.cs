using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje C# enum.
/// </summary>
public sealed class EnumElement : RootElement
{
    public override string Kind => "enum";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    /// <summary>Podkladový typ enumu (výchozí Int32).</summary>
    public DataType UnderlyingType { get; set; } = DataType.Int32;

    /// <summary>Má atribut [Flags]?</summary>
    public bool IsFlags { get; set; }

    /// <summary>Členové enumu.</summary>
    public List<EnumMemberElement> Members { get; } = new();

    public override int TotalCoin => Coin + Members.Sum(m => m.Coin);
}
