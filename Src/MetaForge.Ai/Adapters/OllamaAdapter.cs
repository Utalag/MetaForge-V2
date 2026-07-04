using System.Net.Http.Json;
using System.Text.Json;
using MetaForge.Ai.Abstractions;

namespace MetaForge.Ai.Adapters;

/// <summary>
/// Adapter pro Ollama — lokální AI server (např. Gemma 4 12B).
/// Endpoint: http://localhost:11434
/// </summary>
public sealed class OllamaAdapter : IAiBackendAdapter
{
    private readonly HttpClient _http;
    private readonly string _model;

    public string ProviderName => "Ollama";

    /// <param name="baseUrl">URL Ollama serveru (výchozí http://localhost:11434).</param>
    /// <param name="model">Název modelu (výchozí gemma4:12b).</param>
    public OllamaAdapter(string baseUrl = "http://localhost:11434", string model = "gemma4")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _model = model;
    }

    /// <summary>Ověří, zda je Ollama server dostupný.</summary>
    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync("/api/tags", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Pošle prompt do Ollama /api/generate a vrátí odpověď.</summary>
    public async Task<string?> SendAsync(string prompt, CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                model = _model,
                prompt = prompt,
                stream = false,
                options = new { temperature = 0.1 }
            };

            var response = await _http.PostAsJsonAsync("/api/generate", request, ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(ct);
            return json?.Response?.Trim();
        }
        catch
        {
            return null; // Graceful fallback — nikdy nevyhazovat výjimku
        }
    }

    /// <summary>Pošle prompt, parsuje odpověď jako JSON.</summary>
    public async Task<T?> SendJsonAsync<T>(string prompt, CancellationToken ct = default) where T : class
    {
        var text = await SendAsync(prompt, ct);
        if (string.IsNullOrWhiteSpace(text)) return null;

        try
        {
            // Extrahuj JSON z odpovědi (může být obalený markdown ```json ... ```)
            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = text[jsonStart..(jsonEnd + 1)];
                return JsonSerializer.Deserialize<T>(json);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    // Response model pro Ollama /api/generate
    private class OllamaGenerateResponse
    {
        public string? Response { get; set; }
    }
}
