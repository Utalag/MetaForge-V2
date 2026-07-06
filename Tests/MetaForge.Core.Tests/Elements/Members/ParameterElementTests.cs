using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Tests.Elements.Members;

public class ParameterElementTests
{
    /// <summary>Výchozí Type je TypeModel.Object.</summary>
    [Fact]
    public void Type_Default_IsObject()
    {
        var param = new ParameterElement();
        param.Type.BaseType.Should().Be(DataType.Object);
    }

    /// <summary>HasDefaultValue je false.</summary>
    [Fact]
    public void HasDefaultValue_Default_IsFalse()
    {
        var param = new ParameterElement();
        param.HasDefaultValue.Should().BeFalse();
    }

    /// <summary>DefaultValue je null.</summary>
    [Fact]
    public void DefaultValue_Default_IsNull()
    {
        var param = new ParameterElement();
        param.DefaultValue.Should().BeNull();
    }

    /// <summary>Výchozí Modifier je None.</summary>
    [Fact]
    public void Modifier_Default_IsNone()
    {
        var param = new ParameterElement();
        param.Modifier.Should().Be(ParameterModifier.None);
    }

    /// <summary>Výchozí Coin je 1.</summary>
    [Fact]
    public void Coin_Default_IsOne()
    {
        var param = new ParameterElement();
        param.Coin.Should().Be(1);
    }

    /// <summary>Enum ParameterModifier obsahuje všechny členy.</summary>
    [Fact]
    public void Modifier_Enum_HasAllMembers()
    {
        var values = Enum.GetValues<ParameterModifier>();
        values.Should().Contain(ParameterModifier.None);
        values.Should().Contain(ParameterModifier.Ref);
        values.Should().Contain(ParameterModifier.Out);
        values.Should().Contain(ParameterModifier.In);
        values.Should().Contain(ParameterModifier.Params);
    }
}
