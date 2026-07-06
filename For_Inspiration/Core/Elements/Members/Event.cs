using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Modifiers;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Event (událost).
/// </summary>
public class Event : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private TypeModel _eventHandlerType;
    private AccessModifier _accessModifier = AccessModifier.Public;
    private bool _isStatic;
    private bool _isVirtual;
    private bool _isAbstract;
    private bool _isOverride;

    public Event()
    {
        _eventHandlerType = new TypeModel
        {
            BaseType = DataType.Custom,
            CustomTypeName = "EventHandler"
        };
    }

    /// <summary>
    /// Název eventu.
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
    /// Typ event handleru (např. EventHandler, EventHandler&lt;T&gt;).
    /// </summary>
    public TypeModel EventHandlerType
    {
        get => _eventHandlerType;
        set
        {
            if (_eventHandlerType != value)
            {
                _eventHandlerType = value;
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
    /// Je event statický?
    /// </summary>
    public bool IsStatic
    {
        get => _isStatic;
        set
        {
            if (_isStatic != value)
            {
                _isStatic = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Je event virtuální?
    /// </summary>
    public bool IsVirtual
    {
        get => _isVirtual;
        set
        {
            if (_isVirtual != value)
            {
                _isVirtual = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Je event abstraktní?
    /// </summary>
    public bool IsAbstract
    {
        get => _isAbstract;
        set
        {
            if (_isAbstract != value)
            {
                _isAbstract = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Přepisuje event z bázové třídy?
    /// </summary>
    public bool IsOverride
    {
        get => _isOverride;
        set
        {
            if (_isOverride != value)
            {
                _isOverride = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Popis eventu.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Vygeneruje kód eventu.
    /// </summary>
    public string GenerateCode()
    {
        var parts = new List<string>();

        // Access modifier
        parts.Add(AccessModifier.ToString().ToLower());

        // Static
        if (IsStatic)
        {
            parts.Add("static");
        }

        // Virtual/Abstract/Override
        if (IsAbstract)
        {
            parts.Add("abstract");
        }
        else if (IsOverride)
        {
            parts.Add("override");
        }
        else if (IsVirtual)
        {
            parts.Add("virtual");
        }

        // Event keyword
        parts.Add("event");

        // Type
        parts.Add(EventHandlerType.CurrentSyntax);

        // Name
        parts.Add(Name);

        return string.Join(" ", parts) + ";";
    }
}
