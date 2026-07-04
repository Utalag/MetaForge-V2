namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Atribut business entity — např. "FirstName", "Email", "Price".
/// Popisuje CO atribut znamená, ne JAK je implementován (to řeší Translator).
/// </summary>
public sealed class BusinessAttributeNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název atributu (např. "FirstName").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Business typ (např. "string", "email", "money").</summary>
    public string Type { get; set; } = "string";

    /// <summary>Je atribut povinný?</summary>
    public bool IsRequired { get; set; }

    /// <summary>Maximální délka (pro string).</summary>
    public int? MaxLength { get; set; }

    /// <summary>Minimální hodnota (pro čísla).</summary>
    public string? MinValue { get; set; }

    /// <summary>Maximální hodnota (pro čísla).</summary>
    public string? MaxValue { get; set; }

    /// <summary>Výchozí hodnota.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Dodatečná metadata (JSON-friendly).</summary>
    public Dictionary<string, object?> Metadata { get; } = new();
}
