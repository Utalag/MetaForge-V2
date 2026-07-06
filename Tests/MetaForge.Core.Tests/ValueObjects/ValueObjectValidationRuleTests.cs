using FluentAssertions;
using MetaForge.Core.ValueObjects;

namespace MetaForge.Core.Tests.ValueObjects;

public class ValueObjectValidationRuleTests
{
    /// <summary>Konstruktor nastaví RuleName.</summary>
    [Fact]
    public void Constructor_SetsRuleName()
    {
        var rule = new ValueObjectValidationRule("not_empty");
        rule.RuleName.Should().Be("not_empty");
    }

    /// <summary>Parameter může být null.</summary>
    [Fact]
    public void Constructor_NullParameter_Allowed()
    {
        var rule = new ValueObjectValidationRule("max_length", Parameter: null);
        rule.Parameter.Should().BeNull();
    }

    /// <summary>ErrorMessage může být null.</summary>
    [Fact]
    public void Constructor_NullErrorMessage_Allowed()
    {
        var rule = new ValueObjectValidationRule("email_format", ErrorMessage: null);
        rule.ErrorMessage.Should().BeNull();
    }

    /// <summary>Konstruktor se všemi parametry.</summary>
    [Fact]
    public void Constructor_WithAllParameters_SetsProperties()
    {
        var rule = new ValueObjectValidationRule("max_length", "4000", "Maximální délka je 4000 znaků");
        rule.RuleName.Should().Be("max_length");
        rule.Parameter.Should().Be("4000");
        rule.ErrorMessage.Should().Be("Maximální délka je 4000 znaků");
    }
}
