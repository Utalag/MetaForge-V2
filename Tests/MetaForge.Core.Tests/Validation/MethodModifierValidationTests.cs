using FluentAssertions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Tests.Validation;

/// <summary>
/// Unit testy pro CoreValidator — Method modifikátory (❌ M9-M12 matice).
/// </summary>
public class MethodModifierValidationTests
{
    [Fact]
    public void M9_AbstractVirtual_ReturnsConflictingModifiersIssue()
    {
        var m = new MethodElement { Name = "Bad", IsAbstract = true, IsVirtual = true };
        var issues = CoreValidator.ValidateMethod(m);

        issues.Should().ContainSingle(i => i.Code == "M9")
            .Which.Category.Should().Be(ValidationCategories.ConflictingModifiers);
    }

    [Fact]
    public void M10_AbstractOverride_ReturnsConflictingModifiersIssue()
    {
        var m = new MethodElement { Name = "Bad", IsAbstract = true, IsOverride = true };
        var issues = CoreValidator.ValidateMethod(m);

        issues.Should().ContainSingle(i => i.Code == "M10")
            .Which.Category.Should().Be(ValidationCategories.ConflictingModifiers);
    }

    [Fact]
    public void M11_StaticAbstract_ReturnsConflictingModifiersIssue()
    {
        var m = new MethodElement { Name = "Bad", IsStatic = true, IsAbstract = true };
        var issues = CoreValidator.ValidateMethod(m);

        issues.Should().ContainSingle(i => i.Code == "M11")
            .Which.Category.Should().Be(ValidationCategories.ConflictingModifiers);
    }

    [Fact]
    public void M12_VirtualOverride_ReturnsConflictingModifiersIssue()
    {
        var m = new MethodElement { Name = "Bad", IsVirtual = true, IsOverride = true };
        var issues = CoreValidator.ValidateMethod(m);

        issues.Should().ContainSingle(i => i.Code == "M12")
            .Which.Category.Should().Be(ValidationCategories.ConflictingModifiers);
    }
}
