namespace MetaForge.Core.Diagnostics;

/// <summary>
/// Reportér diagnostických zpráv do výstupního kanálu.
/// </summary>
public interface IDiagnosticReporter
{
    /// <summary>Vypíše diagnostické zprávy.</summary>
    void Report(IReadOnlyList<Diagnostic> diagnostics);
}

/// <summary>
/// Vypisuje diagnostiku na konzoli (stdout/stderr).
/// </summary>
public sealed class ConsoleDiagnosticReporter : IDiagnosticReporter
{
    public bool UseColors { get; set; } = true;

    public void Report(IReadOnlyList<Diagnostic> diagnostics)
    {
        foreach (var d in diagnostics)
        {
            var label = d.Severity switch
            {
                DiagnosticSeverity.Error => "ERR",
                DiagnosticSeverity.Warning => "WRN",
                DiagnosticSeverity.Info => "INF",
                _ => "DBG",
            };
            var msg = $"[{label}] {d.Code}: {d.Message} ({d.Location.Root}/{d.Location.Element})";
            if (d.Severity == DiagnosticSeverity.Error)
                Console.Error.WriteLine(msg);
            else
                Console.WriteLine(msg);
        }
    }
}

/// <summary>
/// Vypisuje diagnostiku jako JSON pole.
/// </summary>
public sealed class JsonDiagnosticReporter : IDiagnosticReporter
{
    public void Report(IReadOnlyList<Diagnostic> diagnostics)
    {
        var items = diagnostics.Select(d => new
        {
            code = d.Code,
            message = d.Message,
            severity = d.Severity.ToString(),
            location = new
            {
                root = d.Location.Root,
                element = d.Location.Element,
                segment = d.Location.Segment,
                subsegment = d.Location.Subsegment,
            },
            helpUrl = d.HelpUrl,
            suggestedFix = d.SuggestedFix,
        });
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(items));
    }
}

/// <summary>
/// Uchovává diagnostiku v paměti pro programové zpracování.
/// </summary>
public sealed class InMemoryDiagnosticReporter : IDiagnosticReporter
{
    private readonly DiagnosticBag _bag = new();

    public DiagnosticBag Bag => _bag;

    public void Report(IReadOnlyList<Diagnostic> diagnostics)
    {
        foreach (var d in diagnostics)
            _bag.Report(d);
    }
}
