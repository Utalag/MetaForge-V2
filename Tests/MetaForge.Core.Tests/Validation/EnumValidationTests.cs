using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Tests.Validation;

/// <summary>
/// Unit testy pro CoreValidator — Enum validace (❌ E5,E6 matice).
/// </summary>
public class EnumValidationTests
{
    [Fact]
    public void E5_StringUnderlying_ReturnsInvalidTypeIssue()
    {
        var e = new EnumElement { Name = "Bad", UnderlyingType = DataType.String };
        var issues = CoreValidator.Validate(e);

        issues.Should().ContainSingle(i => i.Code == "E5")
            .Which.Category.Should().Be(ValidationCategories.InvalidType);
    }

    [Fact]
    public void E6_BoolUnderlying_ReturnsInvalidTypeIssue()
    {
        var e = new EnumElement { Name = "Bad", UnderlyingType = DataType.Bool };
        var issues = CoreValidator.Validate(e);

        issues.Should().ContainSingle(i => i.Code == "E6")
            .Which.Category.Should().Be(ValidationCategories.InvalidType);
    }
}
