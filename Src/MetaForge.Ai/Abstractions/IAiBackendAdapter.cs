namespace MetaForge.Ai.Abstractions;

/// <summary>
/// Technická transportní abstrakce pro AI backend (Ollama, OpenAI, Azure).
/// Definice I IMPLEMENTACE jsou v MetaForge.Ai.
/// </summary>
public interface IAiBackendAdapter
{
    /// <summary>Název providera (pro logování).</summary>
    string ProviderName { get; }

    /// <summary>Je backend dostupný?</summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Pošle prompt a vrátí odpověď jako text.
    /// Vrací null při jakékoliv chybě (graceful fallback).
    /// </summary>
    Task<string?> SendAsync(string prompt, CancellationToken ct = default);

    /// <summary>
    /// Pošle prompt a vrátí odpověď jako naparsovaný JSON.
    /// Vrací null při jakékoliv chybě.
    /// </summary>
    Task<T?> SendJsonAsync<T>(string prompt, CancellationToken ct = default) where T : class;
}
