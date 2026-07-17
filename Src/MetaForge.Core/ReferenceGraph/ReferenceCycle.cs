// ---------------------------------------------------------------------------
// MetaForge.Core — ReferenceCycle
// A detected cycle in the dependency graph.
// Vrstva: Core / ReferenceGraph
//
// PROPOSAL: PROP-055 — ReferenceGraph
// ---------------------------------------------------------------------------

namespace MetaForge.Core.ReferenceGraph;

/// <summary>
/// Detekovaný cyklus v grafu závislostí.
/// Obsahuje jak strojové ID, tak lidsky čitelné názvy pro diagnostiku.
/// </summary>
public sealed record ReferenceCycle
{
    /// <summary>ID elementů v cyklu (v pořadí závislosti).</summary>
    public IReadOnlyList<Guid> ElementIds { get; init; } = [];

    /// <summary>Lidsky čitelné názvy pro diagnostiku.</summary>
    public IReadOnlyList<string> DisplayNames { get; init; } = [];

    /// <summary>Formátovaný výstup: "A → B → A"</summary>
    public override string ToString() => string.Join(" → ", DisplayNames) + " → " + DisplayNames[0];
}
