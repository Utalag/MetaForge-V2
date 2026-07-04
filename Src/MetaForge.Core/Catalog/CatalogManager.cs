namespace MetaForge.Core.Catalog;

/// <summary>
/// Centrální správce katalogu — agreguje všechny ICatalogProvider.
/// Registrace presetů, vyhledávání, resolve typů.
/// </summary>
public sealed class CatalogManager
{
    private readonly List<ICatalogProvider> _providers = new();
    private readonly Dictionary<string, PresetDefinition> _customPresets = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Zaregistruje catalog providera (built-in, filesystem, marketplace).</summary>
    public void RegisterProvider(ICatalogProvider provider)
    {
        _providers.Add(provider);
    }

    /// <summary>Zaregistruje vlastní preset (z ForgeBlocku nebo uživatele).</summary>
    public void RegisterPreset(PresetDefinition preset)
    {
        _customPresets[preset.Name] = preset;
    }

    /// <summary>Vyhledá typ podle názvu — prohledá custom presety, pak providery.</summary>
    public PresetDefinition? ResolveType(string typeName)
    {
        // 1. Vlastní presety
        if (_customPresets.TryGetValue(typeName, out var custom))
            return custom;

        // 2. Providery v pořadí registrace
        foreach (var provider in _providers)
        {
            var result = provider.ResolveType(typeName);
            if (result is not null)
                return result;
        }

        return null;
    }

    /// <summary>Vyhledá presety podle dotazu (hledá v názvu a tazích).</summary>
    public IReadOnlyList<PresetDefinition> SearchPresets(string query)
    {
        var results = new List<PresetDefinition>();

        // Z custom presetů
        results.AddRange(_customPresets.Values
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                        || (p.Tags?.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)) ?? false)));

        // Z providerů
        foreach (var provider in _providers)
        {
            results.AddRange(provider.GetAllPresets()
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                            || (p.Tags?.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)) ?? false)));
        }

        return results.DistinctBy(p => p.Name).ToList().AsReadOnly();
    }

    /// <summary>Vrátí všechny dostupné presety.</summary>
    public IReadOnlyList<PresetDefinition> GetAllPresets()
    {
        var all = new List<PresetDefinition>();
        all.AddRange(_customPresets.Values);

        foreach (var provider in _providers)
            all.AddRange(provider.GetAllPresets());

        return all.DistinctBy(p => p.Name).ToList().AsReadOnly();
    }
}
