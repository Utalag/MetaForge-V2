namespace MetaForge.Core.Abstractions;

/// <summary>
/// Rozsah platnosti metadata — určuje, pro koho je anotace určena.
/// </summary>
public enum MetadataScope
{
    /// <summary>Doménová metadata — business význam, glosář.</summary>
    Domain,

    /// <summary>Validační metadata — pravidla pro validaci.</summary>
    Validation,

    /// <summary>Generátorová metadata — hinty pro generování kódu.</summary>
    Generation,

    /// <summary>AI metadata — kontext pro AI inference, few-shot příklady.</summary>
    Ai,

    /// <summary>Dokumentační metadata — XML docs, remarks.</summary>
    Documentation,
}

/// <summary>
/// Strategie při slučování dvou MetadataBagů.
/// </summary>
public enum MergeStrategy
{
    /// <summary>Novější hodnota přepisuje starší.</summary>
    Override,

    /// <summary>Starší hodnota zůstává, novější se ignoruje.</summary>
    Skip,

    /// <summary>Při konfliktu vyhodí výjimku.</summary>
    Throw,
}

/// <summary>
/// Jeden záznam v MetadataBag.
/// </summary>
public sealed record MetadataEntry(string Key, object? Value, MetadataScope Scope);

/// <summary>
/// Univerzální key-value anotační systém na každém elementu.
/// Komplementární k AttributeElement (C#-specific `[Attribute]`).
/// 
/// Použití:
/// <code>
/// element.Metadata.Set("Docs.Summary", "Popis entity");
/// var summary = element.Metadata.Get&lt;string&gt;("Docs.Summary");
/// </code>
/// </summary>
public sealed class MetadataBag
{
    private readonly Dictionary<string, MetadataEntry> _entries = new();

    /// <summary>Počet záznamů.</summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Nastaví metadata. Přepíše existující hodnotu se stejným klíčem.
    /// </summary>
    public MetadataBag Set<T>(string key, T value, MetadataScope scope = MetadataScope.Domain)
    {
        _entries[key] = new MetadataEntry(key, value, scope);
        return this;
    }

    /// <summary>
    /// Získá hodnotu metadat podle klíče. Vrací default(T) pokud klíč neexistuje.
    /// </summary>
    public T? Get<T>(string key)
    {
        if (_entries.TryGetValue(key, out var entry) && entry.Value is T typedValue)
            return typedValue;
        return default;
    }

    /// <summary>Existuje klíč?</summary>
    public bool Has(string key) => _entries.ContainsKey(key);

    /// <summary>Odstraní klíč.</summary>
    public bool Remove(string key) => _entries.Remove(key);

    /// <summary>Všechny záznamy v daném scope.</summary>
    public IEnumerable<MetadataEntry> Where(MetadataScope scope) =>
        _entries.Values.Where(e => e.Scope == scope);

    /// <summary>Všechny záznamy.</summary>
    public IReadOnlyList<MetadataEntry> GetAll() => _entries.Values.ToList().AsReadOnly();

    /// <summary>
    /// Sloučí jiný MetadataBag do tohoto.
    /// </summary>
    /// <param name="other">Druhý bag ke sloučení.</param>
    /// <param name="strategy">Strategie při konfliktu klíčů.</param>
    public MetadataBag Merge(MetadataBag other, MergeStrategy strategy = MergeStrategy.Override)
    {
        foreach (var entry in other.GetAll())
        {
            if (_entries.ContainsKey(entry.Key))
            {
                switch (strategy)
                {
                    case MergeStrategy.Override:
                        _entries[entry.Key] = entry;
                        break;
                    case MergeStrategy.Skip:
                        break;
                    case MergeStrategy.Throw:
                        throw new InvalidOperationException(
                            $"Metadata key '{entry.Key}' already exists and MergeStrategy is Throw.");
                }
            }
            else
            {
                _entries[entry.Key] = entry;
            }
        }
        return this;
    }

    /// <summary>
    /// Vytvoří kopii MetadataBag s nezávislými záznamy.
    /// </summary>
    public MetadataBag Clone()
    {
        var clone = new MetadataBag();
        foreach (var (key, entry) in _entries)
            clone._entries[key] = entry;
        return clone;
    }

    /// <summary>
    /// Standardizované klíče metadat (konstanty pro typovou bezpečnost).
    /// </summary>
    public static class Keys
    {
        // === Validation ===
        public const string ValidationRequired = "Validation.Required";
        public const string ValidationMinLength = "Validation.MinLength";
        public const string ValidationMaxLength = "Validation.MaxLength";
        public const string ValidationRangeMin = "Validation.Range.Min";
        public const string ValidationRangeMax = "Validation.Range.Max";

        // === Documentation ===
        public const string DocsSummary = "Docs.Summary";
        public const string DocsReturns = "Docs.Returns";
        public const string DocsRemarks = "Docs.Remarks";
        public static string DocsParam(string name) => $"Docs.Param.{name}";
        public static string DocsException(string type) => $"Docs.Exception.{type}";

        // === Generation ===
        public const string GenerationIgnore = "Generation.Ignore";
        public const string GenerationUsePartial = "Generation.UsePartial";
        public const string GenerationFileName = "Generation.FileName";
        public const string GenerationJsonIgnore = "Generation.JsonIgnore";

        // === AI ===
        public const string AiContext = "Ai.Context";
        public const string AiExample = "Ai.Example";

        // === Domain ===
        public const string DomainBusinessName = "Domain.BusinessName";
        public const string DomainGlossary = "Domain.Glossary";
    }
}
