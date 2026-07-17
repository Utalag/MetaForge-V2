using MetaForge.Ai.Abstractions;
using MetaForge.Feedback.Models;
using MetaForge.Translator;

namespace MetaForge.Ai;

/// <summary>
/// AI implementace IRepairSuggestionService — volitelná, s graceful fallback.
/// </summary>
public sealed class AiRepairSuggestionService : IRepairSuggestionService
{
    private readonly IAiBackendAdapter? _ai;

    public AiRepairSuggestionService(IAiBackendAdapter? ai = null)
    {
        _ai = ai;
    }

    public async Task<IReadOnlyList<RepairRecommendation>> SuggestRepairsAsync(
        AuthoringFeedbackRecord feedback,
        CancellationToken ct)
    {
        // Graceful fallback — bez AI vrací prázdné
        if (_ai == null || !await _ai.IsAvailableAsync(ct))
            return Array.Empty<RepairRecommendation>();

        try
        {
            var prompt = BuildPrompt(feedback);
            var response = await _ai.SendAsync(prompt, ct);

            if (string.IsNullOrWhiteSpace(response))
                return Array.Empty<RepairRecommendation>();

            // Best-effort parse — vrací minimálně jednu variantu
            return
            [
                new RepairRecommendation(
                    Rank: 1,
                    Kind: "ai",
                    Confidence: 0.5m,
                    Explanation: response.Trim(),
                    Rationale: $"Na základě diagnostiky {feedback.CoreDiagnostic.Code}",
                    Evidence: []
                )
            ];
        }
        catch
        {
            // Graceful fallback — AI selhání není blokující
            return Array.Empty<RepairRecommendation>();
        }
    }

    private static string BuildPrompt(AuthoringFeedbackRecord feedback)
    {
        return $"""
        Diagnostika: [{feedback.CoreDiagnostic.Code}] {feedback.CoreDiagnostic.Severity}
        Zpráva: {feedback.CoreDiagnostic.Message}
        Element: {feedback.ElementKind} ({feedback.CoreDiagnostic.Location.Element})
        Stage: {feedback.Stage}

        Navrhni až 3 varianty opravy. Pro každou uveď:
        - Co změnit
        - Proč
        - Jaká je confidence (0.0-1.0)
        """;
    }
}
