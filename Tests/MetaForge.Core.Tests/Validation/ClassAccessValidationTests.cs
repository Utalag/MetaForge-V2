using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Tests.Validation;

/// <summary>
/// Unit testy pro CoreValidator — Class access modifiers (❌ A3,A4,A5 matice).
/// </summary>
public class ClassAccessValidationTests
{
    [Fact]
    public void A3_PrivateTopLevel_ReturnsInvalidAccessIssue()
    {
        var c = new ClassElement { Name = "Foo", AccessModifier = AccessModifier.Private };
        var issues = CoreValidator.Validate(c);

        issues.Should().ContainSingle(i => i.Code == "A3")
            .Which.Category.Should().Be(ValidationCategories.InvalidAccess);
    }

    [Fact]
    public void A4_ProtectedTopLevel_ReturnsInvalidAccessIssue()
    {
        var c = new ClassElement { Name = "Foo", AccessModifier = AccessModifier.Protected };
        var issues = CoreValidator.Validate(c);

        issues.Should().ContainSingle(i => i.Code == "A4")
            .Which.Category.Should().Be(ValidationCategories.InvalidAccess);
    }

    [Fact]
    public void A5_PrivateProtectedTopLevel_ReturnsInvalidAccessIssue()
    {
        var c = new ClassElement { Name = "Foo", AccessModifier = AccessModifier.PrivateProtected };
        var issues = CoreValidator.Validate(c);

        issues.Should().ContainSingle(i => i.Code == "A5")
            .Which.Category.Should().Be(ValidationCategories.InvalidAccess);
    }
}
