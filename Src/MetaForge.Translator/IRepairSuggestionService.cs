using MetaForge.Feedback.Models;

namespace MetaForge.Translator;

/// <summary>
/// AI-assisted repair suggestion interface — definováno v Translator vrstvě.
/// Implementace v MetaForge.Ai.
/// </summary>
public interface IRepairSuggestionService
{
    Task<IReadOnlyList<RepairRecommendation>> SuggestRepairsAsync(
        AuthoringFeedbackRecord feedback,
        CancellationToken ct);
}
