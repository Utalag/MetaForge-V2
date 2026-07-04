using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

public static class BusinessDocumentJsonSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static BusinessAuthoringDocument Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<BusinessAuthoringDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException("JSON dokument authoring modelu je prazdny nebo nevalidni.");
    }

    public static string Serialize(BusinessAuthoringDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return JsonSerializer.Serialize(document, JsonOptions);
    }
}