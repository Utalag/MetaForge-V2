using System.Collections.ObjectModel;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Modifiers;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Interface (rozhraní).
/// </summary>
public class Interface : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private string _namespace = string.Empty;
    private AccessModifier _accessModifier = AccessModifier.Public;
    private bool _isPartial;

    /// <summary>
    /// Název interface.
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
    /// Namespace interface.
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
    /// Je interface partial?
    /// </summary>
    public bool IsPartial
    {
        get => _isPartial;
        set
        {
            if (_isPartial != value)
            {
                _isPartial = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Rozhraní které tento interface rozšiřuje.
    /// </summary>
    public ObservableCollection<string> BaseInterfaces { get; } = new();

    /// <summary>
    /// Metody interface (bez implementace).
    /// </summary>
    public ObservableCollection<Method> Methods { get; } = new();

    /// <summary>
    /// Using direktivy.
    /// </summary>
    public ObservableCollection<string> Usings { get; } = new();
}
