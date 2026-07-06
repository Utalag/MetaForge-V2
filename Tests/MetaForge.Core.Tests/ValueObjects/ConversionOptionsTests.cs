using FluentAssertions;
using MetaForge.Core.ValueObjects;

namespace MetaForge.Core.Tests.ValueObjects;

public class ConversionOptionsTests
{
    /// <summary>Všechny výchozí volby jsou true.</summary>
    [Fact]
    public void Default_AllOptionsAreTrue()
    {
        var options = new ConversionOptions();
        options.GenerateImplicitConversion.Should().BeFalse();
        options.GenerateExplicitConversion.Should().BeFalse();
        options.GenerateToString.Should().BeTrue();
        options.GenerateEquals.Should().BeTrue();
        options.GenerateGetHashCode.Should().BeTrue();
    }

    /// <summary>Konstruktor s hodnotami nastaví vlastnosti.</summary>
    [Fact]
    public void Constructor_WithValues_SetsProperties()
    {
        var options = new ConversionOptions(
            GenerateImplicitConversion: true,
            GenerateExplicitConversion: true,
            GenerateToString: false,
            GenerateEquals: false,
            GenerateGetHashCode: false);

        options.GenerateImplicitConversion.Should().BeTrue();
        options.GenerateExplicitConversion.Should().BeTrue();
        options.GenerateToString.Should().BeFalse();
        options.GenerateEquals.Should().BeFalse();
        options.GenerateGetHashCode.Should().BeFalse();
    }
}
