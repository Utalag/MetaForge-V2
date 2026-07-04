namespace MetaForge.Generators;

/// <summary>
/// Diagnostická informace — varování nebo chyba při generování.
/// </summary>
public sealed record DiagnosticInfo(
    string Message,
    DiagnosticSeverity Severity = DiagnosticSeverity.Warning,
    string? ElementId = null,
    string? ElementName = null
);

public enum DiagnosticSeverity
{
    Warning,
    Error,
}
