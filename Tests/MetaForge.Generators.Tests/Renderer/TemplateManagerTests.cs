using FluentAssertions;
using MetaForge.Generators;

namespace MetaForge.Generators.Tests.Renderer;

/// <summary>
/// Unit testy pro TemplateManager — Scriban šablony.
/// PROP-048 — Generator Render Core Tests.
/// </summary>
public class TemplateManagerTests : IDisposable
{
    private readonly string _testTemplateDir;

    public TemplateManagerTests()
    {
        _testTemplateDir = Path.Combine(Path.GetTempPath(), "MetaForge_TemplateTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testTemplateDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testTemplateDir))
            Directory.Delete(_testTemplateDir, true);
    }

    [Fact]
    public void LoadTemplate_Existing_ReturnsTemplate()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testTemplateDir, "Test.scriban"), "Hello {{ name }}!");
        var manager = new TemplateManager(_testTemplateDir);

        // Act
        var template = manager.LoadTemplate("Test");

        // Assert
        template.Should().NotBeNull();
        var result = template.Render(new { name = "World" });
        result.Should().Be("Hello World!");
    }

    [Fact]
    public void LoadTemplate_Missing_ThrowsFileNotFound()
    {
        // Arrange
        var manager = new TemplateManager(_testTemplateDir);

        // Act
        var act = () => manager.LoadTemplate("NonExistent");

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*NonExistent*");
    }

    [Fact]
    public void LoadTemplate_CacheHit_ReturnsSameInstance()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testTemplateDir, "Test.scriban"), "content");
        var manager = new TemplateManager(_testTemplateDir);

        // Act
        var t1 = manager.LoadTemplate("Test");
        var t2 = manager.LoadTemplate("Test");

        // Assert
        t1.Should().BeSameAs(t2);
    }

    [Fact]
    public void LoadTemplate_ScribanError_ThrowsInvalidOperation()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testTemplateDir, "Bad.scriban"), "{{ invalid syntax }");
        var manager = new TemplateManager(_testTemplateDir);

        // Act
        var act = () => manager.LoadTemplate("Bad");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Chyby*parsování*");
    }

    [Fact]
    public void Render_ValidModel_ProducesOutput()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testTemplateDir, "Greet.scriban"),
            "{{ prefix }} {{ name }}!");
        var manager = new TemplateManager(_testTemplateDir);
        var template = manager.LoadTemplate("Greet");

        // Act
        var result = template.Render(new { prefix = "Hello", name = "World" });

        // Assert
        result.Should().Be("Hello World!");
    }

    [Fact]
    public void Render_WithMissingVariable_DoesNotThrow()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testTemplateDir, "Missing.scriban"),
            "{{ name }} {{ missing }}");
        var manager = new TemplateManager(_testTemplateDir);
        var template = manager.LoadTemplate("Missing");

        // Act
        var act = () => template.Render(new { name = "Test" });

        // Assert
        act.Should().NotThrow();
    }
}
