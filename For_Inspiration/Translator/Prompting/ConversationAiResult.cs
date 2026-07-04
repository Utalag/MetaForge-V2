namespace MetaForge.Translator;

public sealed class ConversationAiResult
{
    public string AssistantMessage { get; init; } = string.Empty;

    public IReadOnlyList<string> Warnings { get; init; } = [];

    public IReadOnlyList<string> Questions { get; init; } = [];

    public SemanticBriefJson? Brief { get; init; }
}