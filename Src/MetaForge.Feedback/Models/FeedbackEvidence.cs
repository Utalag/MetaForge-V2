namespace MetaForge.Feedback.Models;

public sealed record FeedbackEvidence(
    string Type,
    string Key,
    string Value
);
