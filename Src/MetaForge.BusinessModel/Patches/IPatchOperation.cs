using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches;

/// <summary>
/// Abstrakce pro patch operaci na BusinessAuthoringDocument.
/// Operace vrací nový dokument — nikdy nemutuje původní (immutable pattern).
/// </summary>
public interface IPatchOperation
{
    /// <summary>Typ commandu pro CommandLog.</summary>
    string CommandType { get; }

    /// <summary>
    /// Provede operaci na dokumentu a vrátí novou instanci.
    /// Původní dokument zůstává nezměněn (immutable).
    /// </summary>
    BusinessAuthoringDocument Apply(BusinessAuthoringDocument document);

    /// <summary>Vytvoří CommandEnvelope pro záznam do logu.</summary>
    CommandEnvelope ToEnvelope();
}
