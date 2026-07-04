namespace MetaForge.BusinessModel;

public sealed class BusinessProjectionCheckpoint
{
    public const string CurrentSchemaVersion = "business-projection-checkpoint/v1";

    public string SchemaVersion { get; init; } = CurrentSchemaVersion;

    public string StreamId { get; init; } = string.Empty;

    public int CommandCount { get; init; }

    public DateTimeOffset SavedAt { get; init; } = DateTimeOffset.UtcNow;

    public BusinessAuthoringDocument Document { get; init; } = new();
}