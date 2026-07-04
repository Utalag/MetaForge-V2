using System.Text.Json;

namespace MetaForge.BusinessModel;

public sealed partial class BusinessPatchEngine
{
    private static string RequireString(BusinessPatchOperation operation, string key, string path)
    {
        return GetString(operation, key)
            ?? throw new PatchOperationException("patch.value.required", $"Patch operace vyzaduje hodnotu {key}.", path);
    }

    private static string? GetString(BusinessPatchOperation operation, string key)
    {
        return TryGetDataValue(operation, key, out var value)
            ? ToStringValue(value)
            : null;
    }

    private static int? GetInt(BusinessPatchOperation operation, string key)
    {
        if (!TryGetDataValue(operation, key, out var value) || value is null)
            return null;

        return value switch
        {
            int intValue => intValue,
            long longValue => checked((int)longValue),
            JsonElement { ValueKind: JsonValueKind.Number } json => json.GetInt32(),
            JsonElement { ValueKind: JsonValueKind.String } json when int.TryParse(json.GetString(), out var parsed) => parsed,
            string text when int.TryParse(text, out var parsed) => parsed,
            _ => null,
        };
    }

    private static bool? GetBool(BusinessPatchOperation operation, string key)
    {
        if (!TryGetDataValue(operation, key, out var value) || value is null)
            return null;

        return value switch
        {
            bool boolValue => boolValue,
            JsonElement { ValueKind: JsonValueKind.True } => true,
            JsonElement { ValueKind: JsonValueKind.False } => false,
            JsonElement { ValueKind: JsonValueKind.String } json when bool.TryParse(json.GetString(), out var parsed) => parsed,
            string text when bool.TryParse(text, out var parsed) => parsed,
            _ => null,
        };
    }

    private static TEnum? GetEnum<TEnum>(BusinessPatchOperation operation, string key)
        where TEnum : struct
    {
        var text = GetString(operation, key);
        return Enum.TryParse<TEnum>(text, ignoreCase: true, out var parsed) ? parsed : null;
    }

    private static IReadOnlyList<string> GetStringList(BusinessPatchOperation operation, string key)
    {
        if (!TryGetDataValue(operation, key, out var value) || value is null)
            return [];

        return value switch
        {
            JsonElement { ValueKind: JsonValueKind.Array } array => array.EnumerateArray().Select(item => ToStringValue(item)).Where(item => !string.IsNullOrWhiteSpace(item)).Cast<string>().ToArray(),
            IEnumerable<object?> items => items.Select(ToStringValue).Where(item => !string.IsNullOrWhiteSpace(item)).Cast<string>().ToArray(),
            string text => text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            _ => ToStringValue(value) is { Length: > 0 } single ? [single] : [],
        };
    }

    private static IReadOnlyList<BusinessBehaviorInputNode> ParseBehaviorInputList(BusinessPatchOperation operation, string key)
    {
        if (!TryGetDataValue(operation, key, out var value) || value is null)
            return [];

        var items = new List<BusinessBehaviorInputNode>();

        if (value is JsonElement { ValueKind: JsonValueKind.Array } array)
        {
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                items.Add(new BusinessBehaviorInputNode
                {
                    Id = item.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                    Name = item.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                    Type = item.TryGetProperty("type", out var type) ? type.GetString() ?? "text" : "text",
                    Required = item.TryGetProperty("required", out var required) && required.ValueKind == JsonValueKind.True,
                    Summary = item.TryGetProperty("summary", out var summary) ? summary.GetString() : null,
                });
            }

            return items;
        }

        if (value is IEnumerable<object?> enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is not Dictionary<string, object?> dictionary)
                    continue;

