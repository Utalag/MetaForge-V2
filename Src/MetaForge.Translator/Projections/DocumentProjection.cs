// ---------------------------------------------------------------------------
// MetaForge.Translator — DocumentProjection
// Unified projection type replacing ProjectionView and ExpertProjectionView.
// Vrstva: Translator / Projections
//
// PROPOSAL: PROP-056 — Projection Unification + JSON Snapshot
// ---------------------------------------------------------------------------

using MetaForge.BusinessModel.Models;

namespace MetaForge.Translator.Projections;

public sealed record DocumentProjection
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public IReadOnlyList<EntityProjection> Entities { get; init; } = [];
    public IReadOnlyList<RelationProjection> Relations { get; init; } = [];
    public IReadOnlyList<BehaviorProjection> Behaviors { get; init; } = [];
    public IReadOnlyList<PendingQuestionProjection> PendingQuestions { get; init; } = [];
    public ProjectionDiagnostics? Diagnostics { get; init; }
    public DependencyGraphSection? DependencyGraph { get; init; }
    public FlowGraphSection? FlowGraph { get; init; }
}

public sealed record EntityProjection
{
    public string Id { get; init; } = string.Empty;
    public Guid? CoreId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? PresetId { get; init; }
    public IReadOnlyList<AttributeProjection> Attributes { get; init; } = [];
    public IReadOnlyList<BehaviorProjection> Behaviors { get; init; } = [];
    public int NoteCount { get; init; }
}

public sealed record AttributeProjection
{
    public string Id { get; init; } = string.Empty;
    public Guid? CoreId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BusinessType { get; init; } = "string";
    public string? CoreType { get; init; }
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public string? DefaultValue { get; init; }
    public CoreDetailInfo? CoreDetail { get; init; }
    public AttributeSyncState SyncState { get; init; } = AttributeSyncState.New;
    public IReadOnlyList<string> Constraints { get; init; } = [];
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}

public sealed record CoreDetailInfo
{
    public string? Source { get; init; }
    public string? ResolvedPresetId { get; init; }
    public string? ValueObjectName { get; init; }
    public bool IsStrongType { get; init; }
    public DateTimeOffset? LastSyncedAt { get; init; }
    public AttributeSyncState SyncState { get; init; }
}

public sealed record RelationProjection
{
    public string FromEntityId { get; init; } = string.Empty;
    public string ToEntityId { get; init; } = string.Empty;
    public Guid? FromEntityCoreId { get; init; }
    public Guid? ToEntityCoreId { get; init; }
    public string RelationType { get; init; } = string.Empty;
    public string? NavigationName { get; init; }
}

public sealed record BehaviorProjection
{
    public string Id { get; init; } = string.Empty;
    public Guid? CoreId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ReturnType { get; init; }
    public IReadOnlyList<string> Parameters { get; init; } = [];
}

public sealed record PendingQuestionProjection
{
    public string Id { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string? ContextEntityId { get; init; }
    public bool IsBlocking { get; init; }
}

public sealed record ProjectionDiagnostics
{
    public int TotalAttributes { get; init; }
    public int WithConstraints { get; init; }
    public int StrongTypes { get; init; }
    public int PresetsUsed { get; init; }
    public int UnsyncedAttributes { get; init; }
    public DateTimeOffset BuiltAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record DependencyGraphSection
{
    public int NodeCount { get; init; }
    public int EdgeCount { get; init; }
    public bool HasCycles { get; init; }
    public IReadOnlyList<DependencyNodeProjection> Nodes { get; init; } = [];
}

public sealed record DependencyNodeProjection
{
    public Guid ElementId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string ElementKind { get; init; } = string.Empty;
    public int InDegree { get; init; }
    public int OutDegree { get; init; }
}
