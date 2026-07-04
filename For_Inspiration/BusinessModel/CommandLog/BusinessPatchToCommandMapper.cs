using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel;

public sealed class BusinessPatchToCommandMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public CommandEnvelope Map(BusinessPatchOperation operation, BusinessPatchCommandContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.StreamId);

        return new CommandEnvelope
        {
            StreamId = context.StreamId,
            ExpectedVersion = context.ExpectedVersion,
            IssuedAt = context.IssuedAt ?? DateTimeOffset.UtcNow,
            IssuedBy = context.IssuedBy,
            Source = context.Source,
            Kind = MapKind(operation.Op),
            Payload = BuildPayload(operation),
            CorrelationId = context.CorrelationId,
            CausationId = context.CausationId,
            MutationId = context.MutationId,
            Provenance = context.Provenance,
        };
    }

    internal static string MapKind(string op)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(op);

        var parts = op.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
            return op.Replace('_', '.');

        var action = parts[0];
        var subject = string.Join('_', parts.Skip(1));
        return $"{subject}.{action}";
    }

    internal static JsonObject BuildPayload(BusinessPatchOperation operation)
    {
        var payload = new JsonObject();

        AddString(payload, "entityId", operation.EntityId);
        AddString(payload, "attributeId", operation.AttributeId);
        AddString(payload, "behaviorId", operation.BehaviorId);
        AddString(payload, "relationId", operation.RelationId);
        AddString(payload, "questionId", operation.QuestionId);
        AddString(payload, "workflowId", operation.WorkflowId);
        AddString(payload, "workflowStepId", operation.WorkflowStepId);
        AddString(payload, "workflowTransitionId", operation.WorkflowTransitionId);

        if (operation.NewIndex is int newIndex)
            payload["newIndex"] = newIndex;

        foreach (var (key, value) in operation.Data)
            payload[key] = ToJsonNode(value);

        return payload;
    }

    private static void AddString(JsonObject payload, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            payload[key] = value;
    }

    private static JsonNode? ToJsonNode(object? value)
    {
        if (value is null)
            return null;

        return JsonSerializer.SerializeToNode(value, JsonOptions)?.DeepClone();
    }
}