using MetaForge.BusinessModel.CommandLog;

namespace MetaForge.Infrastructure.Persistence;

/// <summary>
/// In-memory implementace ICommandLogRepository — pro testy a sandbox.
/// Data nepřežijí restart procesu. Thread-safe.
/// </summary>
public sealed class InMemoryCommandLogRepository : ICommandLogRepository
{
    private readonly List<CommandEnvelope> _commands = new();
    private readonly object _lock = new();

    /// <inheritdoc />
    public Task AppendAsync(CommandEnvelope envelope, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_lock)
        {
            _commands.Add(envelope);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<CommandEnvelope>> LoadAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<CommandEnvelope>>(_commands.ToList().AsReadOnly());
        }
    }

    /// <inheritdoc />
    public Task<int> GetCountAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_commands.Count);
        }
    }
}
