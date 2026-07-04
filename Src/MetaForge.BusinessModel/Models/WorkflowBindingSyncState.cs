namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Stav synchronizace workflow bindingu s Core vrstvou.
/// </summary>
public enum WorkflowBindingSyncState
{
    /// <summary>Nový binding — ještě nesynchronizován.</summary>
    New = 0,

    /// <summary>Synchronizováno — binding odpovídá Core.</summary>
    Synced = 1,

    /// <summary>Binding upraven — Core je neaktuální.</summary>
    Edited = 2,

    /// <summary>Konflikt — business i Core byly upraveny nezávisle.</summary>
    Conflict = 3,
}
