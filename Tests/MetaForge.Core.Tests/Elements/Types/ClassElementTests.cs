using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements.Types;

public class ClassElementTests
{
    /// <summary>Kind vrací "class".</summary>
    [Fact]
    public void Kind_Always_ReturnsClass()
    {
        var cls = new ClassElement();
        cls.Kind.Should().Be("class");
    }

    /// <summary>TotalCoin = Coin při prázdných Properties/Methods.</summary>
    [Fact]
    public void TotalCoin_NoPropertiesNoMethods_ReturnsCoin()
    {
        var cls = new ClassElement { Coin = 10 };
        cls.TotalCoin.Should().Be(10);
    }

    /// <summary>TotalCoin zahrnuje Coin všech Properties.</summary>
    [Fact]
    public void TotalCoin_WithProperties_IncludesSum()
    {
        var cls = new ClassElement { Coin = 5 };
        cls.Properties.Add(new PropertyElement { Coin = 3 });
        cls.Properties.Add(new PropertyElement { Coin = 2 });

        cls.TotalCoin.Should().Be(10);
    }

    /// <summary>TotalCoin zahrnuje TotalCoin všech Methods.</summary>
    [Fact]
    public void TotalCoin_WithMethods_IncludesTotalCoinOfMethods()
    {
        var cls = new ClassElement { Coin = 5 };
        var method = new MethodElement { Coin = 3 };
        method.Parameters.Add(new ParameterElement { Coin = 1 });
        cls.Methods.Add(method);

        cls.TotalCoin.Should().Be(9); // 5 + (3 + 1)
    }

    /// <summary>TotalCoin = Coin + sum(Properties.Coin) + sum(Methods.TotalCoin).</summary>
    [Fact]
    public void TotalCoin_WithPropertiesAndMethods_AggregatesBoth()
    {
        var cls = new ClassElement { Coin = 3 };
        cls.Properties.Add(new PropertyElement { Coin = 4 });
        cls.Properties.Add(new PropertyElement { Coin = 2 });
        cls.Methods.Add(new MethodElement { Coin = 5 });
        cls.Methods.Add(new MethodElement { Coin = 1 });

        cls.TotalCoin.Should().Be(15); // 3 + (4+2) + (5+1)
    }

    /// <summary>IsAbstract, IsSealed, IsStatic, IsPartial, IsRecord jsou false.</summary>
    [Fact]
    public void Flags_Defaults_AreFalse()
    {
        var cls = new ClassElement();
        cls.IsAbstract.Should().BeFalse();
        cls.IsSealed.Should().BeFalse();
        cls.IsStatic.Should().BeFalse();
        cls.IsPartial.Should().BeFalse();
        cls.IsRecord.Should().BeFalse();
    }

    /// <summary>Výchozí AccessModifier je Public.</summary>
    [Fact]
    public void AccessModifier_Default_IsPublic()
    {
        var cls = new ClassElement();
        cls.AccessModifier.Should().Be(AccessModifier.Public);
    }

    /// <summary>BaseClassName je null.</summary>
    [Fact]
    public void BaseClassName_Default_IsNull()
    {
        var cls = new ClassElement();
        cls.BaseClassName.Should().BeNull();
    }

    /// <summary>ImplementedInterfaces je prázdný.</summary>
    [Fact]
    public void ImplementedInterfaces_Default_IsEmpty()
    {
        var cls = new ClassElement();
        cls.ImplementedInterfaces.Should().BeEmpty();
    }
}
