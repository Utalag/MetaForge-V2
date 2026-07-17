// ---------------------------------------------------------------------------
// MetaForge.Core — ElementContract
// Base type for semantic contracts of elements.
// Vrstva: Core / Contracts
//
// PROPOSAL: PROP-057 — ElementContract + VerificationModel
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.Diagnostics;

namespace MetaForge.Core.Contracts;

/// <summary>
/// Sémantický kontrakt elementu.
/// Zobecňuje StrongType pattern: element nese svůj význam, invarianty a scénáře.
/// ⚠️ NEDuplikuje constrainty typů/property. Ty patří StrongType / PropertyElement.
/// </summary>
public abstract record ElementContract
{
    /// <summary>Stabilní ID elementu (PROP-060).</summary>
    public string ElementId { get; init; } = string.Empty;

    /// <summary>Cross-property / cross-parameter invarianty.</summary>
    public IReadOnlyList<ContractInvariant> Invariants { get; init; } = [];

    /// <summary>Unifikované scénáře — validní i nevalidní příklady.</summary>
    public IReadOnlyList<ContractScenario> Scenarios { get; init; } = [];

    /// <summary>Volitelná metadata.</summary>
    public MetadataBag Metadata { get; init; } = new();
}

/// <summary>
/// Odkaz na invariant definovaný v <c>InvariantDefinition</c> (PROP-036).
/// </summary>
public record ContractInvariant
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    /// <summary>Odkaz na InvariantDefinition (PROP-036).</summary>
    public string? InvariantDefinitionId { get; init; }
    public DiagnosticSeverity Severity { get; init; } = DiagnosticSeverity.Error;
}
