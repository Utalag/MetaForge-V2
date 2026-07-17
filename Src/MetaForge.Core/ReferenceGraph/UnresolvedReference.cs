// ---------------------------------------------------------------------------
// MetaForge.Core — UnresolvedReference
// A reference to a type that doesn't exist in the model.
// Vrstva: Core / ReferenceGraph
//
// PROPOSAL: PROP-055 — ReferenceGraph
// ---------------------------------------------------------------------------

namespace MetaForge.Core.ReferenceGraph;

/// <summary>
/// Nevyřešená reference — odkaz na typ, který neexistuje v modelu.
/// Nese informaci o zdroji, cíli, typu reference a kontextu.
/// </summary>
public sealed record UnresolvedReference
{
    /// <summary>ID elementu, který referenci drží.</summary>
    public Guid SourceElementId { get; init; }

    /// <summary>Název zdrojového elementu pro diagnostiku.</summary>
    public string SourceDisplayName { get; init; } = string.Empty;

    /// <summary>ID, na které reference ukazuje (neexistuje v modelu).</summary>
    public Guid TargetId { get; init; }

    /// <summary>Jak se reference jmenuje v kódu (např. "Property 'Manager'").</summary>
    public string ReferencedAs { get; init; } = string.Empty;

    /// <summary>Typ reference.</summary>
    public ReferenceKind Kind { get; init; }
}
