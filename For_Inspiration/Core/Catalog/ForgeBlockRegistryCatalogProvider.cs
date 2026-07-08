using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.Core.Catalog;

/// <summary>
/// ICatalogProvider adapter nad ForgeBlockPackageRegistry.
/// Syntetizuje CatalogItem objekty z package catalog entries, ktere maji nastaven CatalogItemType.
/// Entries bez CatalogItemType jsou discovery-only a do CatalogManager nevstupuji.
/// </summary>
public sealed class ForgeBlockRegistryCatalogProvider : ICatalogProvider
{
    private readonly ForgeBlockPackageRegistry _registry;

    public string Name => "forgeblock-packages";

    /// <summary>
    /// Priorita 10: nizsi nez BuiltInCatalogProvider (0), vyssi nez FileSystemCatalogProvider (100).
    /// Prepisuje BuiltIn polozky se stejnym Id, pokud BuiltIn vrati nejake.
    /// </summary>
    public int Priority => 10;

    public ForgeBlockRegistryCatalogProvider(ForgeBlockPackageRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    public Task<IReadOnlyList<CatalogItem>> LoadItemsAsync()
    {
        var items = _registry.GetCatalogEntries()
            .Where(entry => entry.CatalogItemType.HasValue)
            .Select(entry => new CatalogItem
            {
                Id = entry.EntryId,
                DisplayName = entry.DisplayName,
                Description = entry.Description,
                Category = entry.Category,
                ItemType = entry.CatalogItemType!.Value,
                Tags = entry.Tags.ToList(),
                Source = Name,
                FilePath = $"{entry.PackageId}/{entry.EntryId}",
                RawJson = entry.RawPresetJson
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<CatalogItem>>(items);
    }

    public Task<string> LoadContentAsync(CatalogItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.RawJson is not null)
            return Task.FromResult(item.RawJson);

        throw new NotSupportedException(
            $"Package catalog entry '{item.Id}' does not carry inline preset JSON. " +
            "Register content via RawPresetJson on the catalog entry descriptor.");
    }
}
