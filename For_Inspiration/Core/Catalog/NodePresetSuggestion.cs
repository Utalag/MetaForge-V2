namespace MetaForge.Core.Catalog;

/// <summary>
/// Druh shody mezi vstupním kontextem a katalogovou položkou.
/// </summary>
public enum PresetMatchKind
{
    Name,
    Type,
    Tag,
    Description,
}

/// <summary>
/// Jedna navržená katalogová položka s metadaty shody.
/// </summary>
public sealed class NodePresetSuggestion
{
    /// <summary>Katalogová položka.</summary>
    public CatalogItem Item { get; init; } = null!;

    /// <summary>Druh shody (název, typ, tag, popis).</summary>
    public PresetMatchKind MatchKind { get; init; }

    /// <summary>
    /// Relevance skóre 0–100. Vyšší = lepší shoda.
    /// Name match = 100, Type match = 90, Tag match = 70, Description match = 40.
    /// </summary>
    public int RelevanceScore { get; init; }
}
