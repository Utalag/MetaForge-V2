using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkflowBindingSyncState
{
    New = 0,
    Synced = 1,
    BusinessEdited = 2,
    BindingEdited = 3,
    Conflict = 4,
}