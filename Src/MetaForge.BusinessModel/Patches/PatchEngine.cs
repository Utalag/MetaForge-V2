using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches;

/// <summary>
/// Atomické mutace BusinessAuthoringDocument.
/// Každá mutace vytváří CommandEnvelope a zapisuje do CommandLog.
/// Používá immutable pattern — vrací nový dokument, nemutuje původní.
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
    /// Vrací novou instanci dokumentu — původní zůstává nezměněn.
    /// </summary>
    /// <exception cref="ArgumentNullException">Pokud document nebo operation je null.</exception>
    public BusinessAuthoringDocument Apply(BusinessAuthoringDocument document, IPatchOperation operation)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(operation);

        // 1. Aplikuj operaci — získáme nový dokument
        var newDocument = operation.Apply(document);

        // 2. Vytvoř a zapiš command
        var envelope = operation.ToEnvelope();

        // Použij TryAppend pro idempotenci
        _logStore.TryAppend(envelope);

        // 3. Aktualizuj čas modifikace na novém dokumentu
        newDocument = newDocument with { LastModified = envelope.Timestamp };

        return newDocument;
    }

    /// <summary>Vytvoří CommandEnvelope pro danou operaci (bez aplikace).</summary>
    public CommandEnvelope CreateEnvelope(IPatchOperation operation) =>
        operation.ToEnvelope();
}
