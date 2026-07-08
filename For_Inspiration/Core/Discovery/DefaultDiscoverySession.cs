using MetaForge.Core.Catalog;
using MetaForge.Core.DataTypes;
using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.Core.Discovery;

public sealed class DefaultDiscoverySession : IDiscoverySession
{
    private readonly CatalogManager _catalogManager;
    private readonly ForgeBlockPackageRegistry _registry;

    private static readonly Dictionary<string, CategoryDescriptor> CategoryDescriptors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["types"] = new CategoryDescriptor("Types", "Primitive and built-in types", false),
        ["presets"] = new CategoryDescriptor("Presets", "Preset configurations", false),
        ["capabilities"] = new CategoryDescriptor("Capabilities", "ForgeBlock capabilities", false),
        ["blocks"] = new CategoryDescriptor("Blocks", "Template blocks", true),
        ["methods"] = new CategoryDescriptor("Methods", "Runtime methods", false),
        ["tools"] = new CategoryDescriptor("Tools", "Registered capability tools from ForgeBlock packages", false)
    };

    public DefaultDiscoverySession(CatalogManager catalogManager, ForgeBlockPackageRegistry registry)
    {
        _catalogManager = catalogManager;
        _registry = registry;
    }

    public DiscoveryRootResult GetRoot()
    {
        return new(BuildRootCategories());
    }

    public bool IsKnownCategory(string? category)
    {
        return !string.IsNullOrWhiteSpace(category)
            && CategoryDescriptors.ContainsKey(category.Trim());
    }

    public DiscoveryCategoryResult GetCategory(DiscoveryQuery query)
    {
        var rawCategory = query.Category;
        if (string.IsNullOrWhiteSpace(rawCategory))
            return new DiscoveryCategoryResult("root", "MetaForge discovery", [], null);

        var category = rawCategory.ToLowerInvariant();
        List<DiscoveryItemSummary> items;
        List<string>? subCategories = null;

        switch (category)
        {
            case "types":
                items = GetTypeItems();
                break;
            case "presets":
                items = GetPresetItems();
                break;
            case "capabilities":
                items = GetCapabilityItems();
                break;
            case "blocks":
                items = GetBlockItems();
                subCategories = ["logic", "variable", "loop", "template"];
                break;
            case "tools":
                items = GetToolItems();
                break;
            default:
                items = [];
                break;
        }

        var description = CategoryDescriptors.TryGetValue(category, out var descriptor)
            ? descriptor.Description
            : string.Empty;

        return new DiscoveryCategoryResult(category, description, items, subCategories?.AsReadOnly());
    }

    public DiscoveryItemResult? GetItem(DiscoveryQuery query)
    {
        if (!query.HasItem)
            return null;

        var itemId = query.Item ?? string.Empty;

        return GetItemById(itemId);
    }

    public IReadOnlyList<DiscoveryItemSummary> SearchByTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return [];

        var normalized = tag.Trim();
        var results = new List<DiscoveryItemSummary>();
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var categoryKey in CategoryDescriptors.Keys)
        {
            var categoryResult = GetCategory(DiscoveryQuery.ForCategory(categoryKey));
            foreach (var item in categoryResult.Items)
            {
                if (seenIds.Contains(item.Id))
                    continue;

                if (item.Tags.Any(t => t.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
                {
                    results.Add(item);
                    seenIds.Add(item.Id);
                }
            }
        }

        return results;
    }

    public DiscoveryItemResult? TryResolveShortcut(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return null;

        var normalized = query.Trim();

        var directItem = GetItemById(normalized);
        if (directItem is not null)
            return directItem;

        var dottedShortcut = string.Join(
            '.',
            normalized.Split(['/', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        if (!string.Equals(dottedShortcut, normalized, StringComparison.OrdinalIgnoreCase))
        {
            var dottedItem = GetItemById(dottedShortcut);
            if (dottedItem is not null)
                return dottedItem;
        }

        var capabilityMatches = _registry.GetCapabilities()
            .Where(capability =>
                capability.CapabilityId.Equals(normalized, StringComparison.OrdinalIgnoreCase)
                || capability.PackageId.EndsWith(normalized, StringComparison.OrdinalIgnoreCase)
                || capability.DisplayName.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                || capability.SemanticHandles.Any(handle =>
                    handle.StartsWith($"mf.{normalized}.", StringComparison.OrdinalIgnoreCase)
                    || handle.Equals($"mf.{normalized}.*", StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        if (capabilityMatches.Length == 1)
            return CreateCapabilityItemResult(capabilityMatches[0]);

        var discoveryMatches = _registry.GetDiscoveryItems()
            .Where(item =>
                item.Id.StartsWith(normalized + ".", StringComparison.OrdinalIgnoreCase)
                || item.Id.Equals(normalized, StringComparison.OrdinalIgnoreCase)
                || item.DisplayName.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                || item.SemanticHandles.Any(handle => handle.StartsWith($"mf.{normalized}.", StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        if (discoveryMatches.Length == 1)
            return CreateDiscoveryItemResult(discoveryMatches[0]);

        return null;
    }

    private DiscoveryItemResult? GetItemById(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return null;

        var catalogItem = _catalogManager.Items.FirstOrDefault(i =>
            i.Id.Equals(itemId, StringComparison.OrdinalIgnoreCase));

        if (catalogItem != null)
        {
            return new DiscoveryItemResult(
                catalogItem.Id,
                catalogItem.DisplayName,
                catalogItem.Description,
                catalogItem.Tags.AsReadOnly(),
                Array.Empty<string>(),
                new Dictionary<string, string>
                {
                    ["category"] = catalogItem.Category,
                    ["itemType"] = catalogItem.ItemType.ToString(),
                    ["source"] = catalogItem.Source
                },
                catalogItem.RawJson);
        }

        var capability = _registry.GetCapabilities().FirstOrDefault(c =>
            c.DisplayName.Equals(itemId, StringComparison.OrdinalIgnoreCase) ||
            c.CapabilityId.Equals(itemId, StringComparison.OrdinalIgnoreCase));

        if (capability != null)
        {
            return CreateCapabilityItemResult(capability);
        }

        var discoveryItem = _registry.GetDiscoveryItems().FirstOrDefault(d =>
            d.Id.Equals(itemId, StringComparison.OrdinalIgnoreCase) ||
            d.DisplayName.Equals(itemId, StringComparison.OrdinalIgnoreCase));

        if (discoveryItem != null)
        {
            return CreateDiscoveryItemResult(discoveryItem);
        }

        return null;
    }

    private static DiscoveryItemResult CreateCapabilityItemResult(ForgeBlockCapabilityDescriptor capability)
    {
        return new DiscoveryItemResult(
            capability.CapabilityId,
            capability.DisplayName,
            capability.Description,
            capability.Tags.ToArray(),
            capability.SemanticHandles.ToArray(),
            new Dictionary<string, string>
            {
                ["packageId"] = capability.PackageId,
                ["category"] = capability.Category,
                ["kind"] = capability.Kind.ToString()
            },
            null);
    }

    private static DiscoveryItemResult CreateDiscoveryItemResult(ForgeBlockDiscoveryItem discoveryItem)
    {
        return new DiscoveryItemResult(
            discoveryItem.Id,
            discoveryItem.DisplayName,
            discoveryItem.Description ?? string.Empty,
            discoveryItem.Tags.ToArray(),
            discoveryItem.SemanticHandles.ToArray(),
            new Dictionary<string, string>(),
            null);
    }

    private List<DiscoveryCategorySummary> BuildRootCategories()
    {
        return new List<DiscoveryCategorySummary>
        {
            new("types", "Primitive and built-in types", GetTypeItems().Count, false),
            new("presets", "Preset configurations", GetPresetItems().Count, false),
            new("capabilities", "ForgeBlock capabilities", GetCapabilityItems().Count, false),
            new("blocks", "Template blocks", GetBlockItems().Count, true),
            new("methods", "Runtime methods", 0, false),
            new("tools", "Registered capability tools from ForgeBlock packages", GetToolItems().Count, false)
        };
    }

    private List<DiscoveryItemSummary> GetTypeItems()
    {
        return Enum.GetValues<DataType>()
            .Where(type => type != DataType.Custom)
            .Select(type => new DiscoveryItemSummary(
                type.ToString().ToLowerInvariant(),
                type.ToString(),
                $"Built-in primitive or runtime type '{type}'.",
                ["type", "primitive"],
                Array.Empty<string>()))
            .ToList();
    }
    private List<DiscoveryItemSummary> GetPresetItems() =>
        _catalogManager.Items.Select(item =>
            new DiscoveryItemSummary(item.Id, item.DisplayName, item.Description, item.Tags.AsReadOnly(), [])).ToList();

    private List<DiscoveryItemSummary> GetCapabilityItems()
    {
        var items = new List<DiscoveryItemSummary>();
        foreach (var cap in _registry.GetCapabilities())
        {
            items.Add(new DiscoveryItemSummary(cap.CapabilityId, cap.DisplayName, cap.Description, cap.Tags.ToArray(), cap.SemanticHandles.ToArray()));
        }
        return items;
    }

    private List<DiscoveryItemSummary> GetToolItems()
    {
        var items = new List<DiscoveryItemSummary>();
        foreach (var meta in _registry.GetCapabilityMetadata())
        {
            items.Add(new DiscoveryItemSummary(
                $"{meta.PackageId}.{meta.ToolId}",
                meta.DisplayName,
                meta.Description,
                [.. meta.Tags],
                [.. meta.SemanticHandles]));
        }
        return items;
    }

    private List<DiscoveryItemSummary> GetBlockItems() => [];

    private readonly record struct CategoryDescriptor(string Name, string Description, bool HasSubCategories);
}