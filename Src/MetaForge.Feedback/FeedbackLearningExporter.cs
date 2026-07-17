using System.Text;
using System.Text.Json;
using MetaForge.Feedback.Models;
using Microsoft.Extensions.Options;

namespace MetaForge.Feedback;

/// <summary>
/// HTTP-based export learning záznamů — anonymizované, consent-based.
/// </summary>
public sealed class FeedbackLearningExporter
{
    private readonly IFeedbackLearningRepository _repository;
    private readonly HttpClient _http;
    private readonly string _endpoint;

    public FeedbackLearningExporter(
        IFeedbackLearningRepository repository,
        HttpClient http,
        IOptions<FeedbackOptions> options)
    {
        _repository = repository;
        _http = http;
        _endpoint = options.Value.LearningExportEndpoint ?? "https://api.metaforge.dev/v1/feedback";
    }

    public async Task<int> ExportPendingAsync(CancellationToken ct)
    {
        var pending = await _repository.GetPendingExportAsync(ct);
        if (pending.Count == 0) return 0;

        var json = JsonSerializer.Serialize(pending);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(_endpoint, content, ct);
        response.EnsureSuccessStatusCode();

        foreach (var record in pending)
            await _repository.MarkExportedAsync(record.LearningId, ct);

        return pending.Count;
    }

    /// <summary>
    /// Vytvoří anonymizovaný learning record z uzavřeného feedbacku.
    /// Odstraní všechny PII — názvy entit, atributů, raw text.
    /// </summary>
    public static FeedbackLearningRecord Anonymize(AuthoringFeedbackRecord feedback, ResolutionInfo resolution)
    {
        return new FeedbackLearningRecord(
            LearningId: Guid.NewGuid(),
            DiagnosticCode: feedback.CoreDiagnostic.Code,
            Severity: feedback.CoreDiagnostic.Severity.ToString(),
            Stage: feedback.Stage,
            ElementKind: feedback.ElementKind,
            FingerprintHash: feedback.Fingerprint,
            AiUsed: feedback.Suggestions.Count > 0,
            AiSuggestionAccepted: false,
            SuggestionCount: feedback.Suggestions.Count,
            AcceptedSuggestionRank: null,
            ResolutionKind: resolution.ResolutionKind,
            IterationCount: feedback.OccurrenceCount,
            TimeToResolutionMs: (long)(DateTimeOffset.UtcNow - feedback.FirstSeenUtc).TotalMilliseconds,
            PromptTemplateVersion: null,
            AiProvider: null,
            ClosedAtUtc: DateTimeOffset.UtcNow,
            ConsentState: "ReadyForExport"
        );
    }
}
