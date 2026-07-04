using System.Text;
using MetaForge.Ai.Runtime;
using MetaForge.Core.Configuration;

namespace MetaForge.Translator;

/// <summary>
/// AI translator using the shared MetaForge AI runtime adapter.
/// Supports any configured AI provider via the AiPlatformConfiguration.
/// </summary>
public class AiTranslationService : IAiTranslator
{
    private readonly AIInferenceSettings _settings;
    private readonly IAiRuntimeAdapter _runtimeAdapter;
    private bool? _available;

    public AiTranslationService(string endpoint = "http://localhost:11434", string model = "qwen2.5-coder:7b")
        : this(new AIInferenceSettings
        {
            Enabled = true,
            Provider = AIProvider.Ollama,
            Endpoint = endpoint,
            Model = model,
        })
    {
    }

    public AiTranslationService(AIInferenceSettings settings, HttpClient? httpClient = null)
        : this(new HttpAiRuntimeAdapter(settings, httpClient))
    {
    }

    public AiTranslationService(IAiRuntimeAdapter runtimeAdapter)
    {
        ArgumentNullException.ThrowIfNull(runtimeAdapter);

        _runtimeAdapter = runtimeAdapter;
        _settings = runtimeAdapter.Settings;
    }

    public bool IsAvailable
    {
        get
        {
            _available ??= _runtimeAdapter.IsAvailable;
            return _available.Value;
        }
    }

    public async Task<string?> CompletePromptAsync(string systemPrompt, string userPrompt)
    {
        if (!_settings.Enabled)
            return null;

        var response = await _runtimeAdapter.CompleteAsync(
            AiCompletionRequest.Create(
                systemPrompt,
                userPrompt,
                _settings.Temperature,
                _settings.MaxTokens,
                _settings.EnableStreaming));

        return response?.Content;
    }
}
