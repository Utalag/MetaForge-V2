namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Provenience commandu — metadata o tom, jak command vznikl.
/// Důležité pro AI-assisted scénáře: model, confidence, důvod.
/// </summary>
public sealed class CommandProvenance
{
    /// <summary>Režim vytvoření: "manual", "ai-assisted", "ai-generated", "import".</summary>
    public string Mode { get; init; } = "manual";

    /// <summary>Důvod / kontext vytvoření commandu (např. "uživatel zadal: přidej entitu").</summary>
    public string? Reason { get; init; }

    /// <summary>Název AI modelu, pokud byl použit (např. "llama3:8b", "gpt-4o").</summary>
    public string? Model { get; init; }

    /// <summary>Confidence skóre AI modelu (0.0–1.0), pokud relevantní.</summary>
    public double? Confidence { get; init; }

    /// <summary>Verze promptu, který byl použit pro generování.</summary>
    public string? PromptVersion { get; init; }

    /// <summary>Dodatečná metadata (např. {"tokens_used": 150, "latency_ms": 320}).</summary>
    public Dictionary<string, object?> Metadata { get; init; } = new();
}
