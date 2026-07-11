namespace MetaForge.Core.ValueObjects;

/// <summary>
/// Flags enum mapující Vogen <c>Conversions</c> flags.
/// Určuje, jaké konvertory/serializátory Vogen source-generuje pro daný value object.
/// </summary>
/// <remarks>
/// Hodnoty odpovídají Vogen <c>Conversions</c> enum (SteveDunn/Vogen).
/// <see href="https://github.com/SteveDunn/Vogen"/>
/// </remarks>
[Flags]
public enum VogenConversions
{
    /// <summary>Žádné konvertory — jen holý value object.</summary>
    None = 0,

    // === Serializace ===

    /// <summary><c>System.ComponentModel.TypeConverter</c> — pro ASP.NET MVC routing.</summary>
    TypeConverter = 1,

    /// <summary><c>System.Text.Json.JsonConverter</c> — moderní JSON serializace.</summary>
    SystemTextJson = 2,

    /// <summary><c>Newtonsoft.Json.JsonConverter</c> — legacy JSON serializace.</summary>
    NewtonsoftJson = 4,

    // === Data access ===

    /// <summary>EF Core <c>ValueConverter</c> + <c>ValueComparer</c>.</summary>
    EfCoreValueConverter = 8,

    /// <summary>Dapper <c>SqlMapper.TypeHandler</c>.</summary>
    DapperTypeHandler = 16,

    /// <summary>LINQ to DB value converter.</summary>
    LinqToDbValueConverter = 32,

    // === NoSQL ===

    /// <summary>MongoDB BSON serializer.</summary>
    Bson = 64,

    // === Binary & distributed ===

    /// <summary>MessagePack formatter.</summary>
    MessagePack = 128,

    /// <summary>Microsoft Orleans codec + copier (.NET 8+).</summary>
    Orleans = 256,

    // === Ostatní ===

    /// <summary><c>IXmlSerializable</c> implementace.</summary>
    XmlSerializable = 512,

    /// <summary>ServiceStack.Text serializer.</summary>
    ServiceStackDotText = 1024,
}
