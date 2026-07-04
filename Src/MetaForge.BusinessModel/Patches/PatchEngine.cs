using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches;

/// <summary>
/// Atomické mutace BusinessAuthoringDocument.
/// Každá mutace vytváří CommandEnvelope a zapisuje do CommandLog.
/// </summary>
public sealed class PatchEngine
{
    private readonly CommandLogStore _logStore;

    public PatchEngine(CommandLogStore logStore)
    {
        _logStore = logStore;
    }

    /// <summary>
    /// Aplikuje patch operaci na dokument a zaznamená do logu.
    /// </summary>
    /// <exception cref="ArgumentNullException">Pokud document nebo operation je null.</exception>
    public void Apply(BusinessAuthoringDocument document, IPatchOperation operation)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(operation);

        // 1. Aplikuj mutaci
        operation.Apply(document);

        // 2. Vytvoř a zapiš command
        var envelope = operation.ToEnvelope();
        _logStore.Append(envelope);

        // 3. Aktualizuj čas modifikace
        document.LastModified = envelope.Timestamp;
    }

    /// <summary>Vytvoří CommandEnvelope pro danou operaci (bez aplikace).</summary>
    public CommandEnvelope CreateEnvelope(IPatchOperation operation) =>
        operation.ToEnvelope();
}
