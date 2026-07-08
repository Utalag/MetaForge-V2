// ---------------------------------------------------------------------------
// MetaForge.Core — EvaluationResult
// Result of evaluating invariants against a target element.
// Vrstva: Core / Specifications
// 
// PROPOSAL: PROP-036 — Core Specification Layer
// ---------------------------------------------------------------------------

namespace MetaForge.Core.Specifications;

/// <summary>
/// Result of evaluating a set of invariants against a target element.
/// </summary>
public sealed record EvaluationResult
{
    /// <summary>Whether all invariants passed (no violations at Error or Fatal severity).</summary>
    public bool IsValid => Violations.Count(v => v.Severity >= InvariantSeverity.Error) == 0;

    /// <summary>All violations found, ordered by severity (Fatal first) then by code.</summary>
    public IReadOnlyList<InvariantViolation> Violations { get; init; } = Array.Empty<InvariantViolation>();

    /// <summary>Number of invariants evaluated.</summary>
    public int TotalEvaluated { get; init; }

    /// <summary>Wall-clock time taken for evaluation.</summary>
    public TimeSpan EvaluationTime { get; init; }

    /// <summary>Creates a successful result with no violations.</summary>
    public static EvaluationResult Success(int totalEvaluated, TimeSpan elapsed) => new()
    {
        Violations = Array.Empty<InvariantViolation>(),
        TotalEvaluated = totalEvaluated,
        EvaluationTime = elapsed
    };

    /// <summary>Creates a result with violations.</summary>
    public static EvaluationResult Failed(
        IReadOnlyList<InvariantViolation> violations,
        int totalEvaluated,
        TimeSpan elapsed) => new()
        {
            Violations = violations,
            TotalEvaluated = totalEvaluated,
            EvaluationTime = elapsed
        };
}

/// <summary>
/// A single invariant violation — which invariant, on what element, and why.
/// </summary>
public sealed record InvariantViolation
{
    /// <summary>Code of the violated invariant (e.g., "MF_METHOD_001").</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Human-readable description of the violated invariant.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Severity of the violation.</summary>
    public InvariantSeverity Severity { get; init; } = InvariantSeverity.Error;

    /// <summary>Scope of the violated invariant.</summary>
    public InvariantScope Scope { get; init; } = InvariantScope.Local;

    /// <summary>Path to the offending element (e.g., "Project.MyApp/Models/Customer").</summary>
    public string ElementPath { get; init; } = string.Empty;

    /// <summary>Human-readable message explaining the violation in context.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Creates a violation with a formatted message.</summary>
    public static InvariantViolation Create(
        InvariantDefinition invariant,
        string elementPath,
        string? detail = null) => new()
        {
            Code = invariant.Code,
            Description = invariant.Description,
            Severity = invariant.Severity,
            Scope = invariant.Scope,
            ElementPath = elementPath,
            Message = detail != null
                ? $"[{invariant.Code}] {invariant.Description} — {detail} (at {elementPath})"
                : $"[{invariant.Code}] {invariant.Description} (at {elementPath})"
        };
}
