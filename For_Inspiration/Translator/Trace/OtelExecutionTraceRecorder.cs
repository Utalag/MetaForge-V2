using System.Diagnostics;

namespace MetaForge.Translator.Trace;

/// <summary>
/// Implementace IExecutionTraceRecorder pomoci OpenTelemetry ActivitySource.
/// Zakladni rezim loguje komponentu, operaci, vysledek a trvani.
/// Detailni rezim pridava available options, isPredetermined, error paths a stack trace.
/// </summary>
public sealed class OtelExecutionTraceRecorder : IExecutionTraceRecorder
{
    private readonly ActivitySource _activitySource;

    public bool IsDetailedEnabled { get; }

    public OtelExecutionTraceRecorder(ActivitySource activitySource, bool detailedEnabled = false)
    {
        _activitySource = activitySource;
        IsDetailedEnabled = detailedEnabled;
    }

    public void RecordComponentEntered(string component, string operation, string? inputSummary)
    {
        var activity = _activitySource.StartActivity($"{component}.{operation}");
        activity?.SetTag("metaforge.component", component);
        activity?.SetTag("metaforge.operation", operation);
        if (IsDetailedEnabled && inputSummary is not null)
            activity?.SetTag("metaforge.input_summary", inputSummary);
    }

    public void RecordComponentExited(string component, string operation, string? outputSummary, string result, string? errorMessage, long durationMs)
    {
        var activity = Activity.Current;
        if (activity is null) return;

        activity.SetTag("metaforge.result", result);
        if (IsDetailedEnabled && outputSummary is not null)
            activity.SetTag("metaforge.output_summary", outputSummary);
        if (errorMessage is not null)
            activity.SetTag("metaforge.error", errorMessage);

        if (result == "error")
            activity.SetStatus(ActivityStatusCode.Error, errorMessage);
        else
            activity.SetStatus(ActivityStatusCode.Ok);

        activity.Stop();
    }

    public void RecordDecisionEvaluated(string component, string decision, string[] availableOptions, string selectedOption, bool isPredetermined)
    {
        var activity = Activity.Current;
        if (activity is null) return;

        activity.SetTag("metaforge.decision", decision);
        activity.SetTag("metaforge.selected_option", selectedOption);
        if (IsDetailedEnabled)
        {
            activity.SetTag("metaforge.available_options", string.Join(",", availableOptions));
            activity.SetTag("metaforge.is_predetermined", isPredetermined.ToString());
        }
    }

    public void RecordErrorPathAvailable(string component, string condition, string exceptionType, string guardDescription)
    {
        if (!IsDetailedEnabled) return;

        var activity = Activity.Current;
        if (activity is null) return;

        activity.AddEvent(new ActivityEvent("error_path_available", tags: new ActivityTagsCollection
        {
            { "metaforge.component", component },
            { "metaforge.condition", condition },
            { "metaforge.exception_type", exceptionType },
            { "metaforge.guard", guardDescription }
        }));
    }

    public void RecordErrorPathTriggered(string component, string exceptionType, string message, string? stackTrace)
    {
        var activity = Activity.Current;
        if (activity is null) return;

        var tags = new ActivityTagsCollection
        {
            { "metaforge.component", component },
            { "metaforge.exception_type", exceptionType },
            { "metaforge.error_message", message }
        };

        if (IsDetailedEnabled && stackTrace is not null)
            tags.Add("metaforge.stack_trace", stackTrace);

        activity.AddEvent(new ActivityEvent("error_path_triggered", tags: tags));
    }

    public void RecordFallbackUsed(string component, string fallbackName, string reason)
    {
        var activity = Activity.Current;
        if (activity is null) return;

        activity.SetTag("metaforge.fallback", fallbackName);
        if (IsDetailedEnabled)
            activity.SetTag("metaforge.fallback_reason", reason);

        activity.AddEvent(new ActivityEvent("fallback_used", tags: new ActivityTagsCollection
        {
            { "metaforge.component", component },
            { "metaforge.fallback", fallbackName }
        }));
    }
}
