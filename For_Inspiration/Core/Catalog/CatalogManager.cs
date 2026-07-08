using System.Text.Json;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Catalog;

/// <summary>
/// Centrální orchestrátor katalogu.
/// Agreguje položky ze všech providerů, poskytuje vyhledávání,
/// deserializaci a instanciaci presetů.
/// </summary>
public class CatalogManager
{
    private readonly List<ICatalogProvider> _providers = new();
    private readonly List<CatalogItem> _items = new();
    private bool _isLoaded;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>Všechny načtené položky.</summary>
    public IReadOnlyList<CatalogItem> Items => _items.AsReadOnly();

    /// <summary>Je katalog načtený?</summary>
    public bool IsLoaded => _isLoaded;

    /// <summary>
    /// Zaregistruje catalog provider.
    /// </summary>
    public void RegisterProvider(ICatalogProvider provider)
    {
        _providers.Add(provider);
        _providers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        _isLoaded = false;
    }

    /// <summary>
    /// Načte/obnoví katalog ze všech providerů.
    /// </summary>
    public async Task LoadAsync()
    {
        _items.Clear();

        foreach (var provider in _providers)
        {
            var providerItems = await provider.LoadItemsAsync();

            foreach (var item in providerItems)
            {
                item.Source = provider.Name;

                var existing = _items.FindIndex(i => i.Id == item.Id);
                if (existing >= 0)
                    _items[existing] = item;
                else
                    _items.Add(item);
            }
        }

        _isLoaded = true;
    }

