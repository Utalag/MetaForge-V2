namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Stav synchronizace mezi business vrstvou a Core vrstvou.
/// Sleduje, zda je CoreDetail aktuální vůči business atributu.
/// </summary>
public enum AttributeSyncState
{
    /// <summary>Nově vytvořený atribut — ještě nebyl synchronizován s Core.</summary>
    New = 0,

    /// <summary>Business a Core jsou synchronizované — žádné změny.</summary>
    Synced = 1,

    /// <summary>Business atribut byl upraven — CoreDetail je neaktuální.</summary>
    BusinessEdited = 2,

    /// <summary>CoreDetail byl upraven (např. AI enrichment) — business atribut je neaktuální.</summary>
    CoreEdited = 3,

    /// <summary>Business i Core byly upraveny nezávisle — vzniknul konflikt.</summary>
    Conflict = 4,
}
