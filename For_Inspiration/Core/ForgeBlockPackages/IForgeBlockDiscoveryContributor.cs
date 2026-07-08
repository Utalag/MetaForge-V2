namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Rozhraní pro federované discovery metadata jednoho ForgeBlock balíčku.
/// Každý ForgeBlock nese vlastní discovery informace — categorie, handle, popisy, příklady.
/// </summary>
public interface IForgeBlockDiscoveryContributor
{
    /// <summary>
    /// ID balíčku, ke kterému contributor patří.
    /// </summary>
    string PackageId { get; }

    /// <summary>
    /// Registraruje discovery metadata do katalogu.
    /// </summary>
    void RegisterDiscovery(IDiscoveryCatalog catalog);
}

/// <summary>
/// Katalog pro registraci federovaných discovery položek.
/// </summary>
public interface IDiscoveryCatalog
{
    /// <summary>
    /// Registruje jednu discovery položku.
    /// </summary>
    void AddItem(ForgeBlockDiscoveryItem item);

    /// <summary>
    /// Registraruje celou kategorii (sub-category) s více položkami.
    /// </summary>
    void AddCategory(string categoryName, string description, IReadOnlyList<ForgeBlockDiscoveryItem> items);
}

/// <summary>
/// Discovery položka z jednoho ForgeBlocku.
/// </summary>
public sealed record ForgeBlockDiscoveryItem
{
    /// <summary>
    /// Unikátní ID v rámci balíčku.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Delší popis (věta nebo dvě).
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Canonical semantic handles (např. "mf.math.sqrt", "mf.datetime.overdue").
    /// </summary>
    public IReadOnlyList<string> SemanticHandles { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Tagy pro filtrování a kategorizaci.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Minimální příklad použití.
    /// </summary>
    public string? UsageExample { get; init; }

    /// <summary>
    /// Výstup / návratová hodnota (pokud je known).
    /// </summary>
    public string? Returns { get; init; }
}
