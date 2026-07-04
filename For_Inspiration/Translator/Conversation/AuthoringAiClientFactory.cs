using MetaForge.Ai.Configuration;

namespace MetaForge.Translator;

internal static class AuthoringAiClientFactory
{
    public static AuthoringAiClientDefaults CreateDefaults()
    {
        var aiConfiguration = AiPlatformConfiguration.Load();

        var conversationSettings = aiConfiguration.GetSettingsForSegment(AiSegment.Conversation);
        var translationSettings = aiConfiguration.GetSettingsForSegment(AiSegment.AuthoringTranslation);

        return new AuthoringAiClientDefaults(
            new AuthoringConversationAiClientAdapter(new AiTranslationService(conversationSettings)),
            conversationSettings.Enabled,
            conversationSettings.TimeoutMs,
            new AuthoringAiClientAdapter(new AiTranslationService(translationSettings)),
            translationSettings.Enabled,
            translationSettings.TimeoutMs);
    }
}

internal sealed record AuthoringAiClientDefaults(
    IAuthoringConversationAiClient ConversationClient,
    bool ConversationEnabled,
    int ConversationTimeoutMs,
    IAuthoringAiClient TranslationClient,
    bool TranslationEnabled,
    int TranslationTimeoutMs);