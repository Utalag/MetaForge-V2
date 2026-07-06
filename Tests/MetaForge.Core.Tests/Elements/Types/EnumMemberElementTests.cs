using FluentAssertions;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements.Types;

public class EnumMemberElementTests
{
    /// <summary>Výchozí Name je prázdný string.</summary>
    [Fact]
    public void Name_Default_IsEmptyString()
    {
        var member = new EnumMemberElement();
        member.Name.Should().Be(string.Empty);
    }

    /// <summary>Value je null.</summary>
    [Fact]
    public void Value_Default_IsNull()
    {
        var member = new EnumMemberElement();
        member.Value.Should().BeNull();
    }

    /// <summary>Výchozí Coin je 1.</summary>
    [Fact]
    public void Coin_Default_IsOne()
    {
        var member = new EnumMemberElement();
        member.Coin.Should().Be(1);
    }

    /// <summary>Attributes je prázdný seznam.</summary>
    [Fact]
    public void Attributes_Default_IsEmpty()
    {
        var member = new EnumMemberElement();
        member.Attributes.Should().BeEmpty();
    }
}
