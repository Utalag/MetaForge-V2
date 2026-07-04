using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches;

/// <summary>
/// Abstrakce pro patch operaci na BusinessAuthoringDocument.
/// </summary>
public interface IPatchOperation
{
    /// <summary>Typ commandu pro CommandLog.</summary>
    string CommandType { get; }

    /// <summary>Provede mutaci na dokumentu.</summary>
    void Apply(BusinessAuthoringDocument document);

    /// <summary>Vytvoří CommandEnvelope pro záznam do logu.</summary>
    CommandEnvelope ToEnvelope();
}
