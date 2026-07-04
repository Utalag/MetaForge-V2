namespace MetaForge.Infrastructure.Configuration;

/// <summary>
/// Konfigurace AI backendu.
/// </summary>
public sealed class AiOptions
{
    /// <summary>URL Ollama serveru (nebo OpenAI-compatible endpoint).</summary>
    public string Endpoint { get; init; } = "http://localhost:11434";

    /// <summary>Název modelu (např. "llama3", "gemma4:12b").</summary>
    public string Model { get; init; } = "llama3";

    /// <summary>Teplota pro generování (0.0 = deterministické, 1.0 = kreativní).</summary>
    public double Temperature { get; init; } = 0.3;

    /// <summary>Maximální počet tokenů v odpovědi.</summary>
    public int MaxTokens { get; init; } = 500;

    /// <summary>Timeout pro HTTP requesty v sekundách.</summary>
    public int TimeoutSeconds { get; init; } = 120;
}
