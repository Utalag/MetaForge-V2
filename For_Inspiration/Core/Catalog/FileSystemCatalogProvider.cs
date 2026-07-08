using System.Text.Json;

namespace MetaForge.Core.Catalog;

/// <summary>
/// Provider pro presety uložené na souborovém systému.
/// Umožňuje uživatelům vytvářet vlastní presety bez kompilace.
/// Výchozí cesta: ~/.metaforge/presets/
/// </summary>
public class FileSystemCatalogProvider : ICatalogProvider
{
    private readonly string _basePath;

    public string Name => "user";
    public int Priority => 10;

    public FileSystemCatalogProvider(string? basePath = null)
    {
        _basePath = basePath
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".metaforge", "presets");
    }

    public async Task<IReadOnlyList<CatalogItem>> LoadItemsAsync()
    {
        var items = new List<CatalogItem>();

        if (!Directory.Exists(_basePath))
            return items.AsReadOnly();

        var jsonFiles = Directory.GetFiles(_basePath, "*.json", SearchOption.AllDirectories);

        foreach (var filePath in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                items.Add(new CatalogItem
                {
                    Id = root.TryGetProperty("id", out var id) ? id.GetString() ?? "" : Path.GetFileNameWithoutExtension(filePath),
                    DisplayName = root.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? "" : "",
                    Description = root.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "",
                    Version = root.TryGetProperty("version", out var v) ? v.GetString() ?? "1.0.0" : "1.0.0",
                    Author = root.TryGetProperty("author", out var a) ? a.GetString() ?? "User" : "User",
                    Icon = root.TryGetProperty("icon", out var ic) ? ic.GetString() ?? "📦" : "📦",
                    Category = root.TryGetProperty("category", out var c) ? c.GetString() ?? "" : "",
                    CreditCost = root.TryGetProperty("creditCost", out var cc) ? cc.GetInt32() : 0,
                    Tags = root.TryGetProperty("tags", out var t)
                        ? t.EnumerateArray().Select(e => e.GetString() ?? "").ToList()
                        : new List<string>(),
                    ItemType = DetermineItemType(filePath),
                    FilePath = filePath,
                    Source = "user"
                });
            }
            catch
            {
                // Skip invalid files
            }
        }

        return items.AsReadOnly();
    }

    public async Task<string> LoadContentAsync(CatalogItem item)
    {
        return await File.ReadAllTextAsync(item.FilePath);
    }

    private static CatalogItemType DetermineItemType(string filePath)
    {
        var name = Path.GetFileName(filePath);
        if (name.EndsWith(".vo.json")) return CatalogItemType.ValueObject;
        if (name.EndsWith(".class.json")) return CatalogItemType.ClassPreset;
        if (name.EndsWith(".interface.json")) return CatalogItemType.InterfacePreset;
        if (name.EndsWith(".enum.json")) return CatalogItemType.EnumPreset;
        if (name.EndsWith(".struct.json")) return CatalogItemType.StructPreset;
        if (name.EndsWith(".block.json")) return CatalogItemType.ForgeBlock;
        return CatalogItemType.ClassPreset;
    }
}
