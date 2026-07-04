namespace MetaForge.BusinessModel;

public sealed class BusinessPatchCommandContext
{
    public string StreamId { get; init; } = string.Empty;

    public long? ExpectedVersion { get; init; }

    public DateTimeOffset? IssuedAt { get; init; }

    public CommandIssuedBy IssuedBy { get; init; } = new();

    public CommandSource Source { get; init; } = CommandSource.Unknown;

    public string? CorrelationId { get; init; }

    public string? CausationId { get; init; }

    public string? MutationId { get; init; }

    public CommandProvenance Provenance { get; init; } = new();
}
