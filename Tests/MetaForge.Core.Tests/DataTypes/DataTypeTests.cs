using FluentAssertions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Tests.DataTypes;

public class DataTypeTests
{
    [Fact]
    public void Enum_Has36Members()
    {
        var values = Enum.GetValues<DataType>();
        values.Should().HaveCount(36);
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
