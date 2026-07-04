using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CommandSource
{
    Unknown = 0,
    Chat = 1,
    Cli = 2,
    Mcp = 3,
    Import = 4,
    System = 5,
    WebApi = 6,
    Desktop = 7,
}