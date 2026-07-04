namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Atribut business entity — např. "FirstName", "Email", "Price".
/// Popisuje CO atribut znamená, ne JAK je implementován (to řeší Translator).
/// </summary>
public sealed record BusinessAttributeNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název atributu (např. "FirstName").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Business typ (např. "string", "email", "money").</summary>
    public string Type { get; init; } = "string";

    /// <summary>Je atribut povinný?</summary>
    public bool IsRequired { get; init; }

    /// <summary>Maximální délka (pro string).</summary>
    public int? MaxLength { get; init; }

    /// <summary>Minimální hodnota (pro čísla).</summary>
    public string? MinValue { get; init; }

    /// <summary>Maximální hodnota (pro čísla).</summary>
    public string? MaxValue { get; init; }

    /// <summary>Výchozí hodnota.</summary>
    public string? DefaultValue { get; init; }

    /// <summary>Dodatečná metadata (JSON-friendly).</summary>
    public Dictionary<string, object?> Metadata { get; init; } = new();

    /// <summary>Core-konkretizovaná data — výstup Translatoru / AI enrichmentu.</summary>
    public BusinessAttributeCoreDetail? CoreDetail { get; init; }
}
