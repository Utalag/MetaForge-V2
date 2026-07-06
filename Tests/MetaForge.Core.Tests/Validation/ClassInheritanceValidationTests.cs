using FluentAssertions;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Tests.Validation;

/// <summary>
/// Unit testy pro CoreValidator — Class dědičnost (❌ I5 matice).
/// </summary>
public class ClassInheritanceValidationTests
{
    [Fact]
    public void I5_InheritFromSealed_ReturnsInvalidInheritanceIssue()
    {
        var c = new ClassElement { Name = "Foo", BaseClassName = "string" };
        var issues = CoreValidator.Validate(c);

        issues.Should().ContainSingle(i => i.Code == "I5")
            .Which.Category.Should().Be(ValidationCategories.InvalidInheritance);
    }
}
