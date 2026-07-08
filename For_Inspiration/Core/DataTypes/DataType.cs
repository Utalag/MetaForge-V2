namespace MetaForge.Core.DataTypes;

/// <summary>
/// Výčet datových typů napříč jazyky.
/// </summary>
public enum DataType
{
    // Základní numerické typy
    Byte,
    Short,
    Int,
    Long,
    Float,
    Double,
    Decimal,
    Matrix,

    // Logické typy
    Boolean,

    // Textové typy
    String,
    Char,

    // Speciální typy
    Guid,
    Object,
    Void,

    // Časové typy
    Date,
    Time,
    DateTime,

    // Vlastní typ (nebo ValueObject s CustomTypeName)
    Custom
}
