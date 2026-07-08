using System.Reflection;
using System.Text.Json;

namespace MetaForge.Core.Catalog;

/// <summary>
/// Provider pro built-in presety embedded v assembly.
/// Presety jsou uloženy jako EmbeddedResource ve složce Catalog/Presets/.
/// </summary>
public class BuiltInCatalogProvider : ICatalogProvider
{
    private const string ResourcePrefix = "MetaForge.Core.Catalog.Presets.";

    public string Name => "built-in";
    public int Priority => 0;

    public async Task<IReadOnlyList<CatalogItem>> LoadItemsAsync()
    {
        var items = new List<CatalogItem>();
        var assembly = typeof(BuiltInCatalogProvider).Assembly;

        foreach (var resourceName in assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(ResourcePrefix) && n.EndsWith(".json")))
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;

            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var item = ParseCatalogItem(json, resourceName);
            if (item != null)
                items.Add(item);
        }

        return items.AsReadOnly();
    }

    public async Task<string> LoadContentAsync(CatalogItem item)
    {
        if (item.RawJson != null)
            return item.RawJson;

        var assembly = typeof(BuiltInCatalogProvider).Assembly;
        using var stream = assembly.GetManifestResourceStream(item.FilePath)
            ?? throw new FileNotFoundException($"Embedded resource not found: {item.FilePath}");

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static CatalogItem? ParseCatalogItem(string json, string resourceName)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new CatalogItem
            {
                Id = root.GetProperty("id").GetString() ?? "",
                DisplayName = root.GetProperty("displayName").GetString() ?? "",
                Description = root.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "",
                Version = root.TryGetProperty("version", out var v) ? v.GetString() ?? "1.0.0" : "1.0.0",
                Author = root.TryGetProperty("author", out var a) ? a.GetString() ?? "MetaForge" : "MetaForge",
                Icon = root.TryGetProperty("icon", out var ic) ? ic.GetString() ?? "📦" : "📦",
                Category = root.TryGetProperty("category", out var c) ? c.GetString() ?? "" : "",
                CreditCost = root.TryGetProperty("creditCost", out var cc) ? cc.GetInt32() : 0,
                Tags = root.TryGetProperty("tags", out var t)
                    ? t.EnumerateArray().Select(e => e.GetString() ?? "").ToList()
                    : new List<string>(),
                ItemType = DetermineItemType(resourceName),
                FilePath = resourceName,
                Source = "built-in",
                RawJson = json
            };
        }
        catch
        {
            return null;
        }
    }

    private static CatalogItemType DetermineItemType(string resourceName)
    {
        // Directory-based detection (primary): checks for directory segment in resource name
        if (resourceName.Contains(".ValueObjects.")) return CatalogItemType.ValueObject;
        if (resourceName.Contains(".ForgeBlocks.")) return CatalogItemType.ForgeBlock;

        // Filename convention fallback (legacy/other preset types)
        if (resourceName.Contains(".preset.")) return CatalogItemType.ValueObject;
        if (resourceName.Contains(".class.")) return CatalogItemType.ClassPreset;
        if (resourceName.Contains(".interface.")) return CatalogItemType.InterfacePreset;
        if (resourceName.Contains(".enum.")) return CatalogItemType.EnumPreset;
        if (resourceName.Contains(".struct.")) return CatalogItemType.StructPreset;
        if (resourceName.Contains(".block.")) return CatalogItemType.ForgeBlock;
        return CatalogItemType.ClassPreset;
    }
}
