namespace MetaForge.Core.Diagnostics;

/// <summary>
/// Monadický wrapper nad výsledkem fáze pipeline.
/// Nese hodnotu a diagnostický bag. Při chybě lze přerušit pipeline.
/// </summary>
public sealed record BuildResult<T>
{
    /// <summary>Hodnota — může být default při chybě.</summary>
    public T Value { get; }

    /// <summary>Diagnostické zprávy za tuto fázi.</summary>
    public DiagnosticBag Bag { get; }

    /// <summary>Proběhla fáze bez chyby?</summary>
    public bool IsSuccess => !Bag.HasErrors;

    /// <summary>
    /// Vytvoří úspěšný výsledek.
    /// </summary>
    public BuildResult(T value, DiagnosticBag? bag = null)
    {
        Value = value;
        Bag = bag ?? new DiagnosticBag();
    }

    /// <summary>
    /// Vytvoří výsledek z chyby.
    /// </summary>
    public static BuildResult<T> Failure(DiagnosticBag bag) => new(default!, bag);
    public static BuildResult<T> Failure(string code, string message, ElementPath location)
    {
        var bag = new DiagnosticBag();
        bag.Report(new Diagnostic(code, message, DiagnosticSeverity.Error, location));
        return new BuildResult<T>(default!, bag);
    }

    /// <summary>
    /// Monadické chainování — naváže další fázi jen při úspěchu.
    /// Při chybě propaguje chybový stav.
    /// </summary>
    public BuildResult<TOut> Then<TOut>(Func<T, BuildResult<TOut>> next)
    {
        if (!IsSuccess)
        {
            var errorBag = new DiagnosticBag();
            errorBag.Merge(Bag);
            return new BuildResult<TOut>(default!, errorBag);
        }
        return next(Value);
    }

    /// <summary>
    /// Mapování — transformuje hodnotu bez změny diagnostiky.
    /// </summary>
    public BuildResult<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess
            ? new BuildResult<TOut>(mapper(Value), Bag)
            : new BuildResult<TOut>(default!, Bag);
}
