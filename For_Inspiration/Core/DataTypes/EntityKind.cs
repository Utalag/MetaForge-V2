namespace MetaForge.Core.DataTypes;

/// <summary>
/// Druh entity pro vlastní typy.
/// </summary>
public enum EntityKind
{
    Primitive,
    Class,
    Interface,
    Enum,
    Struct,
    Delegate,

    /// <summary>Vogen Value Object (strongly-typed wrapper).</summary>
    ValueObject
}
