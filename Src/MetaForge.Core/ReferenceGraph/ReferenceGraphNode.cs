// ---------------------------------------------------------------------------
// MetaForge.Core — ReferenceGraphNode
// A single node in the dependency graph.
// Vrstva: Core / ReferenceGraph
//
// PROPOSAL: PROP-055 — ReferenceGraph (ID-based)
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;

namespace MetaForge.Core.ReferenceGraph;

/// <summary>
/// Uzel v grafu závislostí. Reprezentuje jeden RootElement a jeho reference.
/// Používá <see cref="Guid"/> ElementId pro stabilní identifikaci (PROP-060).
/// </summary>
public sealed record ReferenceGraphNode
{
    /// <summary>Stabilní identifikátor elementu (Guid z Core, PROP-060).</summary>
    public Guid ElementId { get; init; }

    /// <summary>Lidsky čitelný název pro debug a diagnostiku.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Typ elementu (Class, Interface, Enum, Struct, ...).</summary>
    public string ElementKind { get; init; } = string.Empty;

    /// <summary>Odkaz na element. Null = ID existuje v referencích, ale ne v modelu (unresolved).</summary>
    public RootElement? Element { get; init; }

    /// <summary>ID elementů, na které tento element odkazuje.</summary>
    public IReadOnlyList<Guid> References { get; init; } = [];
}
