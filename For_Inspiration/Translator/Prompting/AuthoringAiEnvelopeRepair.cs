using System.Text.Json;
using MetaForge.BusinessModel;

namespace MetaForge.Translator;

internal static class AuthoringAiEnvelopeRepair
{
    public static (AuthoringResponseEnvelope? Envelope, bool PayloadRepaired) TryDeserialize(
        string payload,
        JsonSerializerOptions jsonOptions)
    {
        if (TryDeserializePayload(payload, jsonOptions, out var envelope))
            return (NormalizeEnvelope(envelope), false);

        var repairedPayload = AiJsonEnvelopeExtractor.RepairCommonJsonIssues(payload);
        if (!string.Equals(payload, repairedPayload, StringComparison.Ordinal)
            && TryDeserializePayload(repairedPayload, jsonOptions, out envelope))
        {
            return (AddRepairWarning(NormalizeEnvelope(envelope)), true);
        }

        return (null, false);
    }

    private static bool TryDeserializePayload(
        string payload,
        JsonSerializerOptions jsonOptions,
        out AuthoringResponseEnvelope envelope)
    {
        try
        {
            envelope = JsonSerializer.Deserialize<AuthoringResponseEnvelope>(payload, jsonOptions)
                ?? new AuthoringResponseEnvelope
                {
                    Mode = AuthoringResponseMode.Answer,
                    AssistantMessage = payload,
                };

            return true;
        }
        catch (JsonException)
        {
            envelope = default!;
            return false;
        }
    }

    private static AuthoringResponseEnvelope NormalizeEnvelope(AuthoringResponseEnvelope envelope)
    {
        var normalizedPatches = envelope.Patches.Select(NormalizePatch).ToArray();

        return new AuthoringResponseEnvelope
        {
            Mode = envelope.Mode,
            AssistantMessage = envelope.AssistantMessage,
            Questions = envelope.Questions,
            Warnings = envelope.Warnings,
            Patches = normalizedPatches,
        };
    }

    private static AuthoringResponseEnvelope AddRepairWarning(AuthoringResponseEnvelope envelope)
    {
        var warnings = envelope.Warnings
            .Append("AI authoring JSON obalka obsahovala drobnou syntaktickou chybu a byla automaticky opravena.")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new AuthoringResponseEnvelope
        {
            Mode = envelope.Mode,
            AssistantMessage = envelope.AssistantMessage,
            Questions = envelope.Questions,
            Warnings = warnings,
            Patches = envelope.Patches,
        };
    }

    private static BusinessPatchOperation NormalizePatch(BusinessPatchOperation patch)
    {
        var normalizedData = NormalizePatchData(patch.Data);
        var normalizedOperation = NormalizeOperationName(patch, normalizedData);

        return new BusinessPatchOperation
        {
            Op = normalizedOperation,
            EntityId = patch.EntityId,
            AttributeId = patch.AttributeId,
            BehaviorId = patch.BehaviorId,
            RelationId = patch.RelationId,
            QuestionId = patch.QuestionId,
            NewIndex = patch.NewIndex,
            Data = normalizedData,
        };
    }

    private static Dictionary<string, object?> NormalizePatchData(IReadOnlyDictionary<string, object?> source)
    {
        var data = new Dictionary<string, object?>(source, StringComparer.OrdinalIgnoreCase);

        PromoteAlias(data, "summary", "description");
        PromoteAlias(data, "sourceEntityId", "fromEntity");
        PromoteAlias(data, "targetEntityId", "toEntity");

        if (!HasNonEmptyValue(data, "kind")
            && TryGetStringValue(data, "cardinality", out var cardinality))
        {
            data["kind"] = MapCardinalityToRelationKind(cardinality);
        }

        return data;
    }

