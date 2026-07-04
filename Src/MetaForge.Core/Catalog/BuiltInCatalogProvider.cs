using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Catalog;

/// <summary>
/// Vestavěný katalog — obsahuje základní mapování běžných názvů na TypeModel.
/// </summary>
public sealed class BuiltInCatalogProvider : ICatalogProvider
{
    public string ProviderName => "BuiltIn";

    private readonly Dictionary<string, PresetDefinition> _presets;

    public BuiltInCatalogProvider()
    {
        _presets = new Dictionary<string, PresetDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            // Číselné
            ["int"] = new("int", TypeModel.Int32, "32bitové celé číslo"),
            ["long"] = new("long", TypeModel.Of(DataType.Int64), "64bitové celé číslo"),
            ["decimal"] = new("decimal", TypeModel.Decimal, "Desetinné číslo s pevnou řádovou čárkou"),
            ["double"] = new("double", TypeModel.Of(DataType.Double), "Desetinné číslo s plovoucí řádovou čárkou"),
            ["float"] = new("float", TypeModel.Of(DataType.Single), "32bitové desetinné číslo"),

            // Textové
            ["string"] = new("string", TypeModel.String, "Textový řetězec"),
            ["text"] = new("text", TypeModel.String, "Textový řetězec (alias)"),

            // Logické
            ["bool"] = new("bool", TypeModel.Bool, "Pravdivostní hodnota"),
            ["boolean"] = new("boolean", TypeModel.Bool, "Pravdivostní hodnota (alias)"),

            // Časové
            ["datetime"] = new("datetime", TypeModel.DateTime, "Datum a čas"),
            ["date"] = new("date", TypeModel.Of(DataType.DateOnly), "Pouze datum"),
            ["time"] = new("time", TypeModel.Of(DataType.TimeOnly), "Pouze čas"),

            // Speciální
            ["guid"] = new("guid", TypeModel.Guid, "Globálně unikátní identifikátor"),
            ["uuid"] = new("uuid", TypeModel.Guid, "Globálně unikátní identifikátor (alias)"),
            ["email"] = new("email", TypeModel.String, "Emailová adresa", new[] { "contact", "validation" }),
            ["phone"] = new("phone", TypeModel.String, "Telefonní číslo", new[] { "contact" }),
            ["url"] = new("url", TypeModel.Of(DataType.Uri), "URL adresa"),
            ["uri"] = new("uri", TypeModel.Of(DataType.Uri), "URI adresa (alias)"),
            ["money"] = new("money", TypeModel.Decimal, "Peněžní částka", new[] { "finance" }),
            ["price"] = new("price", TypeModel.Decimal, "Cena", new[] { "finance" }),
        };
    }

    public IReadOnlyList<PresetDefinition> GetAllPresets() =>
        _presets.Values.ToList().AsReadOnly();

    public PresetDefinition? ResolveType(string typeName) =>
        _presets.TryGetValue(typeName, out var preset) ? preset : null;
}
