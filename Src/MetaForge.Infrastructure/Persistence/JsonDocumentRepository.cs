using System.Text.Json;
using MetaForge.BusinessModel.Models;
using MetaForge.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace MetaForge.Infrastructure.Persistence;

/// <summary>
/// JSON implementace IDocumentRepository — ukládá snapshot dokumentu jako jeden JSON soubor.
/// </summary>
public sealed class JsonDocumentRepository : IDocumentRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _filePath;

    /// <summary>
    /// Vytvoří JSON document repository s cestou z konfigurace.
    /// </summary>
    public JsonDocumentRepository(IOptions<StorageOptions> options)
    {
        _filePath = options.Value.DocumentPath;
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    /// <inheritdoc />
    public async Task SaveAsync(BusinessAuthoringDocument document, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var json = JsonSerializer.Serialize(document, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json, ct);
    }

    /// <inheritdoc />
    public async Task<BusinessAuthoringDocument?> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath)) return null;
        var json = await File.ReadAllTextAsync(_filePath, ct);
        return JsonSerializer.Deserialize<BusinessAuthoringDocument>(json, JsonOptions);
    }
}
