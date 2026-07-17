using MetaForge.Feedback.Models;
using MetaForge.Generators;

namespace MetaForge.Feedback;

public interface IAuthoringFeedbackService
{
    Task<AuthoringFeedbackSnapshot> GetCurrentAsync(string projectId, CancellationToken ct);
    Task MarkDismissedAsync(Guid feedbackId, CancellationToken ct);
    Task MarkResolvedAsync(Guid feedbackId, ResolutionInfo resolution, CancellationToken ct);
    Task CollectFromGeneratorAsync(
        string projectId,
        IReadOnlyList<GeneratedCodeArtifact> artifacts,
        CancellationToken ct);
}
