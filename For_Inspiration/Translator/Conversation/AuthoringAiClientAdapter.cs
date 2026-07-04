using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaForge.Translator;

internal sealed class AuthoringAiClientAdapter : IAuthoringAiClient, IPromptCompletionAiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IAiTranslator _aiTranslator;

    public AuthoringAiClientAdapter(IAiTranslator aiTranslator)
    {
        _aiTranslator = aiTranslator ?? throw new ArgumentNullException(nameof(aiTranslator));
    }

    public bool IsAvailable => _aiTranslator.IsAvailable;

    public Task<string?> CompletePromptAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _aiTranslator.CompletePromptAsync(systemPrompt, userPrompt);
    }

    public async Task<AuthoringResponseEnvelope?> CompleteAuthoringAsync(
        AuthoringPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var response = await _aiTranslator.CompletePromptAsync(
            AuthoringTranslationModelPrompt.BuildSystemPrompt(),
            AuthoringTranslationModelPrompt.BuildUserPrompt(request));

        if (string.IsNullOrWhiteSpace(response))
            return null;

        var payload = AiJsonEnvelopeExtractor.ExtractJsonPayload(response);

        var (envelope, _) = AuthoringAiEnvelopeRepair.TryDeserialize(payload, JsonOptions);
        if (envelope is not null)
            return envelope;

        try
        {
            return new AuthoringResponseEnvelope
            {
                Mode = AuthoringResponseMode.Answer,
                AssistantMessage = payload,
                Warnings = ["AI nevratila validni authoring JSON obalku; odpoved byla ponechana jako text."],
            };
        }
        catch (JsonException)
        {
            return new AuthoringResponseEnvelope
            {
                Mode = AuthoringResponseMode.Answer,
                AssistantMessage = payload,
                Warnings = ["AI nevratila validni authoring JSON obalku; odpoved byla ponechana jako text."],
            };
        }
    }
}
