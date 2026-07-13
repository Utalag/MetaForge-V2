using MetaForge.BusinessModel.Models;

namespace MetaForge.Translator.Host;

/// <summary>
/// Bohatší projekce business modelu — obsahuje diagnostiku, workflow stavy, relace.
/// Rozšiřuje základní ProjectionView o expertní informace pro pokročilé scénáře.
/// </summary>
public sealed record ExpertProjectionView
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public int EntityCount { get; init; }
    public int RelationCount { get; init; }
    public int OpenQuestionCount { get; init; }
    public int PendingAttributeCount { get; init; }
    public int SyncedAttributeCount { get; init; }

    public IReadOnlyList<ExpertEntityProjection> Entities { get; init; } = [];
    public IReadOnlyList<ExpertRelationProjection> Relations { get; init; } = [];
    public IReadOnlyList<ExpertPendingQuestionProjection> PendingQuestions { get; init; } = [];
    public ExpertProjectionDiagnostics Diagnostics { get; init; } = new();
}

public sealed record ExpertEntityProjection
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? PresetId { get; init; }
    public int NoteCount { get; init; }
    public int BehaviorCount { get; init; }
    public IReadOnlyList<ExpertAttributeProjection> Attributes { get; init; } = [];
    public IReadOnlyList<ExpertBehaviorProjection> Behaviors { get; init; } = [];
}

public sealed record ExpertAttributeProjection
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string BusinessType { get; init; } = "string";
    public string? CoreType { get; init; }
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public string? DefaultValue { get; init; }
    public bool IsStrongType { get; init; }
    public AttributeSyncState SyncState { get; init; } = AttributeSyncState.New;
    public IReadOnlyList<string> Constraints { get; init; } = [];
}

public sealed record ExpertBehaviorProjection
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Returns { get; init; }
    public IReadOnlyList<string> Inputs { get; init; } = [];
    public IReadOnlyList<string> Constraints { get; init; } = [];
}

public sealed record ExpertRelationProjection
{
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? Navigation { get; init; }
}

public sealed record ExpertPendingQuestionProjection
{
    public string Id { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;
    public bool IsBlocking { get; init; }
}

/// <summary>
/// Diagnostické statistiky expertní projekce.
/// </summary>
public sealed record ExpertProjectionDiagnostics
{
    public int TotalAttributes { get; init; }
    public int WithConstraints { get; init; }
    public int StrongTypes { get; init; }
    public int PresetsUsed { get; init; }
    public int UnsyncedAttributes { get; init; }
    public DateTimeOffset BuiltAt { get; init; } = DateTimeOffset.UtcNow;
}
