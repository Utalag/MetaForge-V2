using MetaForge.BusinessModel.Models;

namespace MetaForge.Infrastructure.Persistence;

/// <summary>
/// Abstrakce pro persistentní úložiště dokumentu (snapshot).
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Uloží snapshot dokumentu na disk.
    /// </summary>
    Task SaveAsync(BusinessAuthoringDocument document, CancellationToken ct = default);

    /// <summary>
    /// Načte snapshot dokumentu z disku.
    /// Vrací null pokud soubor neexistuje.
    /// </summary>
    Task<BusinessAuthoringDocument?> LoadAsync(CancellationToken ct = default);
}
