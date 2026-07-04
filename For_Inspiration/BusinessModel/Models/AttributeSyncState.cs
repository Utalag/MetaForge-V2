using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttributeSyncState
{
    New = 0,
    Synced = 1,
    BusinessEdited = 2,
    CoreEdited = 3,
    Conflict = 4,
}
