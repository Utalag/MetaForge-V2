using MetaForge.BusinessModel;

namespace MetaForge.Translator;

public sealed class ConversationPromptRequest
{
    public string UserMessage { get; init; } = string.Empty;

    public BusinessAuthoringDocument Document { get; init; } = new();

    public BusinessTreeDetailLevel TreeDetailLevel { get; init; } = BusinessTreeDetailLevel.Extended;

    public string? CurrentTree { get; init; }

    public SemanticBriefJson? PendingBrief { get; init; }

    public AuthoringContextView? AuthoringContext { get; init; }
}