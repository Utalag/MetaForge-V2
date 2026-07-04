namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Append-only úložiště commandů.
/// INVARIANT: Count nikdy neklesá. Commandy se nikdy nemažou ani nepřepisují.
/// </summary>
public sealed class CommandLogStore
{
    private readonly List<CommandEnvelope> _commands = new();

    /// <summary>Počet commandů v logu. Nikdy neklesá.</summary>
    public int Count => _commands.Count;

    /// <summary>
    /// Přidá command na konec logu.
    /// Každé volání zvyšuje Count o 1.
    /// </summary>
    public void Append(CommandEnvelope envelope)
    {
        _commands.Add(envelope);
    }

    /// <summary>Vrátí všechny commandy v pořadí vložení (pro replay).</summary>
    public IReadOnlyList<CommandEnvelope> GetAll() =>
        _commands.AsReadOnly();

    /// <summary>Vrátí command na daném indexu.</summary>
    public CommandEnvelope? GetAt(int index) =>
        index >= 0 && index < _commands.Count ? _commands[index] : null;

    /// <summary>Vrátí všechny commandy od daného indexu (pro inkrementální replay).</summary>
    public IReadOnlyList<CommandEnvelope> GetFrom(int startIndex) =>
        _commands.Skip(startIndex).ToList().AsReadOnly();
}
