using MetaForge.Core.Catalog;

namespace MetaForge.BusinessModel.Catalog;

/// <summary>
/// Catalog provider pro business šablony (Priority 20).
/// Registruje CatalogItem záznamy s BusinessTemplate a/nebo CoreDetailTemplate.
/// </summary>
public sealed class BusinessTemplateCatalogProvider : ICatalogProvider
{
    private readonly List<CatalogItem> _items = [];

    public string Name => "business-templates";
    public int Priority => 20;

    public void AddTemplate(CatalogItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.Id);

        if (item.BusinessTemplate is null && item.CoreDetailTemplate is null)
            throw new ArgumentException("Business template item must have BusinessTemplate or CoreDetailTemplate.", nameof(item));

        _items.Add(item);
    }

    public Task<IReadOnlyList<CatalogItem>> LoadItemsAsync()
    {
        return Task.FromResult<IReadOnlyList<CatalogItem>>(_items.AsReadOnly());
    }

    public Task<string> LoadContentAsync(CatalogItem item)
    {
        return Task.FromResult(item.RawJson ?? string.Empty);
    }
}
