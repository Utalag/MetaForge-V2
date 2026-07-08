namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Implementace IDiscoveryCatalog pro sběr federovaných discovery položek.
/// </summary>
internal sealed class ForgeBlockDiscoveryCatalog : IDiscoveryCatalog
{
    private readonly List<ForgeBlockDiscoveryItem> _items = new();

    public IReadOnlyCollection<ForgeBlockDiscoveryItem> Items => _items.AsReadOnly();

    public void AddItem(ForgeBlockDiscoveryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    public void AddCategory(string categoryName, string description, IReadOnlyList<ForgeBlockDiscoveryItem> items)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryName);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            // Prefix item tags s kategorii
            var taggedItem = item with
            {
                Tags = new[] { $"category:{categoryName}" }.Concat(item.Tags).ToArray()
            };
            _items.Add(taggedItem);
        }
    }
}
