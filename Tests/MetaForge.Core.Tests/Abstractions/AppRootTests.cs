using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Abstractions;

public class AppRootTests
{
    /// <summary>Ověří, že nový AppRoot má prázdný seznam projektů.</summary>
    [Fact]
    public void AppRoot_Default_HasEmptyProjects()
    {
        var root = new AppRoot();
        root.Projects.Should().BeEmpty();
    }

    /// <summary>TotalCoin = 0 při prázdném seznamu projektů.</summary>
    [Fact]
    public void TotalCoin_EmptyProjects_ReturnsZero()
    {
        var root = new AppRoot();
        root.TotalCoin.Should().Be(0);
    }

    /// <summary>Projekt bez RootElementů → TotalCoin = 0.</summary>
    [Fact]
    public void TotalCoin_SingleProjectNoRootElements_ReturnsZero()
    {
        var root = new AppRoot();
        root.Projects.Add(new ProjectElement { Name = "Empty" });
        root.TotalCoin.Should().Be(0);
    }

    /// <summary>Jeden projekt + jedna třída → TotalCoin = Coin třídy.</summary>
    [Fact]
    public void TotalCoin_SingleProjectWithClass_ReturnsClassTotalCoin()
    {
        var root = new AppRoot();
        var project = new ProjectElement { Name = "App" };
        var cls = new ClassElement { Name = "Foo", Coin = 10 };
        project.RootElements.Add(cls);
        root.Projects.Add(project);

        root.TotalCoin.Should().Be(10);
    }

    /// <summary>Více projektů → TotalCoin je součet napříč projekty.</summary>
    [Fact]
    public void TotalCoin_MultipleProjects_AggregatesAll()
    {
        var root = new AppRoot();
        var p1 = new ProjectElement { Name = "Core" };
        p1.RootElements.Add(new ClassElement { Name = "A", Coin = 5 });
        var p2 = new ProjectElement { Name = "App" };
        p2.RootElements.Add(new ClassElement { Name = "B", Coin = 3 });
        root.Projects.Add(p1);
        root.Projects.Add(p2);

        root.TotalCoin.Should().Be(8);
    }

    /// <summary>Projekt s více RootElementy → TotalCoin je součet všech.</summary>
    [Fact]
    public void TotalCoin_ProjectWithMultipleRootElements_AggregatesAll()
    {
        var root = new AppRoot();
        var project = new ProjectElement { Name = "App" };
        project.RootElements.Add(new ClassElement { Name = "A", Coin = 2 });
        project.RootElements.Add(new EnumElement { Name = "B", Coin = 3 });
        root.Projects.Add(project);

        root.TotalCoin.Should().Be(5);
    }
}
