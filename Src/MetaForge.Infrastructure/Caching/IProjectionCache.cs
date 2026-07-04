using MetaForge.BusinessModel.Models;

namespace MetaForge.Infrastructure.Caching;

/// <summary>
/// Abstrakce pro projekční cache — snapshoty dokumentu pro rychlý replay.
/// </summary>
public interface IProjectionCache
{
    /// <summary>
    /// Pokusí se načíst dokument z posledního checkpointu a rekonstruovat stav.
    /// Vrací null pokud checkpoint neexistuje.
    /// </summary>
    Task<BusinessAuthoringDocument?> TryGetFromCheckpointAsync(CancellationToken ct = default);

    /// <summary>
    /// Uloží checkpoint dokumentu po daném počtu commandů.
    /// </summary>
    Task SaveCheckpointAsync(BusinessAuthoringDocument document, int commandIndex, CancellationToken ct = default);

    /// <summary>
    /// Vrátí poslední checkpoint (pro diagnostiku).
    /// </summary>
    BusinessProjectionCheckpoint? GetLatestCheckpoint();
}
