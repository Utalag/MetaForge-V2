using System.Diagnostics;

namespace MetaForge.Translator.Trace;

/// <summary>
/// Pomocne extension metody pro pohodlne pouziti IExecutionTraceRecorder.
/// Hlavni metoda TraceComponentScope automaticky spari RecordComponentEntered
/// a RecordComponentExited pomoci IDisposable vzoru.
/// </summary>
public static class ExecutionTraceRecorderExtensions
{
    public static IDisposable TraceComponentScope(
        this IExecutionTraceRecorder recorder,
        string component,
        string operation,
        string? inputSummary = null)
    {
        recorder.RecordComponentEntered(component, operation, inputSummary);
        var sw = Stopwatch.StartNew();
        return new ComponentScope(recorder, component, operation, sw);
    }

    private sealed class ComponentScope : IDisposable
    {
        private readonly IExecutionTraceRecorder _recorder;
        private readonly string _component;
        private readonly string _operation;
        private readonly Stopwatch _sw;
        private bool _disposed;
        private string? _explicitError;

        public ComponentScope(IExecutionTraceRecorder recorder, string component, string operation, Stopwatch sw)
        {
            _recorder = recorder;
            _component = component;
            _operation = operation;
            _sw = sw;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _sw.Stop();
            _recorder.RecordComponentExited(_component, _operation, null, _explicitError ?? "ok", durationMs: _sw.ElapsedMilliseconds);
        }

        public void ExitWithError(string errorMessage)
        {
            if (_disposed) return;
            _explicitError = errorMessage;
            _disposed = true;
            _sw.Stop();
            _recorder.RecordComponentExited(_component, _operation, null, "error", errorMessage, _sw.ElapsedMilliseconds);
        }
    }
}
