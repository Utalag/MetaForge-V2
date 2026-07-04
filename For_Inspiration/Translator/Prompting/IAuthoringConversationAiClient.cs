namespace MetaForge.Translator;

public interface IAuthoringConversationAiClient
{
    bool IsAvailable { get; }

    Task<ConversationAiResult?> CompleteConversationAsync(
        ConversationPromptRequest request,
        CancellationToken cancellationToken = default);
}