using FluentAssertions;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Tests.Validation;

/// <summary>
/// Unit testy pro CoreValidator — Class modifikátory (❌ řádky C9,C10,C12 matice).
/// </summary>
public class ClassModifierValidationTests
{
    [Fact]
    public void C9_AbstractSealed_ReturnsConflictingModifiersIssue()
    {
        var c = new ClassElement { Name = "Foo", IsAbstract = true, IsSealed = true };
        var issues = CoreValidator.Validate(c);

        issues.Should().ContainSingle(i => i.Code == "C9")
            .Which.Category.Should().Be(ValidationCategories.ConflictingModifiers);
    }

    [Fact]
    public void C10_AbstractStatic_ReturnsConflictingModifiersIssue()
    {
        var c = new ClassElement { Name = "Foo", IsAbstract = true, IsStatic = true };
        var issues = CoreValidator.Validate(c);

        issues.Should().ContainSingle(i => i.Code == "C10")
            .Which.Category.Should().Be(ValidationCategories.ConflictingModifiers);
    }

    [Fact]
    public void C12_StaticRecord_ReturnsConflictingModifiersIssue()
    {
        var c = new ClassElement { Name = "Foo", IsStatic = true, IsRecord = true };
        var issues = CoreValidator.Validate(c);

        issues.Should().ContainSingle(i => i.Code == "C12")
            .Which.Category.Should().Be(ValidationCategories.ConflictingModifiers);
    }
}
