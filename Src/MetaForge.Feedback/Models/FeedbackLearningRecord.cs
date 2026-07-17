namespace MetaForge.Feedback.Models;

public sealed record FeedbackLearningRecord(
    Guid LearningId,
    string DiagnosticCode,
    string Severity,
    string Stage,
    string ElementKind,
    string FingerprintHash,
    bool AiUsed,
    bool AiSuggestionAccepted,
    int SuggestionCount,
    int? AcceptedSuggestionRank,
    string ResolutionKind,
    int IterationCount,
    long TimeToResolutionMs,
    string? PromptTemplateVersion,
    string? AiProvider,
    DateTimeOffset ClosedAtUtc,
    string ConsentState
);
