namespace MetaForge.Core.Catalog;

/// <summary>
/// Možnosti filtrování a hledání v katalogu.
/// </summary>
public class CatalogSearchOptions
{
    /// <summary>Textový filtr (hledá v názvu, popisu, tazích).</summary>
    public string? SearchText { get; set; }

    /// <summary>Filtr podle typu položky.</summary>
    public CatalogItemType? ItemType { get; set; }

    /// <summary>Filtr podle kategorie.</summary>
    public string? Category { get; set; }

    /// <summary>Filtr podle tagů (OR logika).</summary>
    public List<string>? Tags { get; set; }

    /// <summary>Filtr podle zdroje (built-in, user).</summary>
    public string? Source { get; set; }

    /// <summary>Maximální počet výsledků.</summary>
    public int MaxResults { get; set; } = 50;
}

/// <summary>
/// Statistiky katalogu.
/// </summary>
public class CatalogStats
{
    public int TotalItems { get; set; }
    public int ValueObjects { get; set; }
    public int ClassPresets { get; set; }
    public int ForgeBlocks { get; set; }
    public int Providers { get; set; }
}
