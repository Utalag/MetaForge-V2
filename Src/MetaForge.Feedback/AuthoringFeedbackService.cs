using MetaForge.Core.Diagnostics;
using MetaForge.Feedback.Models;
using MetaForge.Generators;
using CoreSeverity = MetaForge.Core.Diagnostics.DiagnosticSeverity;

namespace MetaForge.Feedback;

public sealed class AuthoringFeedbackService : IAuthoringFeedbackService
{
    private readonly IFeedbackCacheRepository _cache;

    public AuthoringFeedbackService(IFeedbackCacheRepository cache)
    {
        _cache = cache;
    }

    public async Task<AuthoringFeedbackSnapshot> GetCurrentAsync(string projectId, CancellationToken ct)
    {
        var open = await _cache.LoadOpenAsync(projectId, ct);
        return new AuthoringFeedbackSnapshot(
            OpenItems: open,
            TotalCount: open.Count,
            ErrorCount: open.Count(r => r.CoreDiagnostic.Severity == CoreSeverity.Error),
            WarningCount: open.Count(r => r.CoreDiagnostic.Severity == CoreSeverity.Warning)
        );
    }

    public async Task MarkDismissedAsync(Guid feedbackId, CancellationToken ct)
    {
        await _cache.RemoveAsync(feedbackId, ct);
    }

    public async Task MarkResolvedAsync(Guid feedbackId, ResolutionInfo resolution, CancellationToken ct)
    {
        // Resolved → smazat z aktivní cache (po novém překladu se už neobjeví)
        await _cache.RemoveAsync(feedbackId, ct);
    }

    public async Task CollectFromGeneratorAsync(
        string projectId,
        IReadOnlyList<GeneratedCodeArtifact> artifacts,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var artifact in artifacts)
        {
            if (artifact.Diagnostics is not { Count: > 0 }) continue;

            foreach (var diag in artifact.Diagnostics)
            {
                var coreSeverity = diag.Severity == Generators.DiagnosticSeverity.Error
                    ? CoreSeverity.Error
                    : CoreSeverity.Warning;

                // Mapovat DiagnosticInfo → Diagnostic (Core)
                var coreDiag = new Diagnostic(
                    Code: diag.Code ?? "GEN-001",
                    Message: diag.Message,
                    Severity: coreSeverity,
                    Location: new ElementPath(
                        Root: artifact.FileName,
                        Element: diag.ElementId ?? "unknown",
                        Segment: diag.ElementName
                    )
                );

                var record = new AuthoringFeedbackRecord(
                    FeedbackId: Guid.NewGuid(),
                    ProjectId: projectId,
                    ElementId: diag.ElementId,
                    ElementKind: "Property",
                    CoreDiagnostic: coreDiag,
                    Stage: "generator",
                    Suggestions: Array.Empty<RepairRecommendation>(),
                    Status: "Open",
                    Fingerprint: $"{diag.ElementId}:{diag.Message.GetHashCode():X8}",
                    FirstSeenUtc: now,
                    LastSeenUtc: now,
                    OccurrenceCount: 1
                );

                await _cache.UpsertAsync(record, ct);
            }
        }
    }
}
