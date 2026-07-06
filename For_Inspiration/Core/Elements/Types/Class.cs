using System.Collections.ObjectModel;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Modifiers;
using MetaForge.Core.Elements.Primitives;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Třída (Class) - centrální prvek metamodelu.
/// </summary>
public class Class : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private string _namespace = string.Empty;
    private AccessModifier _accessModifier = AccessModifier.Public;
    private bool _isStatic;
    private bool _isAbstract;
    private bool _isSealed;
    private bool _isPartial;
    private string? _baseClass;
    private int _creditScore;

    /// <summary>
    /// Název třídy.
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
    /// Namespace třídy.
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
    /// Je třída statická?
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

                // Static třída nemůže být abstract nebo sealed
                if (value)
                {
                    if (_isAbstract)
                    {
                        _isAbstract = false;
                        OnPropertyChanged(nameof(IsAbstract));
                    }
                    if (_isSealed)
                    {
                        _isSealed = false;
                        OnPropertyChanged(nameof(IsSealed));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Je třída abstraktní?
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

                // Abstract a sealed se vylučují
                if (value)
                {
                    if (_isSealed)
                    {
                        _isSealed = false;
                        OnPropertyChanged(nameof(IsSealed));
                    }
                    if (_isStatic)
                    {
                        _isStatic = false;
                        OnPropertyChanged(nameof(IsStatic));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Je třída sealed?
    /// </summary>
    public bool IsSealed
    {
        get => _isSealed;
        set
        {
            if (_isSealed != value)
            {
                _isSealed = value;
                OnPropertyChanged();

                // Abstract a sealed se vylučují
                if (value)
                {
                    if (_isAbstract)
                    {
                        _isAbstract = false;
                        OnPropertyChanged(nameof(IsAbstract));
                    }
                    if (_isStatic)
                    {
                        _isStatic = false;
                        OnPropertyChanged(nameof(IsStatic));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Je třída partial? (pro bezpečné rozšiřování stávajícího kódu)
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
    /// Bázová třída (pokud dědí).
    /// </summary>
    public string? BaseClass
    {
        get => _baseClass;
        set
        {
            if (_baseClass != value)
            {
                _baseClass = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Kreditové skóre pro monetizaci.
    /// Každá třída má své skóre podle složitosti.
    /// </summary>
    public int CreditScore
    {
        get => _creditScore;
        set
        {
            if (_creditScore != value)
            {
                _creditScore = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Rozhraní které třída implementuje.
    /// </summary>
    public ObservableCollection<string> Interfaces { get; } = new();

    /// <summary>
    /// Fieldy třídy.
    /// </summary>
    public ObservableCollection<Field> Fields { get; } = new();

    /// <summary>
    /// Properties třídy.
    /// </summary>
    public ObservableCollection<Property> Properties { get; } = new();

    /// <summary>
    /// Metody třídy.
    /// </summary>
    public ObservableCollection<Method> Methods { get; } = new();

    /// <summary>
    /// Konstruktory třídy.
    /// </summary>
    public ObservableCollection<Constructor> Constructors { get; } = new();

    /// <summary>
    /// Using direktivy.
    /// </summary>
    public ObservableCollection<string> Usings { get; } = new();

    public Class()
    {
        Fields.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (Field field in e.NewItems)
                field.TargetLanguage = TargetLanguage;
        };
        Properties.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (Property property in e.NewItems)
            {
                property.TargetLanguage = TargetLanguage;

                // Auto-registrace BackingField: synchronizace typu a přidání do Fields
                if (property.BackingField is { } bf)
                {
                    bf.StrongType = property.StrongType;
                    bf.TargetLanguage = TargetLanguage;

                    if (!Fields.Contains(bf))
                        Fields.Add(bf);
                }
            }
        };
        Methods.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (Method method in e.NewItems)
                method.TargetLanguage = TargetLanguage;
        };
        Constructors.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (Constructor constructor in e.NewItems)
                constructor.TargetLanguage = TargetLanguage;
        };
    }

    public override ProgramLanguage TargetLanguage
    {
        get => base.TargetLanguage;
        set
        {
            base.TargetLanguage = value;
            foreach (var f in Fields) f.TargetLanguage = value;
            foreach (var property in Properties) property.TargetLanguage = value;
            foreach (var method in Methods) method.TargetLanguage = value;
            foreach (var constructor in Constructors) constructor.TargetLanguage = value;
        }
    }

    /// <summary>
    /// Vypočítá celkové kreditové skóre včetně všech členů.
    /// </summary>
    public int CalculateTotalCreditScore()
    {
        var total = CreditScore;

        // Přidej kredity za fieldy (1 kredit za field)
        total += Fields.Count;

        // Přidej kredity za properties (2 kredity za property)
        total += Properties.Count * 2;

        // Přidej kredity za metody (5 kreditů za metodu)
        total += Methods.Count * 5;

        // Přidej kredity za konstruktory (3 kredity za konstruktor)
        total += Constructors.Count * 3;

        return total;
    }

    /// <summary>
    /// Validuje invarianty třídy kaskádově.
    /// Gate: všichni členové (Fields, Properties, Methods, Constructors) musí mít State == Ready.
    /// Teprve poté se validují invarianty třídy samotné.
    /// </summary>
    public override ValidationSummary Validate()
    {
        ClearValidationResults();

        // Gate: children musí být Ready (prošli Validate() + MarkReady() z test pipeline)
        foreach (var field in Fields)
        {
            if (field.State != MetadataState.Ready)
                AddError($"Field '{field.Name}' is not Ready (State: {field.State}).", "CLASS_001");
        }

        foreach (var property in Properties)
        {
            if (property.State != MetadataState.Ready)
                AddError($"Property '{property.Name}' is not Ready (State: {property.State}).", "CLASS_002");
        }

        foreach (var method in Methods)
        {
            if (method.State != MetadataState.Ready)
                AddError($"Method '{method.Name}' is not Ready (State: {method.State}).", "CLASS_003");
        }

        foreach (var constructor in Constructors)
        {
            if (constructor.State != MetadataState.Ready)
                AddError($"Constructor '{constructor.ClassName}' is not Ready (State: {constructor.State}).", "CLASS_007");
        }

        // Pokud má některý člen chybu, nezacházíme dál
        if (HasErrors()) return FinalizeValidation(Name);

        // Invarianty třídy
        if (string.IsNullOrWhiteSpace(Name))
            AddError("Class name cannot be empty.", "CLASS_004");

        if (IsAbstract && IsSealed)
            AddError("Class cannot be both abstract and sealed.", "CLASS_005");

        if (IsStatic && (IsAbstract || IsSealed))
            AddError("Static class cannot be abstract or sealed.", "CLASS_006");

        return FinalizeValidation(Name);
    }

}
