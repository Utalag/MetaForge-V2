namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Neutralni metadata jedne invocable capability (tool/operace).
/// Bez MCP/CLI specifik — host bootstrap mapuje na host-specificky schema.
/// </summary>
public sealed record CapabilityMetadata
{
    public required string PackageId { get; init; }

    public required string ToolId { get; init; }

    public required string DisplayName { get; init; }

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<ParameterMetadata> Parameters { get; init; } = [];

    public ReturnMetadata? Returns { get; init; }

    public IReadOnlyList<string> Tags { get; init; } = [];

    public IReadOnlyList<string> SemanticHandles { get; init; } = [];
}
