namespace MetaForge.Core.Catalog;

/// <summary>
/// Typ položky v katalogu.
/// </summary>
public enum CatalogItemType
{
    /// <summary>Value Object definice (.vo.json).</summary>
    ValueObject,

    /// <summary>Class preset (.class.json).</summary>
    ClassPreset,

    /// <summary>Interface preset (.interface.json).</summary>
    InterfacePreset,

    /// <summary>Enum preset (.enum.json).</summary>
    EnumPreset,

    /// <summary>Struct preset (.struct.json).</summary>
    StructPreset,

    /// <summary>ForgeBlock definice (.block.json).</summary>
    ForgeBlock,

    /// <summary>Entity šablona — seed pro business entity s atributy.</summary>
    EntityTemplate,

    /// <summary>Doménová šablona — seed pro více entit a relací.</summary>
    DomainTemplate,

    /// <summary>Architektonická šablona — seed pro pattern (CQRS, Repository, ...).</summary>
    ArchitectureTemplate
}
