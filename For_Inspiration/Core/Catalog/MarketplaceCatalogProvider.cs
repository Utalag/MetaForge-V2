using MetaForge.Core.Catalog;

namespace MetaForge.Core.Catalog;

/// <summary>
/// Placeholder pro budouci Marketplace catalog provider (Priority 50).
/// Dnes vraci prazdny seznam — pripraveno pro budouci napojeni na feed.
/// </summary>
public sealed class MarketplaceCatalogProvider : ICatalogProvider
{
    public string Name => "marketplace";
    public int Priority => 50;

    public Task<IReadOnlyList<CatalogItem>> LoadItemsAsync()
    {
        return Task.FromResult<IReadOnlyList<CatalogItem>>([]);
    }

    public Task<string> LoadContentAsync(CatalogItem item)
    {
        return Task.FromResult(item.RawJson ?? string.Empty);
    }
}
