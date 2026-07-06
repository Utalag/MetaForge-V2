using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Tests.Elements.Members;

public class PropertyElementTests
{
    /// <summary>Výchozí Type je TypeModel.Object.</summary>
    [Fact]
    public void Type_Default_IsObject()
    {
        var prop = new PropertyElement();
        prop.Type.BaseType.Should().Be(DataType.Object);
    }

    /// <summary>HasGetter je true.</summary>
    [Fact]
    public void HasGetter_Default_IsTrue()
    {
        var prop = new PropertyElement();
        prop.HasGetter.Should().BeTrue();
    }

    /// <summary>HasSetter je true.</summary>
    [Fact]
    public void HasSetter_Default_IsTrue()
    {
        var prop = new PropertyElement();
        prop.HasSetter.Should().BeTrue();
    }

    /// <summary>IsInitOnly je false.</summary>
    [Fact]
    public void IsInitOnly_Default_IsFalse()
    {
        var prop = new PropertyElement();
        prop.IsInitOnly.Should().BeFalse();
    }

    /// <summary>IsRequired je false.</summary>
    [Fact]
    public void IsRequired_Default_IsFalse()
    {
        var prop = new PropertyElement();
        prop.IsRequired.Should().BeFalse();
    }

    /// <summary>IsStatic je false.</summary>
    [Fact]
    public void IsStatic_Default_IsFalse()
    {
        var prop = new PropertyElement();
        prop.IsStatic.Should().BeFalse();
    }

    /// <summary>DefaultValue je null.</summary>
    [Fact]
    public void DefaultValue_Default_IsNull()
    {
        var prop = new PropertyElement();
        prop.DefaultValue.Should().BeNull();
    }

    /// <summary>Výchozí Coin je 2.</summary>
    [Fact]
    public void Coin_Default_IsTwo()
    {
        var prop = new PropertyElement();
        prop.Coin.Should().Be(2);
    }

    /// <summary>Výchozí AccessModifier je Public.</summary>
    [Fact]
    public void AccessModifier_Default_IsPublic()
    {
        var prop = new PropertyElement();
        prop.AccessModifier.Should().Be(AccessModifier.Public);
    }
}
