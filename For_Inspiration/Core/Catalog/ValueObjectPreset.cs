using MetaForge.Core.DataTypes;
using MetaForge.Core.ValueObjects;

namespace MetaForge.Core.Catalog;

/// <summary>
/// JSON model pro Value Object preset.
/// Deserializuje se z .vo.json souboru.
/// </summary>
public class ValueObjectPreset
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = "MetaForge";
    public string Icon { get; set; } = "📦";
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public int CreditCost { get; set; }

    /// <summary>Parametrizovatelné vstupy.</summary>
    public Dictionary<string, PresetParameter>? Parameters { get; set; }

    /// <summary>Definice Value Object.</summary>
    public ValueObjectPresetDefinition Definition { get; set; } = new();

    /// <summary>
    /// Převede preset na StrongType pro generování kódu.
    /// </summary>
    public StrongType ToStrongType(string targetNamespace)
    {
        var vo = new StrongType
        {
            Name = Definition.Name,
            Namespace = targetNamespace,
            Description = Description,
            UnderlyingType = ParseUnderlyingType(Definition.UnderlyingType),
            Conversions = ParseConversions(Definition.Conversions),
            GenerateComparisons = Definition.GenerateComparisons
        };

        if (Definition.AdditionalUsings != null)
        {
            foreach (var u in Definition.AdditionalUsings)
                vo.AdditionalUsings.Add(u);
        }

        foreach (var rule in Definition.ValidationRules)
        {
            vo.ValidationRules.Add(new ValueObjectValidationRule
            {
                RuleType = Enum.Parse<ValidationRuleType>(rule.RuleType),
                ErrorMessage = rule.ErrorMessage,
                Parameter = rule.Parameter
            });
        }

        if (Definition.CustomMethods != null)
        {
            foreach (var method in Definition.CustomMethods)
                vo.CustomMethods.Add(method);
        }

        return vo;
    }

    private static TypeModel ParseUnderlyingType(string type)
    {
        var dataType = type.ToLowerInvariant() switch
        {
            "int"     => DataType.Int,
            "long"    => DataType.Long,
            "guid"    => DataType.Guid,
            "string"  => DataType.String,
            "decimal" => DataType.Decimal,
            "double"  => DataType.Double,
            "short"   => DataType.Short,
            "byte"    => DataType.Byte,
            "float"   => DataType.Float,
            "bool"    => DataType.Boolean,
            _         => DataType.Int
        };
        return new TypeModel { BaseType = dataType };
    }

    private static ConversionOptions ParseConversions(List<string> conversions)
    {
        var result = ConversionOptions.None;
        foreach (var c in conversions)
        {
            if (Enum.TryParse<ConversionOptions>(c, ignoreCase: true, out var parsed))
                result |= parsed;
        }
        return result == ConversionOptions.None ? ConversionOptions.Default : result;
    }
}

/// <summary>
/// Definice Value Object v JSON presetu.
/// </summary>
public class ValueObjectPresetDefinition
{
    public string Name { get; set; } = string.Empty;
    public string UnderlyingType { get; set; } = "int";
    public List<string> Conversions { get; set; } = new();
    public bool GenerateComparisons { get; set; }
    public List<string>? AdditionalUsings { get; set; }
    public List<ValidationRulePreset> ValidationRules { get; set; } = new();
    public List<string>? CustomMethods { get; set; }
}

/// <summary>
/// Validační pravidlo v JSON presetu.
/// </summary>
public class ValidationRulePreset
{
    public string RuleType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? Parameter { get; set; }
    public string? Condition { get; set; }
}
