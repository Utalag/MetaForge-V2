namespace MetaForge.Core.Catalog;

/// <summary>
/// Jednotná obálka nad libovolným presetem v katalogu.
/// Metadata + lazy-loaded obsah.
/// </summary>
public class CatalogItem
{
    /// <summary>Unikátní identifikátor v katalogu.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Zobrazovaný název.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Popis presetu.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Typ položky.</summary>
    public CatalogItemType ItemType { get; set; }

    /// <summary>Kategorie pro filtrování (Domain, Infrastructure, ...).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Tagy pro vyhledávání.</summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>Verze presetu.</summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>Autor presetu.</summary>
    public string Author { get; set; } = "MetaForge";

    /// <summary>Kreditový náklad.</summary>
    public int CreditCost { get; set; }

    /// <summary>Ikona (emoji nebo název ikony).</summary>
    public string Icon { get; set; } = "📦";

    /// <summary>Zdroj (built-in, user, community).</summary>
    public string Source { get; set; } = "built-in";

    /// <summary>Cesta k JSON souboru (nebo embedded resource name).</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Surový JSON obsah (lazy-loaded).</summary>
    public string? RawJson { get; set; }

    /// <summary>Business template — seed pro business metadata (patch operace). Null pro Core-only presety.</summary>
    public BusinessTemplate? BusinessTemplate { get; set; }

    /// <summary>CoreDetail template — seed pro CoreDetail pole atributů. Null pro business-only šablony.</summary>
    public CoreDetailTemplate? CoreDetailTemplate { get; set; }
}

/// <summary>
/// Seed pro business metadata — série patch operací, které se aplikují na BusinessModel.
/// </summary>
public class BusinessTemplate
{
    /// <summary>Patch operace, které se aplikují při použití šablony.</summary>
    public List<BusinessPatchSeed> Operations { get; set; } = [];
}

/// <summary>
/// Jednotlivá seed operace v business šabloně.
/// </summary>
public class BusinessPatchSeed
{
    /// <summary>Typ operace (add_entity, add_attribute, add_relation, ...).</summary>
    public string Op { get; set; } = string.Empty;

    /// <summary>Data operace — klíče odpovídají datovému modelu BusinessPatchOperation.</summary>
    public Dictionary<string, object?> Data { get; set; } = [];
}

/// <summary>
/// Seed pro CoreDetail pole atributů.
/// </summary>
public class CoreDetailTemplate
{
    /// <summary>Výchozí ValueObject jméno pro atributy.</summary>
    public string? ValueObjectName { get; set; }

    /// <summary>Zda je atribut StrongType.</summary>
    public bool IsStrongType { get; set; }

    /// <summary>Odkaz na preset ID v katalogu.</summary>
    public string? ResolvedPresetId { get; set; }
}
