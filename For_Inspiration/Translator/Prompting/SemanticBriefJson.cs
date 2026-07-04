using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaForge.Translator;

public sealed class SemanticBriefJson
{
    public string SchemaVersion { get; init; } = "1.0";

    public string BriefId { get; init; } = string.Empty;

    public string SourceTurnId { get; init; } = string.Empty;

    [JsonConverter(typeof(SemanticTranslationIntentJsonConverter))]
    public SemanticTranslationIntent TranslationIntent { get; init; } = new();

    public IReadOnlyList<string> ConversationSummary { get; init; } = [];

    public IReadOnlyList<string> AgreedAssumptions { get; init; } = [];

    public IReadOnlyList<SemanticChangeItem> SemanticChanges { get; init; } = [];

    public IReadOnlyList<SemanticOpenQuestion> OpenQuestions { get; init; } = [];

    public SemanticTranslationHints TranslationHints { get; init; } = new();
}

public sealed class SemanticTranslationIntent
{
    public string State { get; init; } = "None";

    public string Reason { get; init; } = string.Empty;

    public bool CanAutoRun { get; init; }

    public string ManualCommandHint { get; init; } = "translate";
}

public sealed class SemanticChangeItem
{
    public string ChangeId { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public string Kind { get; init; } = string.Empty;

    public Dictionary<string, object?> Target { get; init; } = [];

    public Dictionary<string, object?> Payload { get; init; } = [];

    public double Confidence { get; init; }
}

public sealed class SemanticOpenQuestion
{
    public string Id { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;

    public bool Blocking { get; init; }

    public string Scope { get; init; } = string.Empty;
}

public sealed class SemanticTranslationHints
{
    public string PreferredPatchStyle { get; init; } = "grouped";

    public bool PreferExistingEntitiesByName { get; init; } = true;

    public bool AllowBehaviorNotesForAlgorithmSteps { get; init; } = true;

    public bool PreserveLanguageNeutrality { get; init; } = true;
}

internal sealed class SemanticTranslationIntentJsonConverter : JsonConverter<SemanticTranslationIntent>
{
    public override SemanticTranslationIntent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var state = reader.GetString();
            return new SemanticTranslationIntent
            {
                State = string.IsNullOrWhiteSpace(state) ? "None" : state,
            };
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            return new SemanticTranslationIntent
            {
                State = TryGetString(root, "state") ?? "None",
                Reason = TryGetString(root, "reason") ?? string.Empty,
                CanAutoRun = TryGetBool(root, "canAutoRun"),
                ManualCommandHint = TryGetString(root, "manualCommandHint") ?? "translate",
            };
        }

        throw new JsonException($"Neplatny token {reader.TokenType} pro SemanticTranslationIntent.");
    }

    public override void Write(Utf8JsonWriter writer, SemanticTranslationIntent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("state", value.State);
        writer.WriteString("reason", value.Reason);
        writer.WriteBoolean("canAutoRun", value.CanAutoRun);
        writer.WriteString("manualCommandHint", value.ManualCommandHint);
        writer.WriteEndObject();
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (property.Value.ValueKind == JsonValueKind.String)
                return property.Value.GetString();

            if (property.Value.ValueKind != JsonValueKind.Null)
                return property.Value.GetRawText();
        }

        return null;
    }

    private static bool TryGetBool(JsonElement root, string propertyName)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                continue;

            return property.Value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String when bool.TryParse(property.Value.GetString(), out var parsed) => parsed,
                _ => false,
            };
        }

        return false;
    }
}