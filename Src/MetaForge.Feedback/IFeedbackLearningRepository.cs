using MetaForge.Feedback.Models;

namespace MetaForge.Feedback;

public interface IFeedbackLearningRepository
{
    Task AppendAsync(FeedbackLearningRecord record, CancellationToken ct);
    Task<IReadOnlyList<FeedbackLearningRecord>> GetPendingExportAsync(CancellationToken ct);
    Task MarkExportedAsync(Guid learningId, CancellationToken ct);
}
