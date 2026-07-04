using System.Text.Json;
using System.Text.Json.Nodes;

namespace MetaForge.BusinessModel;

public sealed class CommandEnvelopeToBusinessPatchMapper
{
    public BusinessPatchOperation Map(CommandEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentException.ThrowIfNullOrWhiteSpace(envelope.Kind);

        return new BusinessPatchOperation
        {
            Op = MapOperation(envelope.Kind),
            EntityId = GetString(envelope.Payload, "entityId"),
            AttributeId = GetString(envelope.Payload, "attributeId"),
            BehaviorId = GetString(envelope.Payload, "behaviorId"),
            RelationId = GetString(envelope.Payload, "relationId"),
            QuestionId = GetString(envelope.Payload, "questionId"),
            WorkflowId = GetString(envelope.Payload, "workflowId"),
            WorkflowStepId = GetString(envelope.Payload, "workflowStepId"),
            WorkflowTransitionId = GetString(envelope.Payload, "workflowTransitionId"),
            NewIndex = GetInt(envelope.Payload, "newIndex"),
            Data = BuildData(envelope.Payload),
        };
    }

    internal static string MapOperation(string kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);

        var separatorIndex = kind.IndexOf('.');
        if (separatorIndex <= 0 || separatorIndex >= kind.Length - 1)
            return kind.Replace('.', '_');

        var subject = kind[..separatorIndex];
        var action = kind[(separatorIndex + 1)..];
        return $"{action}_{subject}";
    }

    private static Dictionary<string, object?> BuildData(JsonObject payload)
    {
        var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in payload)
        {
            if (IsReservedPayloadKey(pair.Key))
                continue;

            data[pair.Key] = ToDataValue(pair.Value);
        }

        return data;
    }

    private static bool IsReservedPayloadKey(string key)
    {
        return key.Equals("entityId", StringComparison.OrdinalIgnoreCase)
            || key.Equals("attributeId", StringComparison.OrdinalIgnoreCase)
            || key.Equals("behaviorId", StringComparison.OrdinalIgnoreCase)
            || key.Equals("relationId", StringComparison.OrdinalIgnoreCase)
            || key.Equals("questionId", StringComparison.OrdinalIgnoreCase)
            || key.Equals("workflowId", StringComparison.OrdinalIgnoreCase)
            || key.Equals("workflowStepId", StringComparison.OrdinalIgnoreCase)
            || key.Equals("workflowTransitionId", StringComparison.OrdinalIgnoreCase)
            || key.Equals("newIndex", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetString(JsonObject payload, string key)
    {
        return !TryGetValue(payload, key, out var value)
            ? null
            : value switch
            {
                null => null,
                JsonValue jsonValue when jsonValue.TryGetValue<string>(out var text) => text,
                JsonValue jsonValue when jsonValue.TryGetValue<int>(out var intValue) => intValue.ToString(),
                JsonValue jsonValue when jsonValue.TryGetValue<long>(out var longValue) => longValue.ToString(),
                JsonValue jsonValue when jsonValue.TryGetValue<bool>(out var boolValue) => boolValue.ToString(),
                _ => value.ToJsonString(),
            };
    }

    private static int? GetInt(JsonObject payload, string key)
    {
        if (!TryGetValue(payload, key, out var value) || value is null)
            return null;

        if (value is JsonValue jsonValue)
        {
            if (jsonValue.TryGetValue<int>(out var intValue))
                return intValue;

            if (jsonValue.TryGetValue<long>(out var longValue))
                return checked((int)longValue);

            if (jsonValue.TryGetValue<string>(out var text) && int.TryParse(text, out var parsed))
                return parsed;
        }

        return null;
    }

    private static bool TryGetValue(JsonObject payload, string key, out JsonNode? value)
    {
        foreach (var pair in payload)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    private static object? ToDataValue(JsonNode? node)
    {
        if (node is null)
            return null;

        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }
}