namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Určuje původ informace v Core vrstvě —
/// zda byla vytvořena ručně, generována, nebo kombinací.
/// </summary>
public enum CoreInfoSource
{
    /// <summary>Původ není znám (výchozí stav).</summary>
    Unknown = 0,

    /// <summary>Hodnota byla zadána ručně uživatelem.</summary>
    Manual = 1,

    /// <summary>Hodnota byla vygenerována automaticky (např. AI, analyzátorem).</summary>
    Generated = 2,

    /// <summary>Hodnota vznikla kombinací ručního zadání a automatického doplnění.</summary>
    Hybrid = 3,
}
