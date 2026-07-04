using System.Text.Json;
using MetaForge.BusinessModel.Models;
using MetaForge.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace MetaForge.Infrastructure.Caching;

/// <summary>
/// Implementace projekční cache pomocí checkpoint souborů.
/// Ukládá snapshot dokumentu po každých N commandech pro rychlý replay.
/// </summary>
public sealed class CheckpointProjectionCache : IProjectionCache
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _checkpointDir;
    private BusinessProjectionCheckpoint? _latest;

    /// <summary>
    /// Vytvoří checkpoint cache s cestou z konfigurace.
    /// </summary>
    public CheckpointProjectionCache(IOptions<StorageOptions> options)
    {
        _checkpointDir = options.Value.CheckpointPath;
        Directory.CreateDirectory(_checkpointDir);
    }

    /// <inheritdoc />
    public async Task<BusinessAuthoringDocument?> TryGetFromCheckpointAsync(CancellationToken ct = default)
    {
        if (_latest is null)
        {
            _latest = await LoadLatestCheckpointAsync(ct);
            if (_latest is null) return null;
        }

        var document = JsonSerializer.Deserialize<BusinessAuthoringDocument>(_latest.DocumentJson, JsonOptions);
        return document;
    }

    /// <inheritdoc />
    public async Task SaveCheckpointAsync(BusinessAuthoringDocument document, int commandIndex, CancellationToken ct = default)
    {
        var checkpoint = new BusinessProjectionCheckpoint
        {
            CommandIndex = commandIndex,
            DocumentJson = JsonSerializer.Serialize(document, JsonOptions),
            CreatedAt = DateTimeOffset.UtcNow,
            SchemaVersion = document.SchemaVersion,
        };

        var filePath = Path.Combine(_checkpointDir, $"checkpoint-{commandIndex:D8}.json");
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(checkpoint, JsonOptions), ct);
        _latest = checkpoint;
    }

    /// <inheritdoc />
    public BusinessProjectionCheckpoint? GetLatestCheckpoint() => _latest;

    /// <summary>
    /// Načte nejnovější checkpoint z disku.
    /// </summary>
    private async Task<BusinessProjectionCheckpoint?> LoadLatestCheckpointAsync(CancellationToken ct)
    {
        if (!Directory.Exists(_checkpointDir)) return null;

        var files = Directory.GetFiles(_checkpointDir, "checkpoint-*.json");
        if (files.Length == 0) return null;

        // Seřadit podle názvu (checkpoint-XXXXXXXX.json) — poslední je nejnovější
        var latestFile = files.OrderByDescending(f => f).First();
        var json = await File.ReadAllTextAsync(latestFile, ct);
        return JsonSerializer.Deserialize<BusinessProjectionCheckpoint>(json, JsonOptions);
    }
}
