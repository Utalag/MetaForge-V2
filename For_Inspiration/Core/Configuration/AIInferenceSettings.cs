namespace MetaForge.Core.Configuration;

/// <summary>
/// Nizkourovnove nastaveni pro jeden konkretni AI backend.
/// Typicky vznikaji mapovanim z AiPlatformConfiguration nactene z appsettings.json v MetaForge.Ai,
/// ale lze je vytvaret i programove.
/// </summary>
public sealed class AIInferenceSettings
{
    /// <summary>
    /// Zda je AI inference povolena.
    /// Pokud false, použije se pouze rule-based analyzér.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Poskytovatel AI modelu.
    /// </summary>
    public AIProvider Provider { get; set; } = AIProvider.Ollama;

    /// <summary>
    /// Endpoint pro AI API.
    /// - Ollama: http://localhost:11434
    /// - OpenAI: https://api.openai.com/v1
    /// - MiniMax native: https://api.minimax.io/v1
    /// - MiniMax OpenAI-compatible: pouzij Provider=OpenAI a https://api.minimax.io/v1
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Název modelu.
    /// - Ollama: qwen2.5-coder:7b, llama3.1, mistral, phi3
    /// - OpenAI: gpt-4o, gpt-4o-mini, gpt-4-turbo
    /// - MiniMax native: M2-her
    /// - MiniMax OpenAI-compatible: MiniMax-M2.7, MiniMax-M2.7-highspeed, MiniMax-M2.5
    /// </summary>
    public string Model { get; set; } = "qwen2.5-coder:7b";

    /// <summary>
    /// API klíč pro cloudové poskytovatele (OpenAI, MiniMax, atd.).
    /// Pro lokální Ollama obvykle prázdné.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Maximální počet tokenů v odpovědi.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Temperature pro generování (nižší = determinističtější).
    /// Rozsah: 0.0 - 2.0
    /// </summary>
    public float Temperature { get; set; } = 0.3f;

    /// <summary>
    /// Timeout pro AI volání v milisekundách.
    /// </summary>
    public int TimeoutMs { get; set; } = 60000;

    /// <summary>
    /// Zda použít streaming odpovědí.
    /// </summary>
    public bool EnableStreaming { get; set; } = false;

    /// <summary>
    /// Retry pokusů při selhání.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Podporovaní poskytovatelé AI.
/// </summary>
public enum AIProvider
{
    /// <summary>
    /// Lokální Ollama runtime (Qwen, Llama, Mistral, Phi).
    /// </summary>
    Ollama,

    /// <summary>
    /// OpenAI API (GPT-4o, GPT-4o-mini, atd.).
    /// </summary>
    OpenAI,

    /// <summary>
    /// Azure OpenAI Service.
    /// </summary>
    Azure,

    /// <summary>
    /// MiniMax API.
    /// </summary>
    MiniMax,

    /// <summary>
    /// Vlastní HTTP adaptér (Claude, Gemini, atd.).
    /// </summary>
    Custom
}
