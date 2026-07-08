namespace MetaForge.Core.Catalog;

/// <summary>
/// Parametr presetu — konfigurovatelný vstup pro parametrizované presety.
/// </summary>
public class PresetParameter
{
    /// <summary>Typ parametru (string, enum, bool, int).</summary>
    public string Type { get; set; } = "string";

    /// <summary>Zobrazovaný název.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Popis parametru.</summary>
    public string? Description { get; set; }

    /// <summary>Je parametr povinný?</summary>
    public bool Required { get; set; }

    /// <summary>Výchozí hodnota.</summary>
    public string? Default { get; set; }

    /// <summary>Možné hodnoty (pro typ enum).</summary>
    public List<string>? Options { get; set; }
}
