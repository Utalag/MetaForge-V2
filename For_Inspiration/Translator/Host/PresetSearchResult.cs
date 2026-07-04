namespace MetaForge.Translator;

/// <summary>
/// Výsledek vyhledávání v presetech — agreguje katalogové presety a workspace CustomType.
/// </summary>
public sealed record PresetSearchResult(
    string Id,
    string DisplayName,
    string Kind,
    string Source,
    string Description);
