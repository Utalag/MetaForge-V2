namespace MetaForge.Core.Discovery;

public interface IDiscoverySession
{
    DiscoveryRootResult GetRoot();
    DiscoveryCategoryResult GetCategory(DiscoveryQuery query);
    DiscoveryItemResult? GetItem(DiscoveryQuery query);
    IReadOnlyList<DiscoveryItemSummary> SearchByTag(string tag);
}