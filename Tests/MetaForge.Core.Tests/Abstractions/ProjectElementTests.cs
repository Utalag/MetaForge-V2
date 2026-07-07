using FluentAssertions;
using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Tests.Abstractions;

public class ProjectElementTests
{
    /// <summary>Výchozí Name je prázdný string.</summary>
    [Fact]
    public void Name_Default_IsEmptyString()
    {
        var project = new ProjectElement();
        project.Name.Should().Be(string.Empty);
    }

    /// <summary>Výchozí DefaultNamespace je null.</summary>
    [Fact]
    public void DefaultNamespace_Default_IsNull()
    {
        var project = new ProjectElement();
        project.DefaultNamespace.Should().BeNull();
    }

    /// <summary>RootElements je prázdný seznam.</summary>
    [Fact]
    public void RootElements_Default_IsEmpty()
    {
        var project = new ProjectElement();
        project.RootElements.Should().BeEmpty();
    }

    /// <summary>Výchozí TargetFramework je null.</summary>
    [Fact]
    public void TargetFramework_Default_IsNull()
    {
        var project = new ProjectElement();
        project.TargetFramework.Should().BeNull();
    }

    /// <summary>Výchozí Nullable a ImplicitUsings jsou zapnuté (moderní .NET default).</summary>
    [Fact]
    public void NullableAndImplicitUsings_Default_AreEnabled()
    {
        var project = new ProjectElement();
        project.NullableEnabled.Should().BeTrue();
        project.ImplicitUsingsEnabled.Should().BeTrue();
    }

    /// <summary>WithPackageReference přidá referenci a vrátí this (fluent).</summary>
    [Fact]
    public void WithPackageReference_AddsToList()
    {
        var project = new ProjectElement()
            .WithPackageReference("Newtonsoft.Json", "13.0.3");

        project.PackageReferences.Should().ContainSingle();
        project.PackageReferences[0].Name.Should().Be("Newtonsoft.Json");
        project.PackageReferences[0].Version.Should().Be("13.0.3");
    }

    /// <summary>WithAnalyzerReference přidá referenci a vrátí this (fluent).</summary>
    [Fact]
    public void WithAnalyzerReference_AddsToList()
    {
        var project = new ProjectElement()
            .WithAnalyzerReference("StyleCop.Analyzers", "1.2.0");

        project.AnalyzerReferences.Should().ContainSingle();
    }

    /// <summary>WithProjectReference přidá referenci a vrátí this (fluent).</summary>
    [Fact]
    public void WithProjectReference_AddsToList()
    {
        var project = new ProjectElement()
            .WithProjectReference("MetaForge.Core");

        project.ProjectReferences.Should().ContainSingle();
        project.ProjectReferences[0].ProjectName.Should().Be("MetaForge.Core");
    }

    /// <summary>Fluent metody lze řetězit.</summary>
    [Fact]
    public void FluentMethods_CanBeChained()
    {
        var project = new ProjectElement { Name = "MetaForge.Api" }
            .WithPackageReference("Microsoft.AspNetCore.OpenApi", "9.0.0")
            .WithProjectReference("MetaForge.Core")
            .WithAnalyzerReference("Microsoft.CodeAnalysis.NetAnalyzers", "9.0.0");

        project.PackageReferences.Should().HaveCount(1);
        project.ProjectReferences.Should().HaveCount(1);
        project.AnalyzerReferences.Should().HaveCount(1);
    }
}