                items.Add(new BusinessBehaviorInputNode
                {
                    Id = dictionary.TryGetValue("id", out var id) ? ToStringValue(id) ?? string.Empty : string.Empty,
                    Name = dictionary.TryGetValue("name", out var name) ? ToStringValue(name) ?? string.Empty : string.Empty,
                    Type = dictionary.TryGetValue("type", out var type) ? ToStringValue(type) ?? "text" : "text",
                    Required = dictionary.TryGetValue("required", out var required) && required is true,
                    Summary = dictionary.TryGetValue("summary", out var summary) ? ToStringValue(summary) : null,
                });
            }
        }

        return items;
    }

    private static IReadOnlyList<BusinessAttributeNode> ParseAttributeList(BusinessPatchOperation operation, string key)
    {
        if (!TryGetDataValue(operation, key, out var value) || value is null)
            return [];

        var items = new List<BusinessAttributeNode>();

        if (value is JsonElement { ValueKind: JsonValueKind.Array } array)
        {
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                items.Add(new BusinessAttributeNode
                {
                    Id = item.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                    Name = item.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                    Type = item.TryGetProperty("type", out var type) ? type.GetString() ?? "text" : "text",
                    CustomType = item.TryGetProperty("customType", out var customType) ? customType.GetString() : null,
                    Required = item.TryGetProperty("required", out var required) && required.ValueKind == JsonValueKind.True,
                    Summary = item.TryGetProperty("summary", out var summary) ? summary.GetString() : null,
                    DefaultValue = item.TryGetProperty("defaultValue", out var defaultValue) ? defaultValue.GetString() : null,
                    Constraints = item.TryGetProperty("constraints", out var constraints) && constraints.ValueKind == JsonValueKind.Array
                        ? constraints.EnumerateArray().Select(element => element.GetString()).Where(text => !string.IsNullOrWhiteSpace(text)).Cast<string>().ToArray()
                        : [],
                    Computed = item.TryGetProperty("computed", out var computed) ? computed.GetString() : null,
                    PresetId = item.TryGetProperty("presetId", out var presetId) ? presetId.GetString() : null,
                });
            }

            return items;
        }

        if (value is IEnumerable<object?> enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is not Dictionary<string, object?> dictionary)
                    continue;

                items.Add(new BusinessAttributeNode
                {
                    Id = dictionary.TryGetValue("id", out var id) ? ToStringValue(id) ?? string.Empty : string.Empty,
                    Name = dictionary.TryGetValue("name", out var name) ? ToStringValue(name) ?? string.Empty : string.Empty,
                    Type = dictionary.TryGetValue("type", out var type) ? ToStringValue(type) ?? "text" : "text",
                    CustomType = dictionary.TryGetValue("customType", out var customType) ? ToStringValue(customType) : null,
                    Required = dictionary.TryGetValue("required", out var required) && required is true,
                    Summary = dictionary.TryGetValue("summary", out var summary) ? ToStringValue(summary) : null,
                    DefaultValue = dictionary.TryGetValue("defaultValue", out var defaultValue) ? ToStringValue(defaultValue) : null,
                    Constraints = dictionary.TryGetValue("constraints", out var constraints)
                        ? constraints switch
                        {
                            IEnumerable<object?> constraintItems => constraintItems.Select(ToStringValue).Where(text => !string.IsNullOrWhiteSpace(text)).Cast<string>().ToArray(),
                            _ => ToStringValue(constraints) is { Length: > 0 } single ? [single] : [],
                        }
                        : [],
                    Computed = dictionary.TryGetValue("computed", out var computed) ? ToStringValue(computed) : null,
                    PresetId = dictionary.TryGetValue("presetId", out var presetId) ? ToStringValue(presetId) : null,
                });
            }
        }

        return items;
    }

    private static IReadOnlyList<BusinessBehaviorNode> ParseBehaviorList(BusinessPatchOperation operation, string key)
    {
        if (!TryGetDataValue(operation, key, out var value) || value is null)
            return [];

        var items = new List<BusinessBehaviorNode>();

        if (value is JsonElement { ValueKind: JsonValueKind.Array } array)
        {
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                var inputs = item.TryGetProperty("inputs", out var inputsProperty) && inputsProperty.ValueKind == JsonValueKind.Array
                    ? inputsProperty.EnumerateArray().Select(element => new BusinessBehaviorInputNode
                    {
                        Id = element.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                        Name = element.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                        Type = element.TryGetProperty("type", out var type) ? type.GetString() ?? "text" : "text",
                        Required = element.TryGetProperty("required", out var required) && required.ValueKind == JsonValueKind.True,
                        Summary = element.TryGetProperty("summary", out var summary) ? summary.GetString() : null,
                    }).ToArray()
                    : [];

                items.Add(new BusinessBehaviorNode
                {
                    Id = item.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                    Name = item.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                    Kind = item.TryGetProperty("kind", out var kind) && Enum.TryParse<BusinessBehaviorKind>(kind.GetString(), true, out var parsedKind)
                        ? parsedKind
                        : BusinessBehaviorKind.Query,
                    Summary = item.TryGetProperty("summary", out var summary) ? summary.GetString() : null,
                    Inputs = inputs,
                    Returns = item.TryGetProperty("returns", out var returns) ? returns.GetString() : null,
                    Notes = item.TryGetProperty("notes", out var notesProperty) && notesProperty.ValueKind == JsonValueKind.Array
                        ? notesProperty.EnumerateArray().Select(element => new BusinessNoteNode
                        {
                            Id = element.TryGetProperty("id", out var noteId) ? noteId.GetString() ?? string.Empty : string.Empty,
                            Text = element.TryGetProperty("text", out var noteText) ? noteText.GetString() ?? string.Empty : string.Empty,
                        }).ToArray()
                        : [],
                });
            }

            return items;
        }

        if (value is IEnumerable<object?> enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is not Dictionary<string, object?> dictionary)
                    continue;

                var inputs = dictionary.TryGetValue("inputs", out var inputsValue)
                    ? inputsValue switch
                    {
                        IEnumerable<object?> inputItems => inputItems
                            .OfType<Dictionary<string, object?>>()
                            .Select(input => new BusinessBehaviorInputNode
                            {
                                Id = input.TryGetValue("id", out var id) ? ToStringValue(id) ?? string.Empty : string.Empty,
                                Name = input.TryGetValue("name", out var name) ? ToStringValue(name) ?? string.Empty : string.Empty,
                                Type = input.TryGetValue("type", out var type) ? ToStringValue(type) ?? "text" : "text",
                                Required = input.TryGetValue("required", out var required) && required is true,
                                Summary = input.TryGetValue("summary", out var summary) ? ToStringValue(summary) : null,
                            })
                            .ToArray(),
                        _ => [],
                    }
                    : [];

                items.Add(new BusinessBehaviorNode
                {
                    Id = dictionary.TryGetValue("id", out var id) ? ToStringValue(id) ?? string.Empty : string.Empty,
                    Name = dictionary.TryGetValue("name", out var name) ? ToStringValue(name) ?? string.Empty : string.Empty,
                    Kind = dictionary.TryGetValue("kind", out var kind) && Enum.TryParse<BusinessBehaviorKind>(ToStringValue(kind), true, out var parsedKind)
                        ? parsedKind
                        : BusinessBehaviorKind.Query,
                    Summary = dictionary.TryGetValue("summary", out var summary) ? ToStringValue(summary) : null,
                    Inputs = inputs,
                    Returns = dictionary.TryGetValue("returns", out var returns) ? ToStringValue(returns) : null,
                    Notes = [],
                });
            }
        }

        return items;
    }

    private static IReadOnlyList<BusinessNoteNode> ParseNoteList(BusinessPatchOperation operation, string key)
    {
        if (!TryGetDataValue(operation, key, out var value) || value is null)
            return [];

        if (value is JsonElement { ValueKind: JsonValueKind.Array } array)
        {
            return array.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Object)
                .Select(item => new BusinessNoteNode
                {
                    Id = item.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                    Text = item.TryGetProperty("text", out var text) ? text.GetString() ?? string.Empty : string.Empty,
                })
                .ToArray();
        }

        return [];
    }

    private static bool TryGetDataValue(BusinessPatchOperation operation, string key, out object? value)
    {
        foreach (var pair in operation.Data)
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

    private static bool HasValue(BusinessPatchOperation operation, string key)
    {
        return TryGetDataValue(operation, key, out _);
    }

    private static string NormalizeOperation(string operation)
    {
        return string.IsNullOrWhiteSpace(operation)
            ? string.Empty
            : operation.Trim().ToLowerInvariant();
    }

    private static bool IsError(BusinessValidationIssue issue)
    {
        return string.Equals(issue.Severity, "Error", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ToStringValue(object? value)
    {
        return value switch
        {
            null => null,
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } json => json.GetString(),
            JsonElement { ValueKind: JsonValueKind.Number } json => json.GetRawText(),
            JsonElement { ValueKind: JsonValueKind.True } => bool.TrueString,
            JsonElement { ValueKind: JsonValueKind.False } => bool.FalseString,
            JsonElement { ValueKind: JsonValueKind.Null } => null,
            _ => value.ToString(),
        };
    }

    private static IReadOnlyList<string> ParseStringList(BusinessPatchOperation operation, string key)
    {
        if (!TryGetDataValue(operation, key, out var value) || value is null)
            return [];

        if (value is JsonElement { ValueKind: JsonValueKind.Array } jsonArray)
        {
            var result = new List<string>();
            foreach (var element in jsonArray.EnumerateArray())
            {
                var str = element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString();
                if (!string.IsNullOrWhiteSpace(str))
                    result.Add(str);
            }
            return result;
        }

        if (value is IEnumerable<string> stringEnumerable)
            return stringEnumerable.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        var text = value.ToString();
        if (string.IsNullOrWhiteSpace(text))
            return [];

        return text.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static void InsertAtOrAppend<T>(IList<T> items, T item, int? index)
    {
        if (index.HasValue && index.Value >= 0 && index.Value <= items.Count)
        {
            items.Insert(index.Value, item);
            return;
        }

        items.Add(item);
    }
}
