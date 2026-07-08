// ---------------------------------------------------------------------------
// MetaForge.Core — InvariantSeverity
// Severity level of an invariant violation.
// Vrstva: Core / Specifications
// ---------------------------------------------------------------------------

namespace MetaForge.Core.Specifications;

/// <summary>
/// Severity of an invariant violation.
/// </summary>
public enum InvariantSeverity
{
    /// <summary>Informational only — does not block generation.</summary>
    Info = 0,

    /// <summary>Warning — generation may proceed but result may be degraded.</summary>
    Warning = 1,

    /// <summary>Error — blocks generation. Must be fixed before code emit.</summary>
    Error = 2,

    /// <summary>Fatal — internal inconsistency. Indicates a bug in MetaForge itself.</summary>
    Fatal = 3
}
