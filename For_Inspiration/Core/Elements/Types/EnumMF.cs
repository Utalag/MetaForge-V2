using System.Collections.ObjectModel;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Modifiers;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Enum (výčtový typ).
/// </summary>
public class EnumMF : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private string _namespace = string.Empty;
    private AccessModifier _accessModifier = AccessModifier.Public;
    private DataType _underlyingType = DataType.Int;

    /// <summary>
    /// Název enum.
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
    /// Namespace enum.
    /// </summary>
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

    /// <summary>
    /// Modifikátor přístupu.
    /// </summary>
    public AccessModifier AccessModifier
    {
        get => _accessModifier;
        set
        {
            if (_accessModifier != value)
            {
                _accessModifier = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Podkladový typ (int, byte, long, atd.).
    /// </summary>
    public DataType UnderlyingType
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

    /// <summary>
    /// Hodnoty enum.
    /// </summary>
    public ObservableCollection<EnumValue> Values { get; } = new();

    /// <summary>
    /// Using direktivy.
    /// </summary>
    public ObservableCollection<string> Usings { get; } = new();
}
/// <summary>
/// Hodnota enum.
/// </summary>
public class EnumValue
{
    /// <summary>
    /// Název hodnoty.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Explicitní číselná hodnota (volitelné).
    /// </summary>
    public int? ExplicitValue { get; set; }

    /// <summary>
    /// Popis hodnoty (pro dokumentaci).
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
