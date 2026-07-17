using System.Text.Json;
using MetaForge.Feedback.Models;
using MetaForge.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace MetaForge.Feedback;

public sealed class JsonFeedbackLearningRepository : IFeedbackLearningRepository
{
    private readonly IOptions<StorageOptions> _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonFeedbackLearningRepository(IOptions<StorageOptions> options)
    {
        _options = options;
    }

    private string GetLearningPath()
    {
        var path = _options.Value.LearningArchivePath;
        if (string.IsNullOrWhiteSpace(path))
            path = "data/learning/archive.jsonl";
        return path;
    }

    public async Task AppendAsync(FeedbackLearningRecord record, CancellationToken ct)
    {
        var path = GetLearningPath();
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(record, JsonOptions);
        await File.AppendAllTextAsync(path, json + Environment.NewLine, ct);
    }

    public async Task<IReadOnlyList<FeedbackLearningRecord>> GetPendingExportAsync(CancellationToken ct)
    {
        var path = GetLearningPath();
        if (!File.Exists(path))
            return Array.Empty<FeedbackLearningRecord>();

        var lines = await File.ReadAllLinesAsync(path, ct);
        return lines
            .Select(l => JsonSerializer.Deserialize<FeedbackLearningRecord>(l, JsonOptions))
            .Where(r => r != null && r.ConsentState == "ReadyForExport")
            .Cast<FeedbackLearningRecord>()
            .ToList();
    }

    public async Task MarkExportedAsync(Guid learningId, CancellationToken ct)
    {
        var path = GetLearningPath();
        if (!File.Exists(path)) return;

        var lines = await File.ReadAllLinesAsync(path, ct);
        var updated = lines.Select(line =>
        {
            var record = JsonSerializer.Deserialize<FeedbackLearningRecord>(line, JsonOptions);
            if (record?.LearningId == learningId)
                return JsonSerializer.Serialize(record with { ConsentState = "Exported" }, JsonOptions);
            return line;
        }).ToList();

        await File.WriteAllTextAsync(path, string.Join(Environment.NewLine, updated) + Environment.NewLine, ct);
    }
}