    private static string NormalizeOperationName(BusinessPatchOperation patch, IReadOnlyDictionary<string, object?> data)
    {
        var canonicalOperation = CanonicalizeOperation(patch.Op);
        var aliasKey = NormalizeToken(canonicalOperation);

        return aliasKey switch
        {
            "add" or "create" or "insert" => ResolveOperationFamily("add", patch, data),
            "update" or "edit" or "modify" => ResolveOperationFamily("update", patch, data),
            "delete" or "remove" => ResolveOperationFamily("delete", patch, data),
            _ => canonicalOperation,
        };
    }

    private static string ResolveOperationFamily(
        string family,
        BusinessPatchOperation patch,
        IReadOnlyDictionary<string, object?> data)
    {
        if (!string.IsNullOrWhiteSpace(patch.RelationId)
            || HasNonEmptyValue(data, "sourceEntityId")
            || HasNonEmptyValue(data, "targetEntityId"))
        {
            return $"{family}_relation";
        }

        if (!string.IsNullOrWhiteSpace(patch.BehaviorId))
            return $"{family}_behavior";

        if (!string.IsNullOrWhiteSpace(patch.AttributeId))
            return family switch
            {
                "add" => "add_attribute",
                "update" => "update_attribute",
                "delete" => "delete_attribute",
                _ => $"{family}_attribute",
            };

        if (!string.IsNullOrWhiteSpace(patch.QuestionId) && family == "update")
            return "resolve_question";

        return $"{family}_entity";
    }

    private static void PromoteAlias(Dictionary<string, object?> data, string targetKey, string sourceKey)
    {
        if (HasNonEmptyValue(data, targetKey))
            return;

        if (TryGetValue(data, sourceKey, out var value))
            data[targetKey] = NormalizeAliasValue(value);
    }

    private static string MapCardinalityToRelationKind(string cardinality)
    {
        return NormalizeToken(cardinality) switch
        {
            "onetomany" => BusinessRelationKind.HasMany.ToString(),
            "manytoone" => BusinessRelationKind.BelongsTo.ToString(),
            "onetoone" => BusinessRelationKind.HasOne.ToString(),
            "manytomany" => BusinessRelationKind.ManyToMany.ToString(),
            _ => BusinessRelationKind.BelongsTo.ToString(),
        };
    }

    private static bool HasNonEmptyValue(IReadOnlyDictionary<string, object?> data, string key)
    {
        return TryGetStringValue(data, key, out _)
            || TryGetValue(data, key, out var value) && value is not null;
    }

    private static bool TryGetStringValue(IReadOnlyDictionary<string, object?> data, string key, out string value)
    {
        if (TryGetValue(data, key, out var rawValue))
        {
            var converted = rawValue switch
            {
                string text => text,
                JsonElement { ValueKind: JsonValueKind.String } json => json.GetString(),
                JsonElement { ValueKind: JsonValueKind.Number } json => json.GetRawText(),
                JsonElement { ValueKind: JsonValueKind.True } => bool.TrueString,
                JsonElement { ValueKind: JsonValueKind.False } => bool.FalseString,
                _ => rawValue?.ToString(),
            };

            if (!string.IsNullOrWhiteSpace(converted))
            {
                value = converted;
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static bool TryGetValue(IReadOnlyDictionary<string, object?> data, string key, out object? value)
    {
        foreach (var pair in data)
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

    private static string NormalizeToken(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToLowerInvariant();
    }

    private static string CanonicalizeOperation(string? operation)
    {
        return string.IsNullOrWhiteSpace(operation)
            ? string.Empty
            : operation.Trim().Replace('-', '_').ToLowerInvariant();
    }

    private static object? NormalizeAliasValue(object? value)
    {
        return value switch
        {
            JsonElement { ValueKind: JsonValueKind.String } json => json.GetString(),
            JsonElement { ValueKind: JsonValueKind.Number } json => json.GetRawText(),
            JsonElement { ValueKind: JsonValueKind.True } => true,
            JsonElement { ValueKind: JsonValueKind.False } => false,
            JsonElement { ValueKind: JsonValueKind.Null } => null,
            _ => value,
        };
    }
}