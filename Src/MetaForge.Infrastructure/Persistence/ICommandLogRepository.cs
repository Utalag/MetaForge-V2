using MetaForge.BusinessModel.CommandLog;

namespace MetaForge.Infrastructure.Persistence;

/// <summary>
/// Abstrakce pro persistentní úložiště command logu.
/// Implementace: JSONL soubor, InMemory (pro testy), budoucí DB.
/// </summary>
public interface ICommandLogRepository
{
    /// <summary>
    /// Připojí command na konec logu (append-only).
    /// </summary>
    Task AppendAsync(CommandEnvelope envelope, CancellationToken ct = default);

    /// <summary>
    /// Načte všechny commandy z logu v pořadí vložení.
    /// </summary>
    Task<IReadOnlyList<CommandEnvelope>> LoadAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Vrátí počet commandů v logu.
    /// </summary>
    Task<int> GetCountAsync(CancellationToken ct = default);
}
