namespace MetaForge.Feedback.Models;

/// <summary>
/// Návrh opravy — read-only výstup pro uživatele/AI.
/// Nikdy se neaplikuje automaticky.
/// </summary>
public sealed record RepairRecommendation(
    int Rank,
    string Kind,
    decimal Confidence,
    string Explanation,
    string Rationale,
    IReadOnlyList<FeedbackEvidence> Evidence
);
