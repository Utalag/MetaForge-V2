using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements.Types;

public class InterfaceElementTests
{
    /// <summary>Kind vrací "interface".</summary>
    [Fact]
    public void Kind_Always_ReturnsInterface()
    {
        var iface = new InterfaceElement();
        iface.Kind.Should().Be("interface");
    }

    /// <summary>TotalCoin agreguje Properties a Methods.</summary>
    [Fact]
    public void TotalCoin_WithPropertiesAndMethods_Aggregates()
    {
        var iface = new InterfaceElement { Coin = 2 };
        iface.Properties.Add(new PropertyElement { Coin = 3 });
        iface.Methods.Add(new MethodElement { Coin = 4 });

        iface.TotalCoin.Should().Be(9);
    }

    /// <summary>Výchozí AccessModifier je Public.</summary>
    [Fact]
    public void AccessModifier_Default_IsPublic()
    {
        var iface = new InterfaceElement();
        iface.AccessModifier.Should().Be(AccessModifier.Public);
    }

    /// <summary>Properties je prázdný seznam.</summary>
    [Fact]
    public void Properties_Default_IsEmpty()
    {
        var iface = new InterfaceElement();
        iface.Properties.Should().BeEmpty();
    }

    /// <summary>Methods je prázdný seznam.</summary>
    [Fact]
    public void Methods_Default_IsEmpty()
    {
        var iface = new InterfaceElement();
        iface.Methods.Should().BeEmpty();
    }
}
