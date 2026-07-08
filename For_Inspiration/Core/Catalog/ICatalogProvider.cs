namespace MetaForge.Core.Catalog;

/// <summary>
/// Provider dodávající preset položky do katalogu.
/// Implementace: BuiltInCatalogProvider, FileSystemCatalogProvider.
/// </summary>
public interface ICatalogProvider
{
    /// <summary>Název provideru.</summary>
    string Name { get; }

    /// <summary>Priorita (nižší = vyšší priorita, built-in = 0).</summary>
    int Priority { get; }

    /// <summary>Načte všechny dostupné položky.</summary>
    Task<IReadOnlyList<CatalogItem>> LoadItemsAsync();

    /// <summary>Načte obsah konkrétní položky.</summary>
    Task<string> LoadContentAsync(CatalogItem item);
}
