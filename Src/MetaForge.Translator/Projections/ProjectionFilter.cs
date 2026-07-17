// ---------------------------------------------------------------------------
// MetaForge.Translator — ProjectionFilter
// Controllable detail level for DocumentProjection.
// Vrstva: Translator / Projections
//
// PROPOSAL: PROP-056 — Projection Unification + JSON Snapshot
// ---------------------------------------------------------------------------

namespace MetaForge.Translator.Projections;

public sealed record ProjectionFilter
{
    public bool IncludeRelations { get; init; }
    public bool IncludeBehaviors { get; init; }
    public bool IncludeDiagnostics { get; init; }
    public bool IncludePendingQuestions { get; init; }
    public bool IncludeNotes { get; init; }
    public AttributeDetailLevel AttributeDetail { get; init; } = AttributeDetailLevel.NameAndType;
    public bool IncludeCoreDetail { get; init; }
    public bool IncludeSyncState { get; init; }
    public bool IncludeConstraints { get; init; }
    public bool IncludeMetadata { get; init; }
    public bool IncludeCoinCost { get; init; }
    public bool IncludeCoreIds { get; init; }
    public bool IncludeDependencyGraph { get; init; }
    public bool IncludeFlowGraph { get; init; }
}

public enum AttributeDetailLevel
{
    NameOnly,
    NameAndType,
    WithValidation,
    Full,
}

public static class ProjectionPresets
{
    public static ProjectionFilter Basic => new();
    public static ProjectionFilter Expert => new()
    {
        IncludeRelations = true, IncludeBehaviors = true, IncludeDiagnostics = true,
        IncludePendingQuestions = true, IncludeNotes = true, IncludeCoreDetail = true,
        IncludeSyncState = true, IncludeConstraints = true, IncludeMetadata = true,
        IncludeCoinCost = true, IncludeCoreIds = true, IncludeDependencyGraph = true,
        AttributeDetail = AttributeDetailLevel.Full,
    };
    public static ProjectionFilter AiEnrichment => new()
    {
        IncludeCoreDetail = true, IncludeConstraints = true, IncludeCoreIds = true,
        AttributeDetail = AttributeDetailLevel.WithValidation,
    };
    public static ProjectionFilter FlowGraph => new()
    {
        IncludeRelations = true,
        IncludeFlowGraph = true,
    };
}
