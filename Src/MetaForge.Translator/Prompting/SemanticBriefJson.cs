namespace MetaForge.Translator.Prompting;

/// <summary>
/// Strukturovaný JSON výstup z AI pro enrichment atributu.
/// Toto je očekávaný formát odpovědi z AI modelu.
/// </summary>
public sealed record SemanticBriefJson
{
    /// <summary>Navrhovaný C# typ (např. "string", "decimal", "Email").</summary>
    public string? SuggestedType { get; init; }

    /// <summary>Validační pravidla (např. ["not_empty", "max_length:200"]).</summary>
    public List<string>? ValidationRules { get; init; }

    /// <summary>Výchozí hodnota (např. null, "0", "DateTime.UtcNow").</summary>
    public string? DefaultValue { get; init; }

    /// <summary>Maximální délka pro string atributy.</summary>
    public int? MaxLength { get; init; }

    /// <summary>Minimální délka pro string atributy.</summary>
    public int? MinLength { get; init; }

    /// <summary>Minimální hodnota pro číselné atributy.</summary>
    public decimal? MinValue { get; init; }

    /// <summary>Maximální hodnota pro číselné atributy.</summary>
    public decimal? MaxValue { get; init; }

    /// <summary>Regex pattern (např. pro Email, Phone).</summary>
    public string? RegexPattern { get; init; }

    /// <summary>Míra důvěry modelu (0.0–1.0).</summary>
    public double Confidence { get; init; }
}
