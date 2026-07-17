// ---------------------------------------------------------------------------
// MetaForge.Core — EntityContract
// Semantic contract for entities (ClassElement).
// Vrstva: Core / Contracts
//
// PROPOSAL: PROP-057 — ElementContract + VerificationModel
// ---------------------------------------------------------------------------

namespace MetaForge.Core.Contracts;

/// <summary>
/// Kontrakt pro entitu (ClassElement).
/// ⚠️ NEObsahuje PropertyRule! Constrainty jednotlivých property patří StrongType / PropertyElement.
/// Nese jen cross-property invarianty, vztahové constrainty, a scénáře.
/// </summary>
public sealed record EntityContract : ElementContract
{
    /// <summary>Vztahové constrainty (např. "entita musí mít alespoň 1 atribut").</summary>
    public IReadOnlyList<RelationConstraint> RelationConstraints { get; init; } = [];
}

/// <summary>
/// Vztahový constraint mezi elementy.
/// </summary>
public record RelationConstraint
{
    public string RelationId { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string Description { get; init; } = string.Empty;
}
