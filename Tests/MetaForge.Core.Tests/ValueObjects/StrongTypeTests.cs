using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.ValueObjects;

namespace MetaForge.Core.Tests.ValueObjects;

public class StrongTypeTests
{
    /// <summary>Konstruktor nastaví Name a Underlying.</summary>
    [Fact]
    public void Constructor_SetsNameAndUnderlying()
    {
        var st = new StrongType("Email", TypeModel.String);
        st.Name.Should().Be("Email");
        st.Underlying.BaseType.Should().Be(DataType.String);
    }

    /// <summary>ValidationRules může být null.</summary>
    [Fact]
    public void Constructor_NullValidationRules_Allowed()
    {
        var st = new StrongType("Email", TypeModel.String);
        st.ValidationRules.Should().BeNull();
    }
}
