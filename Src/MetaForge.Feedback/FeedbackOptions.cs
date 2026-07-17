namespace MetaForge.Feedback;

/// <summary>
/// Konfigurace feedback modulu — consent, export endpoint.
/// </summary>
public sealed class FeedbackOptions
{
    public bool CacheEnabled { get; init; } = true;
    public string Consent { get; init; } = "NotAsked";
    public string? LearningExportEndpoint { get; init; }
}
