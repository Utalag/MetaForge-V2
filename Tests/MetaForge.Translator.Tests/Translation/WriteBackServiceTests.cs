using FluentAssertions;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.Translator.Translation;

namespace MetaForge.Translator.Tests.Translation;

public class WriteBackServiceTests
{
    private static WriteBackService CreateService()
    {
        var store = new CommandLogStore();
        var patchEngine = new PatchEngine(store);
        return new WriteBackService(patchEngine);
    }

    [Fact]
    public void ApplyEnrichment_ValidAttribute_CreatesCoreDetail()
    {
        var enrichment = new EnrichmentResult(
            AttributeId: "a1",
            SuggestedCSharpType: "EmailAddress",
            ValidationRules: new List<string> { "email_format" },
            MaxLength: 254
        );

        var doc = CreateDocumentWithAttribute("a1", "email");
        var svc = CreateService();

        var result = svc.ApplyEnrichment(doc, "e1", enrichment);

        var entity = result.Entities.First(e => e.Id == "e1");
        var attr = entity.Attributes.First(a => a.Id == "a1");
        attr.CoreDetail.Should().NotBeNull();
        attr.CoreDetail!.ValueObjectName.Should().Be("EmailAddress");
        attr.CoreDetail.Source.Should().Be(CoreInfoSource.Generated);
        attr.CoreDetail.SyncState.Should().Be(AttributeSyncState.Synced);
        attr.CoreDetail.LastSyncedAt.Should().NotBeNull();
    }

    [Fact]
    public void ApplyEnrichment_NonExistentEntity_ReturnsDocumentUnchanged()
    {
        var enrichment = new EnrichmentResult(
            AttributeId: "a1",
            SuggestedCSharpType: "string"
        );

        var doc = CreateDocumentWithAttribute("a1", "string");
        var svc = CreateService();

        var result = svc.ApplyEnrichment(doc, "nonexistent", enrichment);

        result.Should().BeSameAs(doc);
    }

    [Fact]
    public void ApplyEnrichment_NullEnrichment_ThrowsArgumentNullException()
    {
        var doc = CreateDocumentWithAttribute("a1", "string");
        var svc = CreateService();

        var act = () => svc.ApplyEnrichment(doc, "e1", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private static BusinessAuthoringDocument CreateDocumentWithAttribute(string attrId, string type)
    {
        var attr = new BusinessAttributeNode
        {
            Id = attrId,
            Name = "TestAttr",
            Type = type
        };

        var entity = new BusinessEntityNode
        {
            Id = "e1",
            Name = "TestEntity",
            Attributes = new[] { attr }.ToList().AsReadOnly()
        };

        return new BusinessAuthoringDocument
        {
            ProjectName = "Test",
            Entities = new[] { entity }.ToList().AsReadOnly()
        };
    }
}
