using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Tests.Elements.Members;

public class MethodElementTests
{
    /// <summary>Výchozí ReturnType je TypeModel.Void.</summary>
    [Fact]
    public void ReturnType_Default_IsVoid()
    {
        var method = new MethodElement();
        method.ReturnType.BaseType.Should().Be(DataType.Void);
    }

    /// <summary>TotalCoin = Coin při prázdných Parameters.</summary>
    [Fact]
    public void TotalCoin_NoParameters_ReturnsCoin()
    {
        var method = new MethodElement { Coin = 10 };
        method.TotalCoin.Should().Be(10);
    }

    /// <summary>TotalCoin zahrnuje Coin všech Parameters.</summary>
    [Fact]
    public void TotalCoin_WithParameters_IncludesSum()
    {
        var method = new MethodElement { Coin = 5 };
        method.Parameters.Add(new ParameterElement { Coin = 2 });
        method.Parameters.Add(new ParameterElement { Coin = 3 });

        method.TotalCoin.Should().Be(10);
    }

    /// <summary>IsStatic je false.</summary>
    [Fact]
    public void IsStatic_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsStatic.Should().BeFalse();
    }

    /// <summary>IsAsync je false.</summary>
    [Fact]
    public void IsAsync_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsAsync.Should().BeFalse();
    }

    /// <summary>IsAbstract je false.</summary>
    [Fact]
    public void IsAbstract_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsAbstract.Should().BeFalse();
    }

    /// <summary>IsVirtual je false.</summary>
    [Fact]
    public void IsVirtual_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsVirtual.Should().BeFalse();
    }

    /// <summary>IsOverride je false.</summary>
    [Fact]
    public void IsOverride_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsOverride.Should().BeFalse();
    }

    /// <summary>Body je null.</summary>
    [Fact]
    public void Body_Default_IsNull()
    {
        var method = new MethodElement();
        method.Body.Should().BeNull();
    }

    /// <summary>Výchozí Coin je 5.</summary>
    [Fact]
    public void Coin_Default_IsFive()
    {
        var method = new MethodElement();
        method.Coin.Should().Be(5);
    }

    /// <summary>Parameters je prázdný seznam.</summary>
    [Fact]
    public void Parameters_Default_IsEmpty()
    {
        var method = new MethodElement();
        method.Parameters.Should().BeEmpty();
    }

    /// <summary>Attributes je prázdný seznam.</summary>
    [Fact]
    public void Attributes_Default_IsEmpty()
    {
        var method = new MethodElement();
        method.Attributes.Should().BeEmpty();
    }
}
