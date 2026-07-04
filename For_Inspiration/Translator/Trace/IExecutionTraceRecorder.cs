using System.Diagnostics;

namespace MetaForge.Translator.Trace;

/// <summary>
/// Datove typy pro strukturovany execution trace. Kazdy event nese komponentu,
/// operaci, correlation ID a casovou znacku.
/// </summary>

public readonly record struct ComponentEnteredEvent(
    string Component,
    string Operation,
    string? InputSummary,
    string CorrelationId,
    DateTimeOffset Timestamp
);

public readonly record struct ComponentExitedEvent(
    string Component,
    string Operation,
    string? OutputSummary,
    string Result,
    string? ErrorMessage,
    long DurationMs,
    string CorrelationId,
    DateTimeOffset Timestamp
);

public readonly record struct DecisionEvaluatedEvent(
    string Component,
    string Decision,
    string[] AvailableOptions,
    string SelectedOption,
    bool IsPredetermined,
    string CorrelationId,
    DateTimeOffset Timestamp
);

public readonly record struct ErrorPathAvailableEvent(
    string Component,
    string Condition,
    string ExceptionType,
    string GuardDescription,
    string CorrelationId,
    DateTimeOffset Timestamp
);

public readonly record struct ErrorPathTriggeredEvent(
    string Component,
    string ExceptionType,
    string Message,
    string? StackTrace,
    string CorrelationId,
    DateTimeOffset Timestamp
);

public readonly record struct FallbackUsedEvent(
    string Component,
    string FallbackName,
    string Reason,
    string CorrelationId,
    DateTimeOffset Timestamp
);

/// <summary>
/// Implementace IExecutionTraceRecorder pomoci OpenTelemetry ActivitySource.
/// Zakladni rezim loguje component/operation/result/duration, detailni rezim
/// navic loguje availableOptions, isPredetermined, error paths, stack traces.
/// </summary>
public interface IExecutionTraceRecorder
{
    bool IsDetailedEnabled { get; }

    void RecordComponentEntered(string component, string operation, string? inputSummary = null);
    void RecordComponentExited(string component, string operation, string? outputSummary, string result, string? errorMessage = null, long durationMs = 0);
    void RecordDecisionEvaluated(string component, string decision, string[] availableOptions, string selectedOption, bool isPredetermined = false);
    void RecordErrorPathAvailable(string component, string condition, string exceptionType, string guardDescription);
    void RecordErrorPathTriggered(string component, string exceptionType, string message, string? stackTrace = null);
    void RecordFallbackUsed(string component, string fallbackName, string reason);
}
