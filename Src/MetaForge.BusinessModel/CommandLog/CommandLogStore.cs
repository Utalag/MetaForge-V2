namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Append-only úložiště commandů — thread-safe.
/// INVARIANT: Count nikdy neklesá. Commandy se nikdy nemažou ani nepřepisují.
/// Podporuje idempotenci přes MutationId.
/// </summary>
public sealed class CommandLogStore
{
    private readonly List<CommandEnvelope> _commands = new();
    private readonly HashSet<string> _appliedMutationIds = new();
    private readonly object _lock = new();

    /// <summary>Počet commandů v logu. Nikdy neklesá.</summary>
    public int Count { get { lock (_lock) return _commands.Count; } }

    /// <summary>
    /// Přidá command na konec logu.
    /// Každé volání zvyšuje Count o 1.
    /// </summary>
    public void Append(CommandEnvelope envelope)
    {
        TryAppend(envelope);
    }

    /// <summary>
    /// Přidá command na konec logu s kontrolou idempotence.
    /// Pokud <see cref="CommandEnvelope.MutationId"/> již existuje, command se ignoruje.
    /// </summary>
    /// <returns>true pokud byl command přidán, false pokud již existuje (idempotence).</returns>
    public bool TryAppend(CommandEnvelope envelope)
    {
        lock (_lock)
        {
            if (envelope.MutationId is not null && !_appliedMutationIds.Add(envelope.MutationId))
                return false; // Idempotentní — již aplikováno

            _commands.Add(envelope);
            return true;
        }
    }

    /// <summary>Vrátí všechny commandy v pořadí vložení (pro replay).</summary>
    public IReadOnlyList<CommandEnvelope> GetAll()
    {
        lock (_lock)
        {
            return _commands.ToList().AsReadOnly();
        }
    }

    /// <summary>Vrátí command na daném indexu.</summary>
    public CommandEnvelope? GetAt(int index)
    {
        lock (_lock)
        {
            return index >= 0 && index < _commands.Count ? _commands[index] : null;
        }
    }

    /// <summary>Vrátí všechny commandy od daného indexu (pro inkrementální replay).</summary>
    public IReadOnlyList<CommandEnvelope> GetFrom(int startIndex)
    {
        lock (_lock)
        {
            return _commands.Skip(startIndex).ToList().AsReadOnly();
        }
    }
}
