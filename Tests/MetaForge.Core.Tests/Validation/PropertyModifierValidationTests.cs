using FluentAssertions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Tests.Validation;

/// <summary>
/// Unit testy pro CoreValidator — Property modifikátory (❌ P7 matice).
/// </summary>
public class PropertyModifierValidationTests
{
    [Fact]
    public void P7_NoGetterNoSetter_ReturnsMissingRequiredIssue()
    {
        var p = new PropertyElement { Name = "Bad", HasGetter = false, HasSetter = false };
        var issues = CoreValidator.ValidateProperty(p);

        issues.Should().ContainSingle(i => i.Code == "P7")
            .Which.Category.Should().Be(ValidationCategories.MissingRequired);
    }
}
