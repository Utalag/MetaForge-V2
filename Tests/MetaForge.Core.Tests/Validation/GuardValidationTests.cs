// ---------------------------------------------------------------------------
// MetaForge.Core.Tests — PROP-042: Guard Validation Tests
// Tests for new CoreValidator guard checks (G-01 to G-12)
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Tests.Validation;

public class GuardValidationTests
{
    // === MethodElement guards ===

    [Fact]
    public void G_M13_AbstractAsync_ReturnsIssue()
    {
        var m = new MethodElement { Name = "Test", IsAbstract = true, IsAsync = true };
        var issues = CoreValidator.ValidateMethod(m);
        Assert.Contains(issues, i => i.Code == "M13");
    }

    [Fact]
    public void G_M14_ExtensionNotStatic_ReturnsWarning()
    {
        var m = new MethodElement { Name = "Test", IsExtension = true, IsStatic = false };
        var issues = CoreValidator.ValidateMethod(m);
        Assert.Contains(issues, i => i.Code == "M14");
    }

    [Fact]
    public void G_M14_ExtensionWithStatic_NoIssue()
    {
        var m = new MethodElement { Name = "Test", IsExtension = true, IsStatic = true };
        var issues = CoreValidator.ValidateMethod(m);
        Assert.DoesNotContain(issues, i => i.Code == "M14");
    }

    // === ClassElement guards ===

    [Fact]
    public void G_C13_AbstractSealedStatic_ReturnsIssue()
    {
        var c = new ClassElement { Name = "Bad", IsAbstract = true, IsSealed = true, IsStatic = true };
        var issues = CoreValidator.Validate(c);
        Assert.Contains(issues, i => i.Code == "C13");
    }

    [Fact]
    public void G_C14_AbstractSealedRecord_ReturnsIssue()
    {
        var c = new ClassElement { Name = "Bad", IsAbstract = true, IsSealed = true, IsRecord = true };
        var issues = CoreValidator.Validate(c);
        Assert.Contains(issues, i => i.Code == "C14");
    }

    [Fact]
    public void G11_PartialRecord_ReturnsWarning()
    {
        var c = new ClassElement { Name = "PartialDto", IsPartial = true, IsRecord = true };
        var issues = CoreValidator.Validate(c);
        Assert.Contains(issues, i => i.Code == "G11");
    }

    [Fact]
    public void G12_StaticPartial_ReturnsWarning()
    {
        var c = new ClassElement { Name = "PartialUtils", IsStatic = true, IsPartial = true };
        var issues = CoreValidator.Validate(c);
        Assert.Contains(issues, i => i.Code == "G12");
    }

    // === PropertyElement guards ===

    [Fact]
    public void G_P9_StaticRequired_ReturnsIssue()
    {
        var p = new PropertyElement { Name = "Bad", IsStatic = true, IsRequired = true };
        var issues = CoreValidator.ValidateProperty(p);
        Assert.Contains(issues, i => i.Code == "P9");
    }

    [Fact]
    public void G_P10_InitOnlyPrivateSet_ReturnsWarning()
    {
        var p = new PropertyElement
        {
            Name = "Suspicious",
            IsInitOnly = true,
            HasSetter = true,
            AccessModifier = AccessModifier.Private
        };
        var issues = CoreValidator.ValidateProperty(p);
        Assert.Contains(issues, i => i.Code == "P10");
    }

    [Fact]
    public void G_P10_InitOnlyWithoutPrivateSet_NoIssue()
    {
        var p = new PropertyElement { Name = "Good", IsInitOnly = true, HasSetter = true, AccessModifier = AccessModifier.Public };
        var issues = CoreValidator.ValidateProperty(p);
        Assert.DoesNotContain(issues, i => i.Code == "P10");
    }

    // === Struct validation ===

    [Fact]
    public void G_Struct_Validate_NoIssues_ForBasicStruct()
    {
        var s = StructElement.Basic("Point");
        var issues = CoreValidator.Validate(s);
        Assert.Empty(issues);
    }

    // === ValidateProperty never throws ===

    [Fact]
    public void ValidateProperty_NeverThrows()
    {
        var p = new PropertyElement { Name = "Test", Type = TypeModel.Void }; // Invalid type
        var issues = CoreValidator.ValidateProperty(p);
        Assert.NotNull(issues);
        Assert.NotEmpty(issues); // Should report issues, not throw
    }

    // === ValidateMethod never throws ===

    [Fact]
    public void ValidateMethod_NeverThrows()
    {
        var m = new MethodElement { Name = "Test", IsAbstract = true, IsStatic = true }; // Invalid combo
        var issues = CoreValidator.ValidateMethod(m);
        Assert.NotNull(issues);
        Assert.NotEmpty(issues);
    }
}
