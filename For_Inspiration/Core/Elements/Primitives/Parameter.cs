using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.ValueObjects;

namespace MetaForge.Core.Elements.Primitives;

/// <summary>
/// Parametr metody nebo konstruktoru.
/// </summary>
public class Parameter : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private TypeModel _type = new();
    private string? _defaultValue;
    private string _description = string.Empty;
    private StrongType? _strongTypeRef;
    private bool _isOut;
    private bool _isRef;
    private bool _isParams;

    /// <summary>
    /// Název parametru.
    /// </summary>
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

    /// <summary>
    /// Typ parametru.
    /// </summary>
    public TypeModel Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                _type.TargetLanguage = TargetLanguage;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Výchozí hodnota (volitelné).
    /// </summary>
    public string? DefaultValue
    {
        get => _defaultValue;
        set
        {
            if (_defaultValue != value)
            {
                _defaultValue = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Popis parametru (pro dokumentaci).
    /// </summary>
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

    /// <summary>
    /// Volitelný odkaz na StrongType (Value Object).
    /// Pokud je nastaven, GenerateCode() použije jeho syntaxi místo Type.
    /// </summary>
    public StrongType? StrongTypeRef
    {
        get => _strongTypeRef;
        set
        {
            if (_strongTypeRef != value)
            {
                _strongTypeRef = value;
                if (_strongTypeRef != null)
                    _strongTypeRef.TargetLanguage = TargetLanguage;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Je parametr out? (C# out, Python return value)
    /// </summary>
    public bool IsOut
    {
        get => _isOut;
        set { _isOut = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Je parametr ref? (C# ref only, ne out)
    /// </summary>
    public bool IsRef
    {
        get => _isRef;
        set { _isRef = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Je parametr params pole? (C# params)
    /// </summary>
    public bool IsParams
    {
        get => _isParams;
        set { _isParams = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Vrátí efektivní syntaxi typu: StrongTypeRef má přednost před Type.
    /// </summary>
    public string EffectiveTypeSyntax =>
        StrongTypeRef != null
            ? StrongTypeRef.GetSyntax(TargetLanguage)
            : Type.CurrentSyntax;

    /// <summary>
    /// Vygeneruje kód parametru.
    /// </summary>
    public string GenerateCode()
    {
        // Určení prefixu podle jazyka a modifikátoru
        string prefix = TargetLanguage switch
        {
            Common.ProgramLanguage.CSharp when IsOut => "out ",
            Common.ProgramLanguage.CSharp when IsRef => "ref ",
            Common.ProgramLanguage.CSharp when IsParams => "params ",
            _ => string.Empty
        };

        var syntax = $"{prefix}{EffectiveTypeSyntax} {Name}";

        if (!string.IsNullOrWhiteSpace(DefaultValue))
        {
            syntax += $" = {DefaultValue}";
        }

        return syntax;
    }

    public override Common.ProgramLanguage TargetLanguage
    {
        get => base.TargetLanguage;
        set
        {
            base.TargetLanguage = value;
            Type.TargetLanguage = value;
            if (_strongTypeRef != null)
                _strongTypeRef.TargetLanguage = value;
        }
    }
}
