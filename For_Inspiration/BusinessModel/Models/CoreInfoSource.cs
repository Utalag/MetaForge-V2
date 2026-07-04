using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

/// <summary>
/// Oznacuje puvod informace v commandu — zda byla zadana rucne (Manual), vygenerovana AI (Generated),
/// nebo je kombinaci obou (Hybrid). Pouziva se pri vypoctu AttributeSyncState behem replay
/// jako prerekvizita pro CoreDetail write-back.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoreInfoSource
{
    Unknown = 0,

    /// <summary>Hodnota byla zadana nebo upravena uzivatelsky (rucne).</summary>
    Manual = 1,

    /// <summary>Hodnota byla vygenerovana AI bez manualniho zasahu.</summary>
    Generated = 2,

    /// <summary>Hodnota kombinuje AI generaci a manualní upravu.</summary>
    Hybrid = 3,
}
