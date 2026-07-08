using System.Collections.ObjectModel;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.ValueObjects;

/// <summary>
/// StrongType — metamodel pro silně typovaný wrapper (Vogen struct).
/// Popisuje strongly-typed wrapper kolem libovolného TypeModelu
/// s validací, konverzemi a domain-specific metodami.
/// </summary>
public class StrongType : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private string _namespace = string.Empty;
    private string _description = string.Empty;
    private TypeModel _underlyingType = new() { BaseType = DataType.Int };
    private ConversionOptions _conversions = ConversionOptions.All;
    private bool _generateComparisons = true;

    /// <summary>Název Value Object (např. ProductId, Email, Money).</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Namespace.</summary>
    public string Namespace
    {
        get => _namespace;
        set
        {
            if (_namespace != value)
            {
                _namespace = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Popis Value Object (pro XML dokumentaci).</summary>
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Podkladový typ — může být primitiv i složený typ (List, Dictionary, …).</summary>
    public TypeModel UnderlyingType
    {
        get => _underlyingType;
        set
        {
            if (_underlyingType != value)
            {
                _underlyingType = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Automatické konverze (JSON, EF Core, Dapper).</summary>
    public ConversionOptions Conversions
    {
        get => _conversions;
        set
        {
            if (_conversions != value)
            {
                _conversions = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Generovat IComparable implementaci?</summary>
    public bool GenerateComparisons
    {
        get => _generateComparisons;
        set
        {
            if (_generateComparisons != value)
            {
                _generateComparisons = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Validační pravidla.</summary>
    public ObservableCollection<ValueObjectValidationRule> ValidationRules { get; } = new();

    /// <summary>Vlastní metody (domain-specific, např. GetDomain() pro Email).</summary>
    public ObservableCollection<string> CustomMethods { get; } = new();

    /// <summary>Using direktivy navíc.</summary>
    public ObservableCollection<string> AdditionalUsings { get; } = new();

    /// <summary>
    /// Vrátí syntaxi typu pro daný jazyk.
    /// Pojmenovaný StrongType (VO) vrátí Name; jinak deleguje na UnderlyingType.
    /// </summary>
    public string GetSyntax(ProgramLanguage language) =>
        !string.IsNullOrEmpty(Name) ? Name : UnderlyingType.GetSyntax(language);

    /// <summary>
    /// Sestaví model pro Scriban šablonu.
    /// </summary>
    private Dictionary<string, object> BuildTemplateModel()
    {
        return new Dictionary<string, object>
        {
            { "name", Name },
            { "namespace_name", Namespace },
            { "description", Description },
            { "underlying_type", GetBaseTypeSyntax() },
            { "conversions", BuildConversionsString() },
            { "generate_comparisons", GenerateComparisons },
            { "validation_rules", ValidationRules.Select(r => new Dictionary<string, object?>
                {
                    { "rule_type", r.RuleType.ToString() },
                    { "error_message", r.ErrorMessage },
                    { "parameter", r.Parameter }
                }).ToList()
            },
            { "has_validation_rules", ValidationRules.Count > 0 },
            { "custom_methods", CustomMethods.ToList() },
            { "has_custom_methods", CustomMethods.Count > 0 },
            { "additional_usings", AdditionalUsings.ToList() },
            { "has_additional_usings", AdditionalUsings.Count > 0 }
        };
    }

    private string GetBaseTypeSyntax() => UnderlyingType.GetSyntax(TargetLanguage);

    private string BuildConversionsString()
    {
        var parts = new List<string>();
        if (Conversions.HasFlag(ConversionOptions.Default))
            parts.Add("Conversions.Default");
        if (Conversions.HasFlag(ConversionOptions.SystemTextJson))
            parts.Add("Conversions.SystemTextJson");
        if (Conversions.HasFlag(ConversionOptions.NewtonsoftJson))
            parts.Add("Conversions.NewtonsoftJson");
        if (Conversions.HasFlag(ConversionOptions.EfCoreValueConverter))
            parts.Add("Conversions.EfCoreValueConverter");
        if (Conversions.HasFlag(ConversionOptions.DapperTypeHandler))
            parts.Add("Conversions.DapperTypeHandler");

        return parts.Count > 0 ? string.Join(" | ", parts) : "Conversions.Default";
    }
}
