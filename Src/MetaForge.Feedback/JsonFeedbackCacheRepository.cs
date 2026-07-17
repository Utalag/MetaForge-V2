using System.Text.Json;
using MetaForge.Feedback.Models;
using MetaForge.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace MetaForge.Feedback;

public sealed class JsonFeedbackCacheRepository : IFeedbackCacheRepository
{
    private readonly IOptions<StorageOptions> _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonFeedbackCacheRepository(IOptions<StorageOptions> options)
    {
        _options = options;
    }

    private string GetCachePath(string projectId)
    {
        var basePath = _options.Value.FeedbackCachePath;
        if (string.IsNullOrWhiteSpace(basePath))
            basePath = "data/feedback/";
        return Path.Combine(basePath, $"{projectId}.json");
    }

    public async Task<IReadOnlyList<AuthoringFeedbackRecord>> LoadOpenAsync(string projectId, CancellationToken ct)
    {
        var path = GetCachePath(projectId);
        if (!File.Exists(path))
            return Array.Empty<AuthoringFeedbackRecord>();

        var json = await File.ReadAllTextAsync(path, ct);
        return (JsonSerializer.Deserialize<List<AuthoringFeedbackRecord>>(json, JsonOptions) as IReadOnlyList<AuthoringFeedbackRecord>)
               ?? Array.Empty<AuthoringFeedbackRecord>();
    }

    public async Task UpsertAsync(AuthoringFeedbackRecord record, CancellationToken ct)
    {
        var path = GetCachePath(record.ProjectId);
        var records = (await LoadOpenAsync(record.ProjectId, ct)).ToList();

        var existing = records.FindIndex(r => r.FeedbackId == record.FeedbackId);
        if (existing >= 0)
            records[existing] = record;
        else
            records.Add(record);

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(records, JsonOptions);
        await File.WriteAllTextAsync(path, json, ct);
    }

    public async Task RemoveAsync(Guid feedbackId, CancellationToken ct)
    {
        // Remove requires knowing projectId — scan all cache files
        var basePath = _options.Value.FeedbackCachePath;
        if (string.IsNullOrWhiteSpace(basePath))
            basePath = "data/feedback/";

        if (!Directory.Exists(basePath))
            return;

        foreach (var file in Directory.GetFiles(basePath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file, ct);
            var records = JsonSerializer.Deserialize<List<AuthoringFeedbackRecord>>(json, JsonOptions);
            if (records == null) continue;

            var removed = records.RemoveAll(r => r.FeedbackId == feedbackId);
            if (removed > 0)
            {
                json = JsonSerializer.Serialize(records, JsonOptions);
                await File.WriteAllTextAsync(file, json, ct);
                return;
            }
        }
    }

    public async Task InvalidateByElementAsync(string elementId, CancellationToken ct)
    {
        var basePath = _options.Value.FeedbackCachePath;
        if (string.IsNullOrWhiteSpace(basePath))
            basePath = "data/feedback/";

        if (!Directory.Exists(basePath))
            return;

        foreach (var file in Directory.GetFiles(basePath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file, ct);
            var records = JsonSerializer.Deserialize<List<AuthoringFeedbackRecord>>(json, JsonOptions);
            if (records == null) continue;

            var removed = records.RemoveAll(r => r.ElementId == elementId);
            if (removed > 0)
            {
                json = JsonSerializer.Serialize(records, JsonOptions);
                await File.WriteAllTextAsync(file, json, ct);
            }
        }
    }
}
