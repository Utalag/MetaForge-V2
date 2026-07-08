namespace MetaForge.Core.Discovery;

public sealed record DiscoveryRootResult(IReadOnlyList<DiscoveryCategorySummary> Categories);
public sealed record DiscoveryCategorySummary(string Name, string Description, int ItemCount, bool HasSubCategories);
public sealed record DiscoveryCategoryResult(string Category, string Description, IReadOnlyList<DiscoveryItemSummary> Items, IReadOnlyList<string>? SubCategories);
public sealed record DiscoveryItemSummary(string Id, string DisplayName, string Description, IReadOnlyList<string> Tags, IReadOnlyList<string> SemanticHandles);
public sealed record DiscoveryItemResult(string Id, string DisplayName, string Description, IReadOnlyList<string> Tags, IReadOnlyList<string> SemanticHandles, IReadOnlyDictionary<string, string> Metadata, string? RawContent);

public sealed class DiscoveryQueryResult
{
    public DiscoveryRootResult? Root { get; init; }
    public DiscoveryCategoryResult? Category { get; init; }
    public DiscoveryItemResult? Item { get; init; }
}