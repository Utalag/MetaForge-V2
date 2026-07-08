namespace MetaForge.Core.Diagnostics;

/// <summary>
/// Úroveň závažnosti diagnostické zprávy.
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>Skrytá zpráva — jen pro interní tooling.</summary>
    Hidden = 0,

    /// <summary>Informační — neblokující.</summary>
    Info = 1,

    /// <summary>Varování — potenciální problém.</summary>
    Warning = 2,

    /// <summary>Chyba — blokující, pipeline se zastaví.</summary>
    Error = 3,
}

/// <summary>
/// Přesná pozice v modelu — root element, element, volitelný segment.
/// </summary>
public sealed record ElementPath(
    string Root,
    string Element,
    string? Segment = null,
    string? Subsegment = null
);

/// <summary>
/// Jedna diagnostická zpráva.
/// </summary>
public sealed record Diagnostic(
    string Code,
    string Message,
    DiagnosticSeverity Severity,
    ElementPath Location,
    string? HelpUrl = null,
    string? SuggestedFix = null
);

/// <summary>
/// Sběr diagnostických zpráv během fáze pipeline.
/// </summary>
public interface IDiagnosticCollector
{
    void Report(Diagnostic diagnostic);
    bool HasErrors { get; }
    IReadOnlyList<Diagnostic> ToReadOnly();
}
