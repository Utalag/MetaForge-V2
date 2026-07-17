using FluentAssertions;
using MetaForge.BusinessModel.Models;
using MetaForge.Translator.Projections;

namespace MetaForge.Translator.Tests.Projections;

public class FlowGraphSectionTests
{
    // === Build tests ===

    [Fact]
    public void Build_EmptyDocument_ReturnsEmptyGraph()
    {
        var doc = EmptyDocument();

        var result = FlowGraphBuilder.Build(doc);

        result.Nodes.Should().BeEmpty();
        result.Edges.Should().BeEmpty();
    }

    [Fact]
    public void Build_EntitiesOnly_ReturnsNodesWithoutEdges()
    {
        var doc = DocumentWithEntities("User", "Product", "Order");

        var result = FlowGraphBuilder.Build(doc);

        result.Nodes.Should().HaveCount(3);
        result.Nodes.Select(n => n.Name).Should().BeEquivalentTo(["User", "Product", "Order"]);
        result.Nodes.Should().AllSatisfy(n => n.Kind.Should().Be(FlowNodeKind.Entity));
        result.Edges.Should().BeEmpty();
    }

    [Fact]
    public void Build_EntitiesWithRelations_ReturnsNodesAndEdges()
    {
        var doc = DocumentWithEntitiesAndRelations(
            ("e1", "User"),
            ("e2", "Reservation")
        );
        AddRelation(doc, "e1", "e2", "OneToMany");

        var result = FlowGraphBuilder.Build(doc);

        result.Nodes.Should().HaveCount(2);
        result.Edges.Should().HaveCount(1);
    }

    [Fact]
    public void Build_RelationTypes_ArePreservedInEdgeLabels()
    {
        var doc = DocumentWithEntitiesAndRelations(
            ("e1", "User"),
            ("e2", "AutoSale")
        );
        AddRelation(doc, "e1", "e2", "OneToMany");

        var result = FlowGraphBuilder.Build(doc);

        result.Edges[0].Label.Should().Be("OneToMany");
        result.Edges[0].FromId.Should().Be("e1");
        result.Edges[0].ToId.Should().Be("e2");
        result.Edges[0].Kind.Should().Be(FlowEdgeKind.Relation);
    }

    [Fact]
    public void Build_WithMultipleRelations_BuildsAllEdges()
    {
        var doc = DocumentWithEntitiesAndRelations(
            ("e1", "User"),
            ("e2", "Reservation"),
            ("e3", "AutoSale")
        );
        AddRelation(doc, "e1", "e2", "HasMany");
        AddRelation(doc, "e2", "e3", "OneToOne");

        var result = FlowGraphBuilder.Build(doc);

        result.Nodes.Should().HaveCount(3);
        result.Edges.Should().HaveCount(2);
        result.Edges.Select(e => e.Label).Should().BeEquivalentTo(["HasMany", "OneToOne"]);
    }

    [Fact]
    public void Build_NodesHaveCorrectIds()
    {
        var doc = DocumentWithEntities("e1", "e2");

        var result = FlowGraphBuilder.Build(doc);

        result.Nodes.Select(n => n.Id).Should().BeEquivalentTo(["e1", "e2"]);
    }

    // === ProjectionBuilder integration ===

    [Fact]
    public void ProjectionBuilder_WithFlowGraphFilter_IncludesFlowGraph()
    {
        var doc = DocumentWithEntitiesAndRelations(
            ("e1", "User"),
            ("e2", "Reservation")
        );
        AddRelation(doc, "e1", "e2", "OneToMany");

        var builder = new ProjectionBuilder(new TestTranslator());
        var filter = ProjectionPresets.FlowGraph;

        var projection = builder.Build(doc, filter);

        projection.FlowGraph.Should().NotBeNull();
        projection.FlowGraph!.Nodes.Should().HaveCount(2);
        projection.FlowGraph.Edges.Should().HaveCount(1);
    }

    [Fact]
    public void ProjectionBuilder_WithoutFlowGraphFilter_HasNullFlowGraph()
    {
        var doc = DocumentWithEntities("e1");

        var builder = new ProjectionBuilder(new TestTranslator());
        var filter = ProjectionPresets.Basic;

        var projection = builder.Build(doc, filter);

        projection.FlowGraph.Should().BeNull();
    }

    // === Helpers ===

    private static BusinessAuthoringDocument EmptyDocument()
    {
        return new BusinessAuthoringDocument { ProjectName = "Test" };
    }

    private static BusinessAuthoringDocument DocumentWithEntities(params string[] names)
    {
        var entities = names.Select(name => new BusinessEntityNode
        {
            Id = name,
            Name = name,
        }).ToList();

        return new BusinessAuthoringDocument
        {
            ProjectName = "Test",
            Entities = entities.AsReadOnly(),
        };
    }

    private static BusinessAuthoringDocument DocumentWithEntitiesAndRelations(params (string Id, string Name)[] entities)
    {
        var entityList = entities.Select(e => new BusinessEntityNode
        {
            Id = e.Id,
            Name = e.Name,
        }).ToList();

        return new BusinessAuthoringDocument
        {
            ProjectName = "Test",
            Entities = entityList.AsReadOnly(),
        };
    }

    private static void AddRelation(BusinessAuthoringDocument doc, string fromId, string toId, string relationType)
    {
        // Relations live on the source entity's Relations list
        var entity = doc.Entities.First(e => e.Id == fromId);
        // BusinessEntityNode is immutable, so we rebuild
        var updatedEntities = doc.Entities.Select(e =>
        {
            if (e.Id == fromId)
            {
                var newRelation = new BusinessRelationNode
                {
                    FromEntityId = fromId,
                    ToEntityId = toId,
                    RelationType = relationType,
                };
                return e with { Relations = e.Relations.Append(newRelation).ToList().AsReadOnly() };
            }
            return e;
        }).ToList();

        // Mutation via reflection on the property setter (doc is a record, Entities is { get; init; })
        var prop = typeof(BusinessAuthoringDocument).GetProperty(nameof(BusinessAuthoringDocument.Entities));
        prop!.SetValue(doc, updatedEntities.AsReadOnly());
    }

    /// <summary>
    /// Minimal translator that returns string type for all attributes.
    /// </summary>
    private sealed class TestTranslator : MetaForge.Translator.Translation.IBusinessTranslator
    {
        public MetaForge.Core.DataTypes.TypeModel Translate(BusinessAttributeNode attribute)
        {
            return MetaForge.Core.DataTypes.TypeModel.String;
        }

        public MetaForge.Translator.Translation.EnrichmentResult? TryEnrich(BusinessAttributeNode attribute)
        {
            return null;
        }

        public Task<MetaForge.Translator.Translation.EnrichmentResult?> TryEnrichAsync(
            BusinessAttributeNode attribute,
            IEnumerable<string> siblingAttributes,
            string? entityName = null,
            CancellationToken ct = default)
        {
            return Task.FromResult<MetaForge.Translator.Translation.EnrichmentResult?>(null);
        }

        public IReadOnlyList<MetaForge.Core.Abstractions.RootElement> TranslateDocument(BusinessAuthoringDocument document)
        {
            return [];
        }
    }
}
