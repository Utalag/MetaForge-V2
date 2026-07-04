using FluentAssertions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Tests.DataTypes;

public class DataTypeTests
{
    [Fact]
    public void Enum_HasExpectedCoreValues()
    {
        // Ověřuje přítomnost klíčových hodnot, ne přesný počet
        var values = Enum.GetValues<DataType>();
        values.Should().Contain(DataType.Bool);
        values.Should().Contain(DataType.String);
        values.Should().Contain(DataType.Int32);
        values.Should().Contain(DataType.Decimal);
        values.Should().Contain(DataType.Guid);
        values.Should().Contain(DataType.DateTime);
        values.Should().Contain(DataType.Void);
        values.Should().Contain(DataType.Entity);
        values.Should().Contain(DataType.Object);
    }

    [Fact]
    public void AllValues_AreUnique()
    {
        var values = Enum.GetValues<DataType>();
        values.Distinct().Should().HaveCount(values.Length);
    }

    [Fact]
    public void Bool_IsZero()
    {
        ((int)DataType.Bool).Should().Be(0);
    }

    [Fact]
    public void String_Exists()
    {
        Enum.IsDefined(typeof(DataType), DataType.String).Should().BeTrue();
    }
}
