using System.Text.Json;
using MetaForge.BusinessModel;

namespace MetaForge.Translator;

/// <summary>
/// Orchestrátor node-level AI asistence. Volá AI s node-scoped kontextem a prevadi vysledek.
/// </summary>
public sealed class NodeAssistService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private readonly IPromptCompletionAiClient _aiClient;

    internal NodeAssistService(IPromptCompletionAiClient aiClient)
    {
        _aiClient = aiClient ?? throw new ArgumentNullException(nameof(aiClient));
    }

    public bool IsAvailable => _aiClient.IsAvailable;

    /// <summary>
    /// Pozada AI o navrh pro dany node kontext. Vraci null pokud AI neni dostupne nebo selze.
    /// </summary>
    public async Task<NodeAssistResult?> AssistAsync(
        NodeAssistContext context,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_aiClient.IsAvailable)
            return null;

        var systemPrompt = NodeAssistModelPrompt.BuildSystemPrompt();
        var userPromptText = NodeAssistModelPrompt.BuildUserPrompt(context, userPrompt);

        string? response;
        try
        {
            response = await _aiClient.CompletePromptAsync(systemPrompt, userPromptText, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(response))
            return null;

        var payload = AiJsonEnvelopeExtractor.ExtractJsonPayload(response);
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        try
        {
            var result = JsonSerializer.Deserialize<NodeAssistResult>(payload, JsonOptions);
            return result is null ? null : NodeAssistOperationScopeValidator.Sanitize(context, result);
        }
        catch (JsonException)
        {
            // Fallback: vratime raw text jako explanation
            return new NodeAssistResult
            {
                Explanation = payload,
            };
        }
    }
}
