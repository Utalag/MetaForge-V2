using MetaForge.Core.Catalog;

namespace MetaForge.BusinessModel.Catalog;

/// <summary>
/// Dispatcher, ktery urcuje aplikacni cestu pro CatalogItem.
/// Core presety (ValueObject, ...) → ResolveType(), Business sablony → PatchOperation serie.
/// </summary>
public static class CatalogApplicationDispatch
{
    /// <summary>
    /// Zda je typ polozky business template (aplikuje se jako patch serie).
    /// </summary>
    public static bool IsBusinessTemplate(CatalogItemType type) => type is
        CatalogItemType.EntityTemplate or
        CatalogItemType.DomainTemplate or
        CatalogItemType.ArchitectureTemplate;

    /// <summary>
    /// Zda je typ polozky Core preset (aplikuje se pres ResolveType / CatalogManager).
    /// </summary>
    public static bool IsCorePreset(CatalogItemType type) => type is
        CatalogItemType.ValueObject or
        CatalogItemType.ClassPreset or
        CatalogItemType.InterfacePreset or
        CatalogItemType.EnumPreset or
        CatalogItemType.StructPreset or
        CatalogItemType.ForgeBlock;

    /// <summary>
    /// Extrahuje business patch operace z CatalogItem.
    /// Vraci prazdny seznam pokud item nema BusinessTemplate.
    /// </summary>
    public static IReadOnlyList<BusinessPatchOperation> ExtractBusinessOperations(CatalogItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.BusinessTemplate is null || item.BusinessTemplate.Operations.Count == 0)
            return [];

        return item.BusinessTemplate.Operations
            .Select(seed => new BusinessPatchOperation
            {
                Op = seed.Op,
                Data = new Dictionary<string, object?>(seed.Data),
            })
            .ToList();
    }
}
