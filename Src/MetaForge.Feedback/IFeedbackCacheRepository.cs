using MetaForge.Feedback.Models;

namespace MetaForge.Feedback;

public interface IFeedbackCacheRepository
{
    Task<IReadOnlyList<AuthoringFeedbackRecord>> LoadOpenAsync(string projectId, CancellationToken ct);
    Task UpsertAsync(AuthoringFeedbackRecord record, CancellationToken ct);
    Task RemoveAsync(Guid feedbackId, CancellationToken ct);
    Task InvalidateByElementAsync(string elementId, CancellationToken ct);
}
