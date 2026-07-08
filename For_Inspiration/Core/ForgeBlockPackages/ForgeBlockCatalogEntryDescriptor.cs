namespace MetaForge.Core.ForgeBlockPackages;

using MetaForge.Core.Catalog;

/// <summary>
/// Typ katalogoveho discovery zaznamu vystaveneho ForgeBlock balickem.
/// </summary>
public enum ForgeBlockCatalogEntryKind
{
    Reference,
    Preset,
    Example,
    Template,
    QuickStart
}

/// <summary>
/// Discovery metadata jednoho katalogoveho zaznamu dodaneho ForgeBlock balickem.
/// </summary>
public sealed record ForgeBlockCatalogEntryDescriptor
{
    public required string PackageId { get; init; }

    public required string EntryId { get; init; }

    public required string DisplayName { get; init; }

    public string Description { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public ForgeBlockCatalogEntryKind Kind { get; init; } = ForgeBlockCatalogEntryKind.Reference;

    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<string> SemanticHandles { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<ForgeBlockCapabilityReference> RelatedCapabilities { get; init; } = Array.Empty<ForgeBlockCapabilityReference>();

    /// <summary>
    /// Volitelny typ katalogove polozky pro syntezu CatalogItem pres ForgeBlockRegistryCatalogProvider.
    /// Pokud je null, entry zustane discovery-only a nevstupuje do CatalogManager pipeline.
    /// </summary>
    public CatalogItemType? CatalogItemType { get; init; }

    /// <summary>
    /// Volitelny inline JSON obsah presetu. Pokud je nastaven, umoznuje plne LoadValueObjectPresetAsync/LoadForgeBlockPresetAsync.
    /// Pokud je null a CatalogItemType je nastaven, entry je metadata-only; LoadContentAsync hodi NotSupportedException.
    /// </summary>
    public string? RawPresetJson { get; init; }
}

/// <summary>
/// In-memory katalog descriptoru registrovanych ForgeBlock catalog contributory.
/// </summary>
public sealed class ForgeBlockCatalog : IForgeBlockCatalog
{
    private readonly Dictionary<string, ForgeBlockCatalogEntryDescriptor> _entries = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<ForgeBlockCatalogEntryDescriptor> Entries => _entries.Values
        .OrderBy(entry => entry.PackageId, StringComparer.OrdinalIgnoreCase)
        .ThenBy(entry => entry.EntryId, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public void AddEntry(ForgeBlockCatalogEntryDescriptor entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrWhiteSpace(entry.PackageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(entry.EntryId);

        var key = BuildKey(entry.PackageId, entry.EntryId);
        if (_entries.TryGetValue(key, out var existing))
        {
            if (!EqualityComparer<ForgeBlockCatalogEntryDescriptor>.Default.Equals(existing, entry))
                throw new InvalidOperationException($"Conflicting ForgeBlock catalog entry '{entry.PackageId}/{entry.EntryId}'.");

            return;
        }

        _entries[key] = entry;
    }

    public bool TryGetEntry(string packageId, string entryId, out ForgeBlockCatalogEntryDescriptor entry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        return _entries.TryGetValue(BuildKey(packageId, entryId), out entry!);
    }

    private static string BuildKey(string packageId, string entryId) => $"{packageId}/{entryId}";
}