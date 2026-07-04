using System.Net.Http.Json;
using System.Text.Json;

namespace MetaForge.Translator.Translation;

/// <summary>
/// Implementace IAiTranslator přes přímé HTTP volání Ollama API.
/// Jednoduchá implementace bez závislosti na MetaForge.Ai projektu.
/// Při selhání vrací null (graceful fallback).
/// </summary>
public sealed class OllamaAiTranslator : IAiTranslator
{
    private readonly HttpClient _http;
    private readonly string _model;

    /// <summary>
    /// Vytvoří AI překladač s Ollama backendem.
    /// </summary>
    /// <param name="baseUrl">URL Ollama serveru (výchozí http://localhost:11434).</param>
    /// <param name="model">Název modelu (výchozí llama3).</param>
    public OllamaAiTranslator(string baseUrl = "http://localhost:11434", string model = "llama3")
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(2),
        };
        _model = model;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<string?> CompletePromptAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        try
        {
            var fullPrompt = $"{systemPrompt}\n\n{userPrompt}";
            var request = new
            {
                model = _model,
                prompt = fullPrompt,
                stream = false,
                options = new { temperature = 0.2 }
            };

            var response = await _http.PostAsJsonAsync("/api/generate", request, ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<OllamaResponse>(ct);
            return json?.Response?.Trim();
        }
        catch
        {
            return null; // Graceful fallback
        }
    }

    /// <inheritdoc />
    public async Task<T?> CompleteStructuredAsync<T>(string systemPrompt, string userPrompt, CancellationToken ct = default) where T : class
    {
        var text = await CompletePromptAsync(systemPrompt, userPrompt, ct);
        if (string.IsNullOrWhiteSpace(text)) return null;

        try
        {
            // Extrahovat JSON z odpovědi (může být obalený ```json ... ```)
            var jsonStart = text.IndexOf('{');
            var jsonStart2 = text.IndexOf('[');
            var start = jsonStart >= 0 && jsonStart2 >= 0 ? Math.Min(jsonStart, jsonStart2)
                : jsonStart >= 0 ? jsonStart : jsonStart2;

            if (start < 0) return null;

            var jsonEnd = text.LastIndexOf('}');
            var jsonEnd2 = text.LastIndexOf(']');
            var end = Math.Max(jsonEnd, jsonEnd2);

            if (end <= start) return null;

            var json = text[start..(end + 1)];
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return null;
        }
    }

    private sealed class OllamaResponse
    {
        public string? Response { get; set; }
    }
}
