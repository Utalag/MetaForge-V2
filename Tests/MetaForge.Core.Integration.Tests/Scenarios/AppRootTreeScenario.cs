using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators;

namespace MetaForge.Core.Integration.Tests.Scenarios;

/// <summary>
/// Komplexní integrační test — AppRoot strom traversal.
/// </summary>
public class AppRootTreeScenario
{
    private readonly CodeGenerator _generator = new();

    /// <summary>AppRoot se 2 projekty, 5 elementy → generuje 5 artifactů.</summary>
    [Fact]
    public void AppRoot_WithTwoProjects_GeneratesAllArtifacts()
    {
        // Arrange — sestavit strom
        var root = new AppRoot();

        var coreProject = new ProjectElement { Name = "MyApp.Core", DefaultNamespace = "MyApp.Core" };
        var customer = ClassElement.Basic("Customer");
        customer.Properties.Add(PropertyElement.GetSet("Id", TypeModel.Int32));
        customer.Properties.Add(PropertyElement.GetSet("Name", TypeModel.String));
        coreProject.RootElements.Add(customer);
        coreProject.RootElements.Add(EnumElement.Basic("CustomerStatus"));
        coreProject.RootElements.Add(InterfaceElement.Basic("IRepository"));

        var apiProject = new ProjectElement { Name = "MyApp.Api", DefaultNamespace = "MyApp.Api" };
        apiProject.RootElements.Add(ClassElement.Basic("Startup"));
        apiProject.RootElements.Add(ClassElement.Record("ApiConfig"));

        root.Projects.Add(coreProject);
        root.Projects.Add(apiProject);

        // Act — generovat
        var allElements = root.Projects.SelectMany(p => p.RootElements).ToList();
        var artifacts = allElements.Select(e => _generator.Generate(e)).ToList();

        // Assert
        artifacts.Should().HaveCount(5);
        artifacts.Select(a => a.FileName).Should().Contain(
            "Customer.cs", "CustomerStatus.cs", "IRepository.cs", "Startup.cs", "ApiConfig.cs");
        artifacts.ForEach(a => SnapshotComparer.AssertValidSyntax(a.SourceCode));
    }

    /// <summary>AppRoot — file names korelují s element names.</summary>
    [Fact]
    public void AppRoot_FileNameMatchesElementName()
    {
        var root = new AppRoot();
        var project = new ProjectElement { Name = "Test" };
        project.RootElements.Add(ClassElement.Basic("UserAccount"));
        root.Projects.Add(project);

        var artifact = _generator.Generate(project.RootElements[0]);

        artifact.FileName.Should().Be("UserAccount.cs");
    }
}
