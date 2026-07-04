using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

public sealed class JsonlShadowCommandStore : IShadowCommandStore
{
    private static readonly ConcurrentDictionary<string, object> AppendLocks = new(StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly object _appendLock;

    public JsonlShadowCommandStore(string? filePath = null)
    {
        FilePath = ResolveFilePath(filePath);
        _appendLock = AppendLocks.GetOrAdd(FilePath, static _ => new object());
    }

    public string FilePath { get; }

    public static string GetDefaultPath(string? baseDirectory = null)
    {
        var resolvedBaseDirectory = string.IsNullOrWhiteSpace(baseDirectory)
            ? Directory.GetCurrentDirectory()
            : baseDirectory;

        return Path.GetFullPath(Path.Combine(
            resolvedBaseDirectory,
            "artifacts",
            "business-authoring-command-log.jsonl"));
    }

    public ShadowCommandAppendResult Append(CommandEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        try
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(envelope, JsonOptions);

            lock (_appendLock)
            {
                using var stream = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                writer.WriteLine(json);
            }

            return new ShadowCommandAppendResult(true);
        }
        catch (Exception exception)
        {
            return new ShadowCommandAppendResult(false, $"Append shadow command logu selhal: {exception.Message}");
        }
    }

    private static string ResolveFilePath(string? filePath)
    {
        return string.IsNullOrWhiteSpace(filePath)
            ? GetDefaultPath()
            : Path.GetFullPath(filePath);
    }
}