using MetaForge.BusinessModel;

namespace MetaForge.Translator;

public sealed class ConversationTurnResult
{
    public bool Success { get; init; }

    public AuthoringResponseMode Mode { get; init; } = AuthoringResponseMode.Answer;

    public string AssistantMessage { get; init; } = string.Empty;

    public string Tree { get; init; } = string.Empty;

    public string ReadDocumentJson { get; init; } = string.Empty;

    public string WriteDocumentJson { get; init; } = string.Empty;

    public string PersistedDocumentPath { get; init; } = string.Empty;

    public BusinessTreeDetailLevel TreeDetailLevel { get; init; } = BusinessTreeDetailLevel.Extended;

    public IReadOnlyList<string> Warnings { get; init; } = [];

    public IReadOnlyList<string> Questions { get; init; } = [];

    public IReadOnlyList<BusinessValidationIssue> Issues { get; init; } = [];

    public int AppliedOperationCount { get; init; }

    public string? PendingBriefJson { get; init; }
}