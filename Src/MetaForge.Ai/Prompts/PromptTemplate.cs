namespace MetaForge.Ai.Prompts;

/// <summary>
/// Verzovaná šablona promptu s metadaty.
/// Načítá se z .prompt.md souborů s YAML frontmatter.
/// </summary>
public sealed record PromptTemplate
{
    /// <summary>Název promptu (z názvu souboru bez přípony).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Verze promptu (pro iterativní vylepšování).</summary>
    public int Version { get; init; } = 1;

    /// <summary>Cílový model (např. "llama3", "gemma4").</summary>
    public string Model { get; init; } = "llama3";

    /// <summary>Teplota generování (0.0–1.0).</summary>
    public double Temperature { get; init; } = 0.3;

    /// <summary>Maximální počet tokenů odpovědi.</summary>
    public int MaxTokens { get; init; } = 500;

    /// <summary>Systémový prompt — instrukce pro model.</summary>
    public string SystemPrompt { get; init; } = string.Empty;

    /// <summary>Šablona uživatelského promptu s {{placeholdery}}.</summary>
    public string UserPromptTemplate { get; init; } = string.Empty;

    /// <summary>Tagy pro kategorizaci.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Datum vytvoření.</summary>
    public string Created { get; init; } = string.Empty;

    /// <summary>Autor promptu.</summary>
    public string Author { get; init; } = string.Empty;

    /// <summary>
    /// Aplikuje hodnoty placeholderů do šablony a vrátí výsledný prompt.
    /// </summary>
    /// <param name="placeholders">Mapa placeholder → hodnota (např. {{attributeName}} → "Email").</param>
    public string BuildPrompt(IReadOnlyDictionary<string, string> placeholders)
    {
        var result = UserPromptTemplate;
        foreach (var (key, value) in placeholders)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }
        return result;
    }
}
