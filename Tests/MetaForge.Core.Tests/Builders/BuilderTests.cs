using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Builders;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Builders;

public class BuilderTests
{
    [Fact]
    public void Define_Class_BuildsCorrectModel()
    {
        var model = TypeModelExtensions.Define("TestApp")
            .Class("Order", cls => cls
                .Sealed()
                .Property("Id", TypeModel.Guid, p => p.Init())
                .Property("Total", TypeModel.Decimal, p => p.GetSet())
                .Metadata(m => m.Set("Docs.Summary", "Customer order")))
            .Build();

        model.Namespace.Should().Be("TestApp");
        model.RootElements.Should().HaveCount(1);

        var order = model.RootElements[0].Should().BeOfType<ClassElement>().Subject;
        order.Name.Should().Be("Order");
        order.IsSealed.Should().BeTrue();
        order.Properties.Should().HaveCount(2);
        order.Properties[0].Name.Should().Be("Id");
        order.Properties[0].IsInitOnly.Should().BeTrue();
        order.Metadata.Get<string>("Docs.Summary").Should().Be("Customer order");
    }

    [Fact]
    public void Define_Enum_Members_BuildsCorrectly()
    {
        var model = TypeModelExtensions.Define("TestApp")
            .Enum("Status", e => e
                .Flags()
                .Member("Pending", 0)
                .Member("Active", 1)
                .Member("Archived", 2))
            .Build();

        var status = model.RootElements[0].Should().BeOfType<EnumElement>().Subject;
        status.Name.Should().Be("Status");
        status.IsFlags.Should().BeTrue();
        status.Members.Should().HaveCount(3);
        status.Members[2].Name.Should().Be("Archived");
    }

    [Fact]
    public void MetadataBag_StandardKeys_Work()
    {
        var bag = new MetadataBag();
        bag.Set(MetadataBag.Keys.ValidationRequired, true, MetadataScope.Validation);
        bag.Set(MetadataBag.Keys.DocsSummary, "Test", MetadataScope.Documentation);
        bag.Set(MetadataBag.Keys.GenerationIgnore, false, MetadataScope.Generation);

        bag.Has(MetadataBag.Keys.ValidationRequired).Should().BeTrue();
        bag.Get<bool>(MetadataBag.Keys.ValidationRequired).Should().BeTrue();
        bag.Get<string>(MetadataBag.Keys.DocsSummary).Should().Be("Test");
        bag.Count.Should().Be(3);
    }

    [Fact]
    public void MetadataBag_Merge_Override_Works()
    {
        var a = new MetadataBag().Set("key1", "a-value");
        var b = new MetadataBag().Set("key1", "b-value").Set("key2", "b2");

        a.Merge(b, MergeStrategy.Override);
        a.Get<string>("key1").Should().Be("b-value");
        a.Get<string>("key2").Should().Be("b2");
    }

    [Fact]
    public void MetadataBag_Merge_Skip_PreservesOriginal()
    {
        var a = new MetadataBag().Set("key1", "original");
        var b = new MetadataBag().Set("key1", "new");

        a.Merge(b, MergeStrategy.Skip);
        a.Get<string>("key1").Should().Be("original");
    }
}
