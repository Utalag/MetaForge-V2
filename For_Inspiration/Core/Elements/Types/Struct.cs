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
/// Struct (hodnotový typ).
/// </summary>
public class Struct : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private string _namespace = string.Empty;
    private AccessModifier _accessModifier = AccessModifier.Public;
    private bool _isPartial;
    private bool _isReadOnly; // C# 7.2+

    /// <summary>
    /// Název struct.
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
    /// Namespace struct.
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
    /// Je struct partial?
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
    /// Je struct readonly? (C# 7.2+)
    /// </summary>
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (_isReadOnly != value)
            {
                _isReadOnly = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Rozhraní které struct implementuje.
    /// </summary>
    public ObservableCollection<string> Interfaces { get; } = new();

    /// <summary>
    /// Fieldy struct.
    /// </summary>
    public ObservableCollection<Field> Fields { get; } = new();

    /// <summary>
    /// Properties struct.
    /// </summary>
    public ObservableCollection<Property> Properties { get; } = new();

    /// <summary>
    /// Metody struct.
    /// </summary>
    public ObservableCollection<Method> Methods { get; } = new();

    /// <summary>
    /// Konstruktory struct.
    /// </summary>
    public ObservableCollection<Constructor> Constructors { get; } = new();

    /// <summary>
    /// Using direktivy.
    /// </summary>
    public ObservableCollection<string> Usings { get; } = new();

    /// <summary>
    /// Kreditové skóre struct (pro monetizaci).
    /// </summary>
    public int CreditScore { get; set; } = 15; // Struct má nižší skóre než Class

    public Struct()
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

                if (property.BackingField is { } backingField)
                {
                    backingField.StrongType = property.StrongType;
                    backingField.TargetLanguage = TargetLanguage;

                    if (!Fields.Contains(backingField))
                        Fields.Add(backingField);
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
            foreach (var structField in Fields) structField.TargetLanguage = value;
            foreach (var property in Properties) property.TargetLanguage = value;
            foreach (var method in Methods) method.TargetLanguage = value;
            foreach (var constructor in Constructors) constructor.TargetLanguage = value;
        }
    }

    /// <summary>
    /// Vypočítá celkové kreditové skóre včetně členů.
    /// </summary>
    public int CalculateTotalCreditScore()
    {
        int total = CreditScore;

        // Fields: 1 kredit
        total += Fields.Count;

        // Properties: 2 kredity
        total += Properties.Count * 2;

        // Methods: 3 kredity
        total += Methods.Count * 3;

        // Constructors: 2 kredity
        total += Constructors.Count * 2;

        return total;
    }

    /// <summary>
    /// Validuje invarianty struct kaskádově.
    /// Gate: všichni členové (Fields, Properties, Methods, Constructors) musí mít State == Ready.
    /// </summary>
    public override ValidationSummary Validate()
    {
        ClearValidationResults();

        foreach (var field in Fields)
        {
            if (field.State != MetadataState.Ready)
                AddError($"Field '{field.Name}' is not Ready (State: {field.State}).", "STRUCT_001");
        }

        foreach (var property in Properties)
        {
            if (property.State != MetadataState.Ready)
                AddError($"Property '{property.Name}' is not Ready (State: {property.State}).", "STRUCT_002");
        }

        foreach (var method in Methods)
        {
            if (method.State != MetadataState.Ready)
                AddError($"Method '{method.Name}' is not Ready (State: {method.State}).", "STRUCT_003");
        }

        foreach (var constructor in Constructors)
        {
            if (constructor.State != MetadataState.Ready)
                AddError($"Constructor '{constructor.ClassName}' is not Ready (State: {constructor.State}).", "STRUCT_004");
        }

        if (HasErrors()) return FinalizeValidation(Name);

        if (string.IsNullOrWhiteSpace(Name))
            AddError("Struct name cannot be empty.", "STRUCT_005");

        return FinalizeValidation(Name);
    }

}
