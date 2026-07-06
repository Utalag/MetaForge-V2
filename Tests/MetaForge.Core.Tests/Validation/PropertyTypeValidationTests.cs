using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Tests.Validation;

/// <summary>
/// Unit testy pro CoreValidator — Property TypeModel (❌ T19,T20,T21 matice).
/// </summary>
public class PropertyTypeValidationTests
{
    [Fact]
    public void T19_VoidAsPropertyType_ReturnsInvalidTypeIssue()
    {
        var p = new PropertyElement { Name = "Bad", Type = TypeModel.Void };
        var issues = CoreValidator.ValidateProperty(p);

        issues.Should().ContainSingle(i => i.Code == "T19")
            .Which.Category.Should().Be(ValidationCategories.InvalidType);
    }

    [Fact]
    public void T20_NullableVoidAsPropertyType_ReturnsInvalidTypeIssue()
    {
        var p = new PropertyElement { Name = "Bad", Type = TypeModel.Void.MakeNullable() };
        var issues = CoreValidator.ValidateProperty(p);

        issues.Should().ContainSingle(i => i.Code == "T20")
            .Which.Category.Should().Be(ValidationCategories.InvalidType);
    }

    [Fact]
    public void T21_VoidCollectionAsPropertyType_ReturnsInvalidTypeIssue()
    {
        var p = new PropertyElement { Name = "Bad", Type = TypeModel.Void.MakeCollection() };
        var issues = CoreValidator.ValidateProperty(p);

        issues.Should().ContainSingle(i => i.Code == "T21")
            .Which.Category.Should().Be(ValidationCategories.InvalidType);
    }
}
