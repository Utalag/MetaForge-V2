using System.Diagnostics;

namespace MetaForge.Translator.Telemetry;

/// <summary>
/// ActivitySource pro Translator vrstvu — sledování překladu a AI enrichmentu.
/// </summary>
public static class TranslatorActivitySource
{
    public const string SourceName = "MetaForge.Translator";
    private static readonly ActivitySource Source = new(SourceName, "1.0.0");

    /// <summary>Vytvoří span pro překlad atributu.</summary>
    public static Activity? StartTranslationActivity(string attributeName, string attributeType)
    {
        var activity = Source.StartActivity("Translator.Translate", ActivityKind.Internal);
        activity?.SetTag("attribute.name", attributeName);
        activity?.SetTag("attribute.type", attributeType);
        return activity;
    }

    /// <summary>Vytvoří span pro AI enrichment.</summary>
    public static Activity? StartAiEnrichmentActivity(string entityName, int attributeCount)
    {
        var activity = Source.StartActivity("Translator.AiEnrichment", ActivityKind.Internal);
        activity?.SetTag("entity.name", entityName);
        activity?.SetTag("attribute.count", attributeCount);
        return activity;
    }

    /// <summary>Zaznamená výsledek AI enrichmentu.</summary>
    public static void RecordAiResult(Activity? activity, string? model, double durationMs, double? confidence)
    {
        activity?.SetTag("ai.model", model);
        activity?.SetTag("ai.duration_ms", durationMs);
        activity?.SetTag("ai.confidence", confidence);
    }
}
