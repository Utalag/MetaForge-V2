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
}
