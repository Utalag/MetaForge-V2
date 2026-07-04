namespace MetaForge.Core.DataTypes;

/// <summary>
/// Výčet 32 datových typů mapovaných na C# typy.
/// Používá se v TypeModel jako základní typová informace.
/// </summary>
public enum DataType : int
{
    // === Číselné ===
    Bool,       // System.Boolean
    Byte,       // System.Byte
    SByte,      // System.SByte
    Int16,      // System.Int16
    UInt16,     // System.UInt16
    Int32,      // System.Int32
    UInt32,     // System.UInt32
    Int64,      // System.Int64
    UInt64,     // System.UInt64
    Int128,     // System.Int128
    Half,       // System.Half
    Single,     // System.Single
    Double,     // System.Double
    Decimal,    // System.Decimal
    NInt,       // System.IntPtr
    NUInt,      // System.UIntPtr

    // === Textové ===
    Char,       // System.Char
    String,     // System.String

    // === Binární ===
    Binary,     // System.Byte[]

    // === Časové ===
    DateOnly,   // System.DateOnly
    TimeOnly,   // System.TimeOnly
    DateTime,   // System.DateTime
    DateTimeOffset,
    TimeSpan,

    // === Speciální ===
    Guid,
    Uri,
    Version,

    // === Placeholder pro komplexní typy ===
    Entity,     // Odkaz na jinou entitu
    EnumValue,  // Odkaz na hodnotu enumu
    Object,     // System.Object — fallback
    Dynamic,    // dynamic — otevřený typ
    Void,       // System.Void
    Array,
    Nullable,
    Struct,
    Record,
}
