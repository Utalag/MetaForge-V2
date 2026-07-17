// ---------------------------------------------------------------------------
// MetaForge.Translator — FlowGraphSection
// Derived graph visualization from business model entities and relations.
// Vrstva: Translator / Projections
//
// PROPOSAL: PROP-062 — FlowGraphSection — Derived Flow Visualization
// ---------------------------------------------------------------------------

namespace MetaForge.Translator.Projections;

public sealed record FlowGraphSection
{
    public IReadOnlyList<FlowNode> Nodes { get; init; } = [];
    public IReadOnlyList<FlowEdge> Edges { get; init; } = [];
}

public sealed record FlowNode
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public FlowNodeKind Kind { get; init; }
    public string? ParentEntityId { get; init; }
}

public sealed record FlowEdge
{
    public string FromId { get; init; } = string.Empty;
    public string ToId { get; init; } = string.Empty;
    public FlowEdgeKind Kind { get; init; }
    public string? Label { get; init; }
    public string? Condition { get; init; }
}

public enum FlowNodeKind
{
    Entity = 0,
    Behavior = 1,
}

public enum FlowEdgeKind
{
    Relation = 0,
    Invokes = 1,
}
