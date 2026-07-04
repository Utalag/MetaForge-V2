using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MetaForge.Translator.Telemetry;

/// <summary>
/// Helper pro pohodlne mereni duration s automatickym zapsanim do histogramu.
/// </summary>
public readonly struct TelemetryTimer : IDisposable
{
    private readonly Histogram<double> _histogram;
    private readonly TagList _tags;
    private readonly long _startTimestamp;

    public TelemetryTimer(Histogram<double> histogram, TagList tags)
    {
        _histogram = histogram;
        _tags = tags;
        _startTimestamp = Stopwatch.GetTimestamp();
    }

    public void Dispose()
    {
        var elapsedMs = Stopwatch.GetElapsedTime(_startTimestamp).TotalMilliseconds;
        _histogram.Record(elapsedMs, _tags);
    }
}

public static class TelemetryHelper
{
    /// <summary>
    /// Zahaji mereni duration pro dany histogram a tagy.
    /// Po dispose se automaticky zapise elapsed time v ms.
    /// </summary>
    public static TelemetryTimer StartDuration(Histogram<double> histogram, params KeyValuePair<string, object?>[] tags)
    {
        TagList tagList = new(tags);
        return new TelemetryTimer(histogram, tagList);
    }

    /// <summary>
    /// Prevede bool Success a seznam Issues na telemetry result tag.
    /// </summary>
    public static string ResolveResultTag(bool success, IReadOnlyList<BusinessModel.BusinessValidationIssue> issues)
    {
        if (success)
            return TelemetryTags.ResultOk;

        if (issues.Count > 0)
            return TelemetryTags.ResultValidationError;

        return TelemetryTags.ResultError;
    }

    /// <summary>
    /// Prevede bool na telemetry result tag.
    /// </summary>
    public static string ResolveResultTag(bool success)
    {
        return success ? TelemetryTags.ResultOk : TelemetryTags.ResultError;
    }
}
