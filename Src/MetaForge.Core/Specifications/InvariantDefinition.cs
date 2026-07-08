// ---------------------------------------------------------------------------
// MetaForge.Core — InvariantDefinition
// First-class specification artifact: a declarative, serialisable invariant rule.
// Vrstva: Core / Specifications
// 
// PROPOSAL: PROP-036 — Core Specification Layer
// ---------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace MetaForge.Core.Specifications;

/// <summary>
/// Declarative invariant rule that can be evaluated against a target element.
/// Forms the single source of truth for validity constraints.
/// 
/// An invariant says: WHEN (condition is met) THEN MUST (condition holds).
/// Violation = When evaluates true AND Must evaluates false.
/// </summary>
public sealed record InvariantDefinition
{
    /// <summary>Unique invariant code (e.g., "MF_METHOD_001").</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The kind of element this invariant targets.
    /// Should match the element's Kind string (e.g. "MethodElement", "ClassElement").
    /// </summary>
    public string TargetKind { get; init; } = string.Empty;

    /// <summary>Human-readable description of what this invariant enforces.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Severity of a violation.</summary>
    public InvariantSeverity Severity { get; init; } = InvariantSeverity.Error;

    /// <summary>Scope of evaluation context needed.</summary>
    public InvariantScope Scope { get; init; } = InvariantScope.Local;

    /// <summary>Condition that must be true for this invariant to be checked. Null = always check.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InvariantExpression? When { get; init; }

    /// <summary>Condition that must hold true when this invariant is active. Null = vacuously true.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InvariantExpression? Must { get; init; }

    /// <summary>Optional hint for test generation (valid/invalid generators).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeneratorIntent? GeneratorIntent { get; init; }

    /// <summary>Provenance metadata: who/what created this invariant.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InvariantProvenance? Provenance { get; init; }

    /// <summary>Arbitrary metadata bag for extensibility.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Metadata { get; init; }

    /// <summary>Creates a simple invariant that always applies (no When clause).</summary>
    public static InvariantDefinition Always(
        string code,
        string targetKind,
        string description,
        InvariantExpression must,
        InvariantSeverity severity = InvariantSeverity.Error,
        InvariantScope scope = InvariantScope.Local) => new()
        {
            Code = code,
            TargetKind = targetKind,
            Description = description,
            Severity = severity,
            Scope = scope,
            Must = must
        };

    /// <summary>Creates a conditional invariant that only applies when the When clause is true.</summary>
    public static InvariantDefinition WhenCondition(
        string code,
        string targetKind,
        string description,
        InvariantExpression when,
        InvariantExpression must,
        InvariantSeverity severity = InvariantSeverity.Error,
        InvariantScope scope = InvariantScope.Local) => new()
        {
            Code = code,
            TargetKind = targetKind,
            Description = description,
            Severity = severity,
            Scope = scope,
            When = when,
            Must = must
        };
}

/// <summary>
/// Hints for test generation from an invariant.
/// </summary>
public sealed record GeneratorIntent
{
    /// <summary>Whether valid-element generation should avoid triggering this constraint.</summary>
    public bool AvoidInValidGenerator { get; init; } = true;

    /// <summary>Whether an explicit invalid-generator test should be created for this invariant.</summary>
    public bool GenerateInvalidTest { get; init; } = true;

    /// <summary>Priority for test generation ordering.</summary>
    public int TestPriority { get; init; } = 0;
}

/// <summary>
/// Provenance metadata for AI-generated or user-defined invariants.
/// </summary>
public sealed record InvariantProvenance
{
    /// <summary>Who/what created the invariant: "User", "AI", "System", etc.</summary>
    public string Source { get; init; } = "User";

    /// <summary>When the invariant was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>If AI-generated: the prompt that produced it.</summary>
    public string? Prompt { get; init; }

    /// <summary>If AI-generated: model version identifier.</summary>
    public string? ModelVersion { get; init; }

    /// <summary>Number of times this invariant has flagged false positives.</summary>
    public int FalsePositiveCount { get; init; }
}
