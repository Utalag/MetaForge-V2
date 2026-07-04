using FluentAssertions;
using MetaForge.Core.Catalog;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Tests.Catalog;

public class CatalogManagerTests
{
    private readonly CatalogManager _catalog = new();

    [Fact]
    public void ResolveType_WithBuiltInProvider_ReturnsPreset()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        var preset = _catalog.ResolveType("email");

        preset.Should().NotBeNull();
        preset!.Name.Should().Be("email");
        preset.Type.BaseType.Should().Be(DataType.String);
    }

    [Fact]
    public void ResolveType_Unknown_ReturnsNull()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        var preset = _catalog.ResolveType("neexistujici_typ_xyz");

        preset.Should().BeNull();
    }

    [Fact]
    public void ResolveType_CustomPreset_TakesPriority()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        _catalog.RegisterPreset(new PresetDefinition("email", TypeModel.Int32));

        var preset = _catalog.ResolveType("email");
        preset.Should().NotBeNull();
        preset!.Type.BaseType.Should().Be(DataType.Int32); // Custom má prioritu
    }

    [Fact]
    public void RegisterPreset_AddsToCatalog()
    {
        var preset = new PresetDefinition("mytype", TypeModel.Guid);
        _catalog.RegisterPreset(preset);

        var result = _catalog.ResolveType("mytype");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Guid);
    }

    [Fact]
    public void GetAllPresets_IncludesBothCustomAndProvider()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        _catalog.RegisterPreset(new PresetDefinition("custom", TypeModel.Bool));

        var all = _catalog.GetAllPresets();
        all.Should().NotBeEmpty();
        all.Should().Contain(p => p.Name == "custom");
        all.Should().Contain(p => p.Name == "email");
    }

    [Fact]
    public void SearchPresets_FindsByName()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        var results = _catalog.SearchPresets("email");

        results.Should().NotBeEmpty();
        results.Should().Contain(p => p.Name == "email");
    }

    [Fact]
    public void SearchPresets_CaseInsensitive()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        var results = _catalog.SearchPresets("EMAIL");

        results.Should().NotBeEmpty();
    }
}