    /// <summary>
    /// Vyhledá položky v katalogu.
    /// </summary>
    public IReadOnlyList<CatalogItem> Search(CatalogSearchOptions? options = null)
    {
        EnsureLoaded();

        var query = _items.AsEnumerable();

        if (options == null)
            return query.ToList().AsReadOnly();

        if (options.ItemType.HasValue)
            query = query.Where(i => i.ItemType == options.ItemType.Value);

        if (!string.IsNullOrWhiteSpace(options.Category))
            query = query.Where(i => i.Category.Equals(
                options.Category, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(options.Source))
            query = query.Where(i => i.Source.Equals(
                options.Source, StringComparison.OrdinalIgnoreCase));

        if (options.Tags is { Count: > 0 })
            query = query.Where(i => i.Tags.Any(t =>
                options.Tags.Contains(t, StringComparer.OrdinalIgnoreCase)));

        if (!string.IsNullOrWhiteSpace(options.SearchText))
        {
            var search = options.SearchText;
            query = query.Where(i =>
                i.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                i.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                i.Tags.Any(t => t.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        return query.Take(options.MaxResults).ToList().AsReadOnly();
    }

    /// <summary>
    /// Načte obsah položky a vrátí surový JSON.
    /// </summary>
    public async Task<string> LoadContentAsync(CatalogItem item)
    {
        if (item.RawJson != null)
            return item.RawJson;

        var provider = _providers.FirstOrDefault(p => p.Name == item.Source)
            ?? throw new InvalidOperationException($"Provider '{item.Source}' not found.");

        item.RawJson = await provider.LoadContentAsync(item);
        return item.RawJson;
    }

    /// <summary>
    /// Načte a deserializuje Value Object definici z katalogu.
    /// </summary>
    public async Task<ValueObjectPreset> LoadValueObjectPresetAsync(string presetId)
    {
        EnsureLoaded();

        var item = _items.FirstOrDefault(i =>
            i.Id == presetId && i.ItemType == CatalogItemType.ValueObject)
            ?? throw new KeyNotFoundException($"Value Object preset '{presetId}' not found.");

        var json = await LoadContentAsync(item);
        return JsonSerializer.Deserialize<ValueObjectPreset>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize preset '{presetId}'.");
    }

    /// <summary>
    /// Načte a deserializuje ForgeBlock definici z katalogu.
    /// </summary>
    public async Task<ForgeBlockPreset> LoadForgeBlockPresetAsync(string presetId)
    {
        EnsureLoaded();

        var item = _items.FirstOrDefault(i =>
            i.Id == presetId && i.ItemType == CatalogItemType.ForgeBlock)
            ?? throw new KeyNotFoundException($"ForgeBlock preset '{presetId}' not found.");

        var json = await LoadContentAsync(item);
        return JsonSerializer.Deserialize<ForgeBlockPreset>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize preset '{presetId}'.");
    }

    /// <summary>
    /// Najde položku podle ID.
    /// </summary>
    public CatalogItem? FindById(string id)
    {
        EnsureLoaded();
        return _items.FirstOrDefault(i => i.Id == id);
    }

    /// <summary>
    /// Statistiky katalogu.
    /// </summary>
    public CatalogStats GetStats()
    {
        return new CatalogStats
        {
            TotalItems = _items.Count,
            ValueObjects = _items.Count(i => i.ItemType == CatalogItemType.ValueObject),
            ClassPresets = _items.Count(i => i.ItemType == CatalogItemType.ClassPreset),
            ForgeBlocks = _items.Count(i => i.ItemType == CatalogItemType.ForgeBlock),
            Providers = _providers.Count
        };
    }

    private void EnsureLoaded()
    {
        if (!_isLoaded)
            throw new InvalidOperationException("Catalog not loaded. Call LoadAsync() first.");
    }

    // ──────────────────── Primitive alias map ────────────────────

    private static readonly Dictionary<string, DataType> PrimitiveAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["string"]   = DataType.String,
        ["text"]     = DataType.String,
        ["int"]      = DataType.Int,
        ["integer"]  = DataType.Int,
        ["long"]     = DataType.Long,
        ["short"]    = DataType.Short,
        ["byte"]     = DataType.Byte,
        ["float"]    = DataType.Float,
        ["double"]   = DataType.Double,
        ["decimal"]  = DataType.Decimal,
        ["bool"]     = DataType.Boolean,
        ["boolean"]  = DataType.Boolean,
        ["guid"]     = DataType.Guid,
        ["uuid"]     = DataType.Guid,
        ["char"]     = DataType.Char,
        ["date"]     = DataType.Date,
        ["time"]     = DataType.Time,
        ["datetime"] = DataType.DateTime,
        ["object"]   = DataType.Object,
        ["void"]     = DataType.Void,
    };

    private static readonly Dictionary<DataType, string> PrimitiveNames = PrimitiveAliases
        .GroupBy(kv => kv.Value)
        .ToDictionary(g => g.Key, g => g.First().Key);

    // ──────────────────── Name inference patterns ────────────────────

    private static readonly (string Pattern, string CatalogId)[] NameInferencePatterns =
    [
        ("email",       "email-address"),
        ("mail",        "email-address"),
        ("phone",       "phone-number"),
        ("tel",         "phone-number"),
        ("url",         "url-value"),
        ("uri",         "url-value"),
        ("link",        "url-value"),
        ("money",       "money"),
        ("price",       "money"),
        ("amount",      "money"),
        ("cost",        "money"),
        ("fee",         "money"),
        ("salary",      "money"),
        ("wage",        "money"),
        ("total",       "money"),
        ("balance",     "money"),
        ("postal",      "postal-code"),
        ("zip",         "postal-code"),
        ("postcode",    "postal-code"),
        ("vin",         "vin"),
        ("iban",        "iban"),
        ("ipaddress",   "ip-address"),
        ("ip_address",  "ip-address"),
        ("macaddress",  "mac-address"),
        ("mac_address", "mac-address"),
        ("creditcard",  "credit-card"),
        ("cardnumber",  "credit-card"),
        ("country",     "country"),
        ("countrycode", "country"),
        ("color",       "color"),
        ("colour",      "color"),
        ("ssn",         "ssn"),
    ];

    // ──────────────────── New API methods ────────────────────

    /// <summary>
    /// Rozliší typ z textového výrazu.
    /// Nejprve hledá primitivní alias, pak katalogový preset (ID nebo tag match).
    /// </summary>
    public TypeResolution ResolveType(string typeExpr)
    {
        if (string.IsNullOrWhiteSpace(typeExpr))
            return TypeResolution.Unresolved;

        var normalized = typeExpr.Trim();

        // 1. Primitivní alias
        if (PrimitiveAliases.TryGetValue(normalized, out var dataType))
            return TypeResolution.FromPrimitive(dataType);

        // 2. Přímý catalog ID match
        EnsureLoaded();
        var item = _items.FirstOrDefault(i =>
            i.Id.Equals(normalized, StringComparison.OrdinalIgnoreCase) &&
            i.ItemType == CatalogItemType.ValueObject);

        if (item is not null)
            return TypeResolution.FromCatalog(item.Id);

        // 3. Tag-based match (hledáme v tazích Value Object presetů)
        item = _items.FirstOrDefault(i =>
            i.ItemType == CatalogItemType.ValueObject &&
            i.Tags.Any(t => t.Equals(normalized, StringComparison.OrdinalIgnoreCase)));

        if (item is not null)
            return TypeResolution.FromCatalog(item.Id);

        return TypeResolution.Unresolved;
    }

    /// <summary>
    /// Navrhne katalogový preset na základě názvu property.
    /// Vrátí null, pokud žádný preset neodpovídá.
    /// </summary>
    public CatalogItem? SuggestPreset(string propertyName)
    {
        var results = SuggestPresets(propertyName);
        return results.Count > 0 ? results[0] : null;
    }

    /// <summary>
    /// Navrhne katalogové presety na základě názvu property a volitelně typu.
    /// Deterministický pattern matching — žádné AI volání.
    /// Výstup = IReadOnlyList&lt;CatalogItem&gt; jako strukturovaný kontext pro LLM.
    /// </summary>
    public IReadOnlyList<CatalogItem> SuggestPresets(string propertyName, string? type = null)
    {
        var suggestions = SuggestPresets(propertyName, type, description: null, tags: null);
        return suggestions.Select(s => s.Item).ToList();
    }

    /// <summary>
    /// Navrhne katalogové presety na základě názvu property, typu, popisu a tagů.
    /// Deterministický pattern matching s rankingem — žádné AI volání.
    /// </summary>
    /// <param name="propertyName">Název property pro name matching.</param>
    /// <param name="type">Volitelný typ pro type matching.</param>
    /// <param name="description">Volitelný popis pro description matching.</param>
    /// <param name="tags">Volitelné tagy pro tag matching.</param>
    /// <param name="itemTypes">Povolené typy katalogových položek. Pokud je null, použije se [ValueObject].</param>
    public IReadOnlyList<NodePresetSuggestion> SuggestPresets(
        string propertyName,
        string? type,
        string? description,
        IReadOnlyList<string>? tags,
        IReadOnlyList<CatalogItemType>? itemTypes = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return [];

        EnsureLoaded();

        var allowedTypes = itemTypes ?? [CatalogItemType.ValueObject];
        var suggestions = new List<(CatalogItem Item, PresetMatchKind Kind, int Score)>();
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        bool IsAllowedType(CatalogItemType itemType) => allowedTypes.Contains(itemType);

        // 1. Name match (skóre 100)
        var normalizedName = NormalizeForMatching(propertyName);
        foreach (var (pattern, catalogId) in NameInferencePatterns)
        {
            if (!normalizedName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!seenIds.Add(catalogId))
                continue;

            var item = _items.FirstOrDefault(i =>
                i.Id.Equals(catalogId, StringComparison.OrdinalIgnoreCase) &&
                IsAllowedType(i.ItemType));

            if (item is not null)
                suggestions.Add((item, PresetMatchKind.Name, 100));
        }

        // 2. Type match (skóre 90)
        if (!string.IsNullOrWhiteSpace(type))
        {
            foreach (var item in _items.Where(i => IsAllowedType(i.ItemType)))
            {
                if (item.Tags.Any(t => t.Equals(type, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!seenIds.Add(item.Id))
                        continue;
                    suggestions.Add((item, PresetMatchKind.Type, 90));
                }
            }
        }

        // 3. Tag match (skóre 70)
        if (tags is { Count: > 0 })
        {
            foreach (var item in _items.Where(i => IsAllowedType(i.ItemType)))
            {
                if (item.Tags.Any(t => tags.Any(tag =>
                    tag.Equals(t, StringComparison.OrdinalIgnoreCase))))
                {
                    if (!seenIds.Add(item.Id))
                        continue;
                    suggestions.Add((item, PresetMatchKind.Tag, 70));
                }
            }
        }

        // 4. Description match (skóre 40)
        if (!string.IsNullOrWhiteSpace(description))
        {
            foreach (var item in _items.Where(i => IsAllowedType(i.ItemType)))
            {
                if (!string.IsNullOrWhiteSpace(item.Description) &&
                    item.Description.Contains(description, StringComparison.OrdinalIgnoreCase))
                {
                    if (!seenIds.Add(item.Id))
                        continue;
                    suggestions.Add((item, PresetMatchKind.Description, 40));
                }
            }
        }

        // Volitelný type filtr — pokud je type zadaný a existují name match výsledky,
        // upřednostni položky jejichž tagy obsahují type
        if (!string.IsNullOrWhiteSpace(type) && suggestions.Count > 1)
        {
            var typeMatches = suggestions
                .Where(s => s.Item.Tags.Any(tag => tag.Equals(type, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (typeMatches.Count > 0)
            {
                // Zvýšíme skóre name matchů o 10, pokud mají i type match
                suggestions = suggestions.Select(s =>
                {
                    if (s.Item.Tags.Any(tag => tag.Equals(type, StringComparison.OrdinalIgnoreCase)))
                        return (s.Item, s.Kind, s.Score + 10);
                    return s;
                }).ToList();
            }
        }

        return suggestions
            .Select(s => new NodePresetSuggestion
            {
                Item = s.Item,
                MatchKind = s.Kind,
                RelevanceScore = s.Score,
            })
            .OrderByDescending(s => s.RelevanceScore)
            .ToList();
    }

    private static string NormalizeForMatching(string input)
    {
        return input
            .Replace("_", "")
            .Replace("-", "")
            .ToLowerInvariant();
    }

    /// <summary>
    /// Vrátí kanonický textový alias pro primitivní DataType.
    /// </summary>
    public static string? GetPrimitiveName(DataType dataType) =>
        PrimitiveNames.TryGetValue(dataType, out var name) ? name : null;
}
