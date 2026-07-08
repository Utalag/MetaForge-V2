namespace MetaForge.Core.Common;

/// <summary>
/// Stav metadatového prvku.
/// </summary>
public enum MetadataState
{
    /// <summary>
    /// Prvek je v návrhu.
    /// </summary>
    Draft,

    /// <summary>
    /// Prvek je validní.
    /// </summary>
    Valid,

    /// <summary>
    /// Prvek obsahuje chyby.
    /// </summary>
    Invalid,

    /// <summary>
    /// Prvek je připraven k generování.
    /// </summary>
    Ready
}
