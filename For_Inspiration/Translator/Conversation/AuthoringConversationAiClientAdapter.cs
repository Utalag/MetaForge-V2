using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaForge.Translator;

internal sealed class AuthoringConversationAiClientAdapter : IAuthoringConversationAiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IAiTranslator _aiTranslator;

    public AuthoringConversationAiClientAdapter(IAiTranslator aiTranslator)
    {
        _aiTranslator = aiTranslator ?? throw new ArgumentNullException(nameof(aiTranslator));
    }

    public bool IsAvailable => _aiTranslator.IsAvailable;

    public async Task<ConversationAiResult?> CompleteConversationAsync(
        ConversationPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var response = await _aiTranslator.CompletePromptAsync(
            AuthoringConversationModelPrompt.BuildSystemPrompt(),
            AuthoringConversationModelPrompt.BuildUserPrompt(request));

        if (string.IsNullOrWhiteSpace(response))
            return null;

        var payload = AiJsonEnvelopeExtractor.ExtractJsonPayload(response);

        try
        {
            return JsonSerializer.Deserialize<ConversationAiResult>(payload, JsonOptions)
                ?? new ConversationAiResult
                {
                    AssistantMessage = payload,
                };
        }
        catch (JsonException)
        {
            return new ConversationAiResult
            {
                AssistantMessage = payload,
                Warnings = ["AI nevratila validni conversation JSON obalku; odpoved byla ponechana jako text."],
            };
        }
    }
}
