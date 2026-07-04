using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

public sealed class BusinessProjectionCheckpointStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly BusinessDocumentValidator _validator = new();

    public BusinessProjectionCheckpointStore(string checkpointPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(checkpointPath);
        CheckpointPath = Path.GetFullPath(checkpointPath);
    }

    public string CheckpointPath { get; }

    public bool Exists => File.Exists(CheckpointPath);

    public static string GetDefaultPath(string? baseDirectory = null)
    {
        var resolvedBaseDirectory = string.IsNullOrWhiteSpace(baseDirectory)
            ? Directory.GetCurrentDirectory()
            : baseDirectory;

        return Path.GetFullPath(Path.Combine(
            resolvedBaseDirectory,
            "artifacts",
            "business-projection-checkpoint.json"));
    }

    public BusinessProjectionCheckpoint Load()
    {
        var json = File.ReadAllText(CheckpointPath);
        return Parse(json);
    }

    public BusinessProjectionCheckpoint? TryLoad()
    {
        return Exists ? Load() : null;
    }

    public string Serialize(BusinessProjectionCheckpoint checkpoint)
    {
        Validate(checkpoint);
        return JsonSerializer.Serialize(checkpoint, JsonOptions);
    }

    public BusinessProjectionCheckpoint Save(BusinessProjectionCheckpoint checkpoint, bool reloadFromDisk = true)
    {
        var json = Serialize(checkpoint);
        var directory = Path.GetDirectoryName(CheckpointPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(CheckpointPath, json);
        return reloadFromDisk ? Load() : Parse(json);
    }

    private BusinessProjectionCheckpoint Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var checkpoint = JsonSerializer.Deserialize<BusinessProjectionCheckpoint>(json, JsonOptions)
            ?? throw new InvalidOperationException("Projection checkpoint je prazdny nebo nevalidni.");

        Validate(checkpoint);
        return checkpoint;
    }

    private void Validate(BusinessProjectionCheckpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);

        if (checkpoint.CommandCount < 0)
            throw new InvalidOperationException("Projection checkpoint nesmi mit zaporny pocet commandu.");

        _validator.EnsureValid(checkpoint.Document);
    }
}