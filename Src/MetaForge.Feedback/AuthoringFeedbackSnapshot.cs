using MetaForge.Feedback.Models;

namespace MetaForge.Feedback;

public sealed record AuthoringFeedbackSnapshot(
    IReadOnlyList<AuthoringFeedbackRecord> OpenItems,
    int TotalCount,
    int ErrorCount,
    int WarningCount
);
