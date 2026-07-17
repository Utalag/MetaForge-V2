// ---------------------------------------------------------------------------
// MetaForge.BusinessModel — SyncState
// Typed record-state machine for synchronization between BusinessModel and Core.
// Replaces enum AttributeSyncState with exhaustive-switch safety.
// Vrstva: BusinessModel / Models
//
// PROPOSAL: PROP-060 (extracted from PROP-023 #1)
// ---------------------------------------------------------------------------

namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Typovaný stav synchronizace mezi BusinessModel a Core vrstvou.
/// Nahrazuje <see cref="AttributeSyncState"/> enum (PROP-020).
/// Každý přechod je explicitní metoda — kompilátor hlídá exhaustivní pokrytí.
/// 
/// Použití:
/// <code>
///   var state = new SyncState.New();
///   state = state.OnBusinessEdit();  // → BusinessEdited
///   state = state.OnCoreEdit(id);    // → Conflict (both edited)
///   state = state.OnSyncResolved(id); // → Synced
/// </code>
/// </summary>
public abstract record SyncState
{
    /// <summary>Nový element, ještě nesynchronizovaný.</summary>
    public sealed record New : SyncState;

    /// <summary>Synchronizováno — BusinessModel a Core jsou ve shodě.</summary>
    public sealed record Synced(DateTimeOffset SyncedAt, Guid CoreElementId) : SyncState;

    /// <summary>Uživatel upravil BusinessModel — čeká na nový překlad.</summary>
    public sealed record BusinessEdited(SyncState Previous) : SyncState;

    /// <summary>Core bylo obohaceno (AI enrichment) — čeká na write-back.</summary>
    public sealed record CoreEdited(SyncState Previous) : SyncState;

    /// <summary>Konflikt — obě strany se změnily nezávisle.</summary>
    public sealed record Conflict(string Reason, SyncState Business, SyncState Core) : SyncState;

    // === Explicitní přechody ===

    /// <summary>Přechod při úpravě BusinessModel (uživatel).</summary>
    public SyncState OnBusinessEdit() => this switch
    {
        Synced s => new BusinessEdited(s),
        CoreEdited c => new Conflict("both edited", new BusinessEdited(c.Previous), c),
        Conflict => this,
        _ => this,
    };

    /// <summary>Přechod při obohacení Core (AI enrichment / Translator).</summary>
    public SyncState OnCoreEdit(Guid newCoreId) => this switch
    {
        Synced s => new CoreEdited(s),
        BusinessEdited b => new Conflict("both edited", b, new CoreEdited(b.Previous)),
        Conflict => this,
        _ => this,
    };

    /// <summary>Přechod při vyřešení — obě strany synchronizovány.</summary>
    public SyncState OnSyncResolved(Guid coreId) => this switch
    {
        BusinessEdited => new Synced(DateTimeOffset.UtcNow, coreId),
        CoreEdited => new Synced(DateTimeOffset.UtcNow, coreId),
        Conflict => new Synced(DateTimeOffset.UtcNow, coreId),
        _ => this,
    };

    /// <summary>Převede starý <see cref="AttributeSyncState"/> enum na typovaný SyncState.</summary>
    public static SyncState FromLegacy(AttributeSyncState legacyState, Guid? coreId = null)
    {
        return legacyState switch
        {
            AttributeSyncState.New => new New(),
            AttributeSyncState.Synced => new Synced(DateTimeOffset.UtcNow, coreId ?? Guid.Empty),
            AttributeSyncState.BusinessEdited => new BusinessEdited(new Synced(DateTimeOffset.UtcNow, coreId ?? Guid.Empty)),
            AttributeSyncState.CoreEdited => new CoreEdited(new Synced(DateTimeOffset.UtcNow, coreId ?? Guid.Empty)),
            AttributeSyncState.Conflict => new Conflict("legacy migration", new New(), new New()),
            _ => new New(),
        };
    }
}
