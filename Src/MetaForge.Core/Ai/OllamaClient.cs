namespace MetaForge.Core.Ai;

/// <summary>
/// Sdílené rozhraní pro Ollama HTTP API — používané napříč vrstvami.
/// Eliminuje duplikaci mezi OllamaAdapter (MetaForge.Ai) a OllamaAiTranslator (MetaForge.Translator).
/// </summary>
public interface IOllamaClient
{
    /// <summary>Je Ollama server dostupný?</summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>Pošle prompt a vrátí odpověď jako text.</summary>
    Task<string?> SendPromptAsync(string prompt, CancellationToken ct = default);

    /// <summary>Pošle prompt a parsuje odpověď jako JSON.</summary>
    Task<T?> SendStructuredAsync<T>(string prompt, CancellationToken ct = default) where T : class;
}

/// <summary>
/// Implementace IOllamaClient — přímé HTTP volání Ollama /api/generate.
/// </summary>
public sealed class OllamaClient : IOllamaClient
{
    private readonly HttpClient _http;
    private readonly string _model;

    /// <param name="httpClient">HttpClient instance (pro testování).</param>
    /// <param name="model">Název modelu.</param>
    public OllamaClient(HttpClient httpClient, string model = "gemma4")
    {
        _http = httpClient;
        _model = model;
    }

    /// <param name="baseUrl">URL Ollama serveru (výchozí http://localhost:11434).</param>
    /// <param name="model">Název modelu (výchozí gemma4).</param>
    public OllamaClient(string baseUrl = "http://localhost:11434", string model = "gemma4")
        : this(new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromMinutes(2) }, model)
    {
    }

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        try { return (await _http.GetAsync("/api/tags", ct)).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<string?> SendPromptAsync(string prompt, CancellationToken ct = default)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new { model = _model, prompt, stream = false, options = new { temperature = 0.2 } });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("/api/generate", content, ct);
            if (!response.IsSuccessStatusCode) return null;
            var responseJson = await response.Content.ReadAsStringAsync(ct);
            var result = System.Text.Json.JsonSerializer.Deserialize<OllamaResponse>(responseJson);
            return result?.Response?.Trim();
        }
        catch { return null; }
    }

    public async Task<T?> SendStructuredAsync<T>(string prompt, CancellationToken ct = default) where T : class
    {
        var text = await SendPromptAsync(prompt, ct);
        if (string.IsNullOrWhiteSpace(text)) return null;
        try
        {
            var start = Math.Min(text.IndexOf('{'), text.IndexOf('['));
            if (start < 0) return null;
            var end = Math.Max(text.LastIndexOf('}'), text.LastIndexOf(']'));
            if (end <= start) return null;
            return System.Text.Json.JsonSerializer.Deserialize<T>(text[start..(end + 1)]);
        }
        catch { return null; }
    }

    private sealed class OllamaResponse { public string? Response { get; set; } }
}
