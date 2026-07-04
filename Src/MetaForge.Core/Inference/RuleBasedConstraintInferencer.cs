using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Inference;

/// <summary>
/// Deterministická implementace inference constraintů pomocí pravidel.
/// </summary>
public sealed class RuleBasedConstraintInferencer : IConstraintInferencer
{
    private static readonly Dictionary<string, string[]> Rules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["email"] = new[] { "email_format", "not_empty", "max_length:254" },
        ["phone"] = new[] { "phone_format", "not_empty" },
        ["url"] = new[] { "url_format" },
        ["age"] = new[] { "range:0-150", "not_negative" },
        ["price"] = new[] { "not_negative", "decimal_places:2" },
        ["quantity"] = new[] { "not_negative", "integer" },
        ["name"] = new[] { "not_empty", "max_length:200" },
        ["description"] = new[] { "max_length:4000" },
        ["password"] = new[] { "min_length:8", "not_empty" },
        ["zipcode"] = new[] { "zip_format" },
        ["color"] = new[] { "hex_color_format" },
        ["percentage"] = new[] { "range:0-100" },
    };

    public IReadOnlyList<string> Infer(string attributeName, TypeModel type)
    {
        // Přesná shoda
        if (Rules.TryGetValue(attributeName, out var constraints))
            return constraints;

        // Prefixová shoda (např. "emailAddress" → "email")
        foreach (var (key, value) in Rules)
        {
            if (attributeName.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                return value;
        }

        // Inferuje podle typu
        if (type.BaseType == DataType.String && type.IsNullable == false)
            return new[] { "not_empty" };

        return Array.Empty<string>();
    }
}
