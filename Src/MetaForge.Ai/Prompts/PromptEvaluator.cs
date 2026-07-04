namespace MetaForge.Ai.Prompts;

/// <summary>
/// Testovací případ pro vyhodnocení promptu.
/// </summary>
public sealed record PromptTestCase
{
    /// <summary>Název testovacího případu.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Vstupní data (jako JSON string nebo prostý text).</summary>
    public string Input { get; init; } = string.Empty;

    /// <summary>
    /// Validační funkce — vrací true pokud výstup splňuje očekávání.
    /// Např. output => output?.Contains("decimal") == true.
    /// </summary>
    public Func<string?, bool> Validator { get; init; } = _ => true;
}

/// <summary>
/// Výsledek jednoho testovacího případu.
/// </summary>
public sealed record TestCaseResult
{
    /// <summary>Název testovacího případu.</summary>
    public string TestName { get; init; } = string.Empty;

    /// <summary>Prošel test?</summary>
    public bool Passed { get; init; }

    /// <summary>Výstup z AI (může být null).</summary>
    public string? Output { get; init; }

    /// <summary>Chybová zpráva (pokud test selhal).</summary>
    public string? Error { get; init; }
}

/// <summary>
/// Výsledek vyhodnocení celého promptu.
/// </summary>
public sealed record PromptEvalResult
{
    /// <summary>Název promptu.</summary>
    public string PromptName { get; init; } = string.Empty;

    /// <summary>Úspěšnost (0.0–1.0).</summary>
    public double PassRate { get; init; }

    /// <summary>Výsledky jednotlivých testovacích případů.</summary>
    public IReadOnlyList<TestCaseResult> Results { get; init; } = Array.Empty<TestCaseResult>();

    /// <summary>Souhrnná zpráva.</summary>
    public string Summary => $"{PromptName}: {PassRate:P0} úspěšnost ({Results.Count(r => r.Passed)}/{Results.Count})";
}
