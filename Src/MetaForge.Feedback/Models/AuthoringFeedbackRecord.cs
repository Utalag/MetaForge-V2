using MetaForge.Core.Diagnostics;

namespace MetaForge.Feedback.Models;

/// <summary>
/// Agregát nad Diagnostic (Core) — přidává per-project metadata a lifecycle.
/// Wrapper, ne duplicitní typ.
/// </summary>
public sealed record AuthoringFeedbackRecord(
    Guid FeedbackId,
    string ProjectId,
    string? ElementId,
    string ElementKind,
    Diagnostic CoreDiagnostic,
    string Stage,
    IReadOnlyList<RepairRecommendation> Suggestions,
    string Status,
    string Fingerprint,
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc,
    int OccurrenceCount
);
