using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Diagnostics;
using MetaForge.Core.Transforms;

namespace MetaForge.Core.Tests.Diagnostics;

public class DiagnosticBagTests
{
    [Fact]
    public void Bag_Empty_HasNoErrors()
    {
        var bag = new DiagnosticBag();
        bag.HasErrors.Should().BeFalse();
        bag.ErrorCount.Should().Be(0);
        bag.Count.Should().Be(0);
    }

    [Fact]
    public void Bag_WithError_HasErrors()
    {
        var bag = new DiagnosticBag();
        bag.Report(new Diagnostic("MF-001", "test error", DiagnosticSeverity.Error,
            new ElementPath("Model", "Class")));

        bag.HasErrors.Should().BeTrue();
        bag.ErrorCount.Should().Be(1);
    }

    [Fact]
    public void Bag_Warnings_AreNotErrors()
    {
        var bag = new DiagnosticBag();
        bag.Report(new Diagnostic("MF-002", "test warning", DiagnosticSeverity.Warning,
            new ElementPath("Model", "Property")));

        bag.HasErrors.Should().BeFalse();
        bag.WarningCount.Should().Be(1);
    }

    [Fact]
    public void BuildResult_Success_Then_Chains()
    {
        var r1 = new BuildResult<int>(42);
        var r2 = r1.Then(x => new BuildResult<string>($"Value: {x}"));

        r2.IsSuccess.Should().BeTrue();
        r2.Value.Should().Be("Value: 42");
    }

    [Fact]
    public void BuildResult_Error_Then_Stops()
    {
        var bag = new DiagnosticBag();
        bag.Report(new Diagnostic("MF-ERR", "fail", DiagnosticSeverity.Error,
            new ElementPath("X", "Y")));

        var r1 = new BuildResult<int>(0, bag);
        var r2 = r1.Then(x => new BuildResult<string>($"Value: {x}"));

        r2.IsSuccess.Should().BeFalse();
        r2.Bag.HasErrors.Should().BeTrue();
        r2.Value.Should().BeNull();
    }

    [Fact]
    public void AttributeReflection_MapsRequiredToValidationKey()
    {
        var key = AttributeReflection.MapAttributeToMetadataKey("RequiredAttribute");
        key.Should().Be(MetadataBag.Keys.ValidationRequired);
    }

    [Fact]
    public void AttributeReflection_UnknownAttribute_ReturnsNull()
    {
        var key = AttributeReflection.MapAttributeToMetadataKey("UnknownAttribute");
        key.Should().BeNull();
    }
}
