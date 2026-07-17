// ---------------------------------------------------------------------------
// MetaForge.Translator — FlowGraphBuilder
// Builds FlowGraphSection from BusinessAuthoringDocument.
// Vrstva: Translator / Projections
//
// PROPOSAL: PROP-062 — FlowGraphSection — Derived Flow Visualization
// ---------------------------------------------------------------------------

using MetaForge.BusinessModel.Models;

namespace MetaForge.Translator.Projections;

public static class FlowGraphBuilder
{
    public static FlowGraphSection Build(BusinessAuthoringDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var nodes = new List<FlowNode>();
        var edges = new List<FlowEdge>();

        foreach (var entity in document.Entities)
        {
            nodes.Add(new FlowNode
            {
                Id = entity.Id,
                Name = entity.Name,
                Kind = FlowNodeKind.Entity,
            });
        }

        foreach (var entity in document.Entities)
        {
            foreach (var relation in entity.Relations)
            {
                edges.Add(new FlowEdge
                {
                    FromId = relation.FromEntityId,
                    ToId = relation.ToEntityId,
                    Kind = FlowEdgeKind.Relation,
                    Label = relation.RelationType,
                });
            }
        }

        return new FlowGraphSection { Nodes = nodes, Edges = edges };
    }
}
