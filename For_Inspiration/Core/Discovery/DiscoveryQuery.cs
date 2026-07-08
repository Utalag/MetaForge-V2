namespace MetaForge.Core.Discovery;

public sealed class DiscoveryQuery
{
    public string? Category { get; init; }
    public string? SubCategory { get; init; }
    public string? Item { get; init; }

    public bool HasCategory => !string.IsNullOrWhiteSpace(Category);
    public bool HasSubCategory => !string.IsNullOrWhiteSpace(SubCategory);
    public bool HasItem => !string.IsNullOrWhiteSpace(Item);

    public static DiscoveryQuery Root => new();
    public static DiscoveryQuery ForCategory(string category) => new() { Category = category };
    public static DiscoveryQuery ForItem(string category, string item) => new() { Category = category, Item = item };

    public static DiscoveryQuery Parse(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Root;

        var parts = query.Trim().Split(['/', ' '], 3, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var category = parts.Length > 0 ? parts[0] : null;
        var subCategory = parts.Length > 1 ? parts[1] : null;
        var item = parts.Length > 2 ? parts[2] : null;

        return new DiscoveryQuery { Category = category, SubCategory = subCategory, Item = item };
    }
}