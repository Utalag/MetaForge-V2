using FluentAssertions;
using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.Core.Tests.ForgeBlockPackages;

public class ForgeBlockRegistryTests
{
    private readonly ForgeBlockRegistry _registry = new();

    private static IForgeBlockPackage CreateTestPackage(string handle)
    {
        return new TestForgeBlock(handle);
    }

    [Fact]
    public void Register_AddsPackage()
    {
        var package = CreateTestPackage("test");
        _registry.Register(package);

        _registry.Packages.Should().HaveCount(1);
    }

    [Fact]
    public void Register_DuplicateHandle_Throws()
    {
        var p1 = CreateTestPackage("duplicate");
        var p2 = CreateTestPackage("duplicate");
        _registry.Register(p1);

        var act = () => _registry.Register(p2);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*duplicate*");
    }

    [Fact]
    public void GetPackage_Existing_ReturnsPackage()
    {
        var package = CreateTestPackage("findme");
        _registry.Register(package);

        var found = _registry.GetPackage("findme");
        found.Should().NotBeNull();
        found!.Handle.Should().Be("findme");
    }

    [Fact]
    public void GetPackage_NonExisting_ReturnsNull()
    {
        var found = _registry.GetPackage("nonexistent");
        found.Should().BeNull();
    }

    [Fact]
    public void SearchByTag_FindsMatching()
    {
        var package = CreateTestPackage("math");
        _registry.Register(package);

        var results = _registry.SearchByTag("math");
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void GetAllCapabilities_ReturnsAll()
    {
        var p1 = CreateTestPackage("p1");
        _registry.Register(p1);

        var capabilities = _registry.GetAllCapabilities();
        capabilities.Should().NotBeEmpty();
    }

    private sealed class TestForgeBlock : IForgeBlockPackage
    {
        public string Handle { get; }
        public string Version => "1.0.0";
        public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
        {
            new("test_op", "Test Operation", "A test capability", new[] { "test" }),
        };
        public DiscoveryMetadata Discovery { get; }

        public TestForgeBlock(string handle)
        {
            Handle = handle;
            Discovery = new DiscoveryMetadata(
                DisplayName: handle,
                Description: "Test package",
                Tags: new[] { handle }
            );
        }

        public void Register(ForgeBlockRegistry registry) { }
    }
}
