namespace MetaForge.Translator.Translation;

/// <summary>
/// AI překladač — využívá AI backend pro enrichment business atributů.
/// Volitelná komponenta — při nedostupnosti se použije deterministický DefaultBusinessTranslator.
/// </summary>
public interface IAiTranslator
{
    /// <summary>Je AI backend dostupný a připravený?</summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Pošle prompt AI backendu a vrátí odpověď jako text.
    /// Vrací null při jakékoliv chybě (graceful fallback).
    /// </summary>
    /// <param name="systemPrompt">Systémový prompt s instrukcemi.</param>
    /// <param name="userPrompt">Uživatelský prompt s kontextem.</param>
    Task<string?> CompletePromptAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);

    /// <summary>
    /// Pošle prompt a parsuje odpověď jako JSON do daného typu.
    /// Vrací null při jakékoliv chybě.
    /// </summary>
    Task<T?> CompleteStructuredAsync<T>(string systemPrompt, string userPrompt, CancellationToken ct = default) where T : class;
}
