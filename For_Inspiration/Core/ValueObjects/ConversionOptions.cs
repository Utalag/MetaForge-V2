namespace MetaForge.Core.ValueObjects;

/// <summary>
/// Volby automatických konverzí pro Value Object.
/// Odpovídá Vogen Conversions flags enum.
/// </summary>
[Flags]
public enum ConversionOptions
{
    None = 0,

    /// <summary>Výchozí konverze (TypeConverter + ToString).</summary>
    Default = 1,

    /// <summary>System.Text.Json konvertor.</summary>
    SystemTextJson = 2,

    /// <summary>Newtonsoft.Json konvertor.</summary>
    NewtonsoftJson = 4,

    /// <summary>EF Core ValueConverter.</summary>
    EfCoreValueConverter = 8,

    /// <summary>Dapper TypeHandler.</summary>
    DapperTypeHandler = 16,

    /// <summary>Všechny běžné konverze.</summary>
    All = Default | SystemTextJson | EfCoreValueConverter
}
