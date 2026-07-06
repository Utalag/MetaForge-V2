using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements.Types;

public class EnumElementTests
{
    /// <summary>Kind vrací "enum".</summary>
    [Fact]
    public void Kind_Always_ReturnsEnum()
    {
        var en = new EnumElement();
        en.Kind.Should().Be("enum");
    }

    /// <summary>UnderlyingType je Int32.</summary>
    [Fact]
    public void UnderlyingType_Default_IsInt32()
    {
        var en = new EnumElement();
        en.UnderlyingType.Should().Be(DataType.Int32);
    }

    /// <summary>IsFlags je false.</summary>
    [Fact]
    public void IsFlags_Default_IsFalse()
    {
        var en = new EnumElement();
        en.IsFlags.Should().BeFalse();
    }

    /// <summary>TotalCoin = Coin při prázdných Members.</summary>
    [Fact]
    public void TotalCoin_NoMembers_ReturnsCoin()
    {
        var en = new EnumElement { Coin = 7 };
        en.TotalCoin.Should().Be(7);
    }

    /// <summary>TotalCoin zahrnuje Coin všech Members.</summary>
    [Fact]
    public void TotalCoin_WithMembers_IncludesSum()
    {
        var en = new EnumElement { Coin = 3 };
        en.Members.Add(new EnumMemberElement { Coin = 2 });
        en.Members.Add(new EnumMemberElement { Coin = 4 });

        en.TotalCoin.Should().Be(9);
    }

    /// <summary>Výchozí AccessModifier je Public.</summary>
    [Fact]
    public void AccessModifier_Default_IsPublic()
    {
        var en = new EnumElement();
        en.AccessModifier.Should().Be(AccessModifier.Public);
    }

    /// <summary>Members je prázdný seznam.</summary>
    [Fact]
    public void Members_Default_IsEmpty()
    {
        var en = new EnumElement();
        en.Members.Should().BeEmpty();
    }
}
