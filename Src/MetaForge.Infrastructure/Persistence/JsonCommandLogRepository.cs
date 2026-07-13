using System.Text.Json;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Threading;

namespace MetaForge.Infrastructure.Persistence;

/// <summary>
/// JSONL implementace ICommandLogRepository — append-only soubor, každý command na jeden řádek.
/// Thread-safe pro zápis (lock), bezpečné pro paralelní čtení.
/// </summary>
public sealed class JsonCommandLogRepository : ICommandLogRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false, // Jeden řádek na command
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _filePath;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    /// <summary>
    /// Vytvoří JSONL repository s cestou z konfigurace.
    /// </summary>
    public JsonCommandLogRepository(IOptions<StorageOptions> options)
    {
        _filePath = options.Value.CommandLogPath;
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    /// <inheritdoc />
    public async Task AppendAsync(CommandEnvelope envelope, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var line = JsonSerializer.Serialize(envelope, JsonOptions);

        await _writeLock.WaitAsync(ct);
        try
        {
            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, ct);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CommandEnvelope>> LoadAllAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath)) return Array.Empty<CommandEnvelope>();

        var commands = new List<CommandEnvelope>();
        var lines = await File.ReadAllLinesAsync(_filePath, ct);
        foreach (var line in lines)
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(line)) continue;
            var envelope = JsonSerializer.Deserialize<CommandEnvelope>(line, JsonOptions);
            if (envelope is not null) commands.Add(envelope);
        }
        return commands.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath)) return 0;
        var lines = await File.ReadAllLinesAsync(_filePath, ct);
        return lines.Count(l => !string.IsNullOrWhiteSpace(l));
    }
}
