using System.Text;
using MetaForge.Core.Discovery;

namespace MetaForge.Translator;

public static class DiscoveryTextRenderer
{
    public static string Render(DiscoveryQueryResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Item is not null)
            return RenderItem(result.Item);

        if (result.Category is not null)
            return RenderCategory(result.Category);

        if (result.Root is not null)
            return RenderRoot(result.Root);

        return "MetaForge discovery nevratilo zadny vysledek.";
    }

    private static string RenderRoot(DiscoveryRootResult root)
    {
        var builder = new StringBuilder();
        builder.AppendLine("MetaForge Discovery — dostupne kategorie:");
        builder.AppendLine();

        foreach (var category in root.Categories)
        {
            var subCategorySuffix = category.HasSubCategories ? " | ma dalsi drill-down" : string.Empty;
            builder.AppendLine($"{category.Name,-16} — {category.Description} ({category.ItemCount}){subCategorySuffix}");
        }

        builder.AppendLine();
        builder.AppendLine("Pouziti:");
        builder.AppendLine("  discovery");
        builder.AppendLine("  discovery capabilities");
        builder.AppendLine("  discovery capabilities standard-math");
        builder.AppendLine("  discovery presets");

        return builder.ToString().TrimEnd();
    }

    private static string RenderCategory(DiscoveryCategoryResult category)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Discovery category: {category.Category}");

        if (!string.IsNullOrWhiteSpace(category.Description))
            builder.AppendLine($"Popis: {category.Description}");

        if (category.SubCategories is { Count: > 0 })
        {
            builder.AppendLine();
            builder.AppendLine($"Subcategories: {string.Join(", ", category.SubCategories)}");
        }

        if (category.Items.Count == 0)
        {
            builder.AppendLine();
            builder.AppendLine("Kategorie je zatim prazdna.");
            return builder.ToString().TrimEnd();
        }

        builder.AppendLine();
        builder.AppendLine("Items:");

        foreach (var item in category.Items)
        {
            builder.AppendLine($"- {item.Id} | {item.DisplayName}");
            if (!string.IsNullOrWhiteSpace(item.Description))
                builder.AppendLine($"  {item.Description}");
            if (item.SemanticHandles.Count > 0)
                builder.AppendLine($"  handles: {string.Join(", ", item.SemanticHandles)}");
            if (item.Tags.Count > 0)
                builder.AppendLine($"  tagy: {string.Join(", ", item.Tags)}");
        }

        return builder.ToString().TrimEnd();
    }

    private static string RenderItem(DiscoveryItemResult item)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Discovery item: {item.Id}");
        builder.AppendLine($"Nazev: {item.DisplayName}");

        if (!string.IsNullOrWhiteSpace(item.Description))
            builder.AppendLine($"Popis: {item.Description}");

        if (item.SemanticHandles.Count > 0)
            builder.AppendLine($"Handles: {string.Join(", ", item.SemanticHandles)}");

        if (item.Tags.Count > 0)
            builder.AppendLine($"Tagy: {string.Join(", ", item.Tags)}");

        if (item.Metadata.Count > 0)
        {
            builder.AppendLine("Metadata:");
            foreach (var (key, value) in item.Metadata.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
                builder.AppendLine($"- {key}: {value}");
        }

        if (!string.IsNullOrWhiteSpace(item.RawContent))
        {
            builder.AppendLine();
            builder.AppendLine("Raw content:");
            builder.AppendLine(item.RawContent);
        }

        return builder.ToString().TrimEnd();
    }
}