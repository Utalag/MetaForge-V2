using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Primitives;

/// <summary>
/// Variable (lokální proměnná).
/// </summary>
public class Variable : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private TypeModel _type;
    private string _defaultValue = string.Empty;
    private bool _isConst;
    private bool _isReadOnly;

    public Variable()
    {
        _type = new TypeModel { BaseType = DataType.Object };
    }

    /// <summary>
    /// Název proměnné.
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
    /// Datový typ proměnné.
    /// </summary>
    public TypeModel Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Výchozí hodnota proměnné.
    /// </summary>
    public string DefaultValue
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
    /// Je proměnná konstanta?
    /// </summary>
    public bool IsConst
    {
        get => _isConst;
        set
        {
            if (_isConst != value)
            {
                _isConst = value;
                if (_isConst)
                {
                    IsReadOnly = false; // Const a readonly se vylučují
                }
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Je proměnná readonly? (pouze pro C#)
    /// </summary>
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (_isReadOnly != value)
            {
                _isReadOnly = value;
                if (_isReadOnly)
                {
                    IsConst = false; // Const a readonly se vylučují
                }
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Popis proměnné.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Vygeneruje kód proměnné.
    /// </summary>
    public string GenerateCode()
    {
        var parts = new List<string>();

        // Const/readonly
        if (IsConst)
        {
            parts.Add("const");
        }
        else if (IsReadOnly)
        {
            parts.Add("readonly");
        }

        // Type
        parts.Add(Type.CurrentSyntax);

        // Name
        parts.Add(Name);

        // Default value
        if (!string.IsNullOrWhiteSpace(DefaultValue))
        {
            parts.Add("=");
            parts.Add(DefaultValue);
        }

        return string.Join(" ", parts) + ";";
    }
}
