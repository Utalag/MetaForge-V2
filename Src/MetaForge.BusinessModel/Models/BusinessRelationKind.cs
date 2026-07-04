namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Druh relace mezi entitami.
/// Nahrazuje stringový RelationType za typově bezpečný enum.
/// </summary>
public enum BusinessRelationKind
{
    /// <summary>Entita patří jiné entitě (1:1, owned).</summary>
    BelongsTo = 0,

    /// <summary>Entita má mnoho jiných entit (1:N).</summary>
    HasMany = 1,

    /// <summary>Entita má právě jednu jinou entitu (1:1).</summary>
    HasOne = 2,

    /// <summary>Mnoho na mnoho (N:M).</summary>
    ManyToMany = 3,
}
