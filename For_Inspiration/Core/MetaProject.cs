using System.Collections.ObjectModel;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Validation;

namespace MetaForge.Core;

/// <summary>
/// Kořenový kontejner celého projektu — obsahuje všechny třídy a enumy.
/// </summary>
public class MetaProject : RootElement
{
    private string _name = string.Empty;
    private string? _description;
    private string? _icon;
    private int _version = 1;

    /// <summary>
    /// Název projektu.
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
    /// Volitelný popis projektu.
    /// </summary>
    public string? Description
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
    /// Volitelná ikona projektu (emoji).
    /// </summary>
    public string? Icon
    {
        get => _icon;
        set
        {
            if (_icon != value)
            {
                _icon = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Verze projektu (inkrementuje se při změnách).
    /// </summary>
    public int Version
    {
        get => _version;
        set
        {
            if (_version != value)
            {
                _version = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Kolekce tříd v projektu.
    /// </summary>
    public ObservableCollection<Class> Classes { get; } = new();

    /// <summary>
    /// Kolekce enumů v projektu.
    /// </summary>
    public ObservableCollection<EnumMF> Enums { get; } = new();

    public MetaProject()
    {
        Classes.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (Class cls in e.NewItems)
                cls.TargetLanguage = TargetLanguage;
        };
        Enums.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (EnumMF enm in e.NewItems)
                enm.TargetLanguage = TargetLanguage;
        };
    }

    public override ProgramLanguage TargetLanguage
    {
        get => base.TargetLanguage;
        set
        {
            base.TargetLanguage = value;
            foreach (var cls in Classes) cls.TargetLanguage = value;
            foreach (var enm in Enums) enm.TargetLanguage = value;
        }
    }

    public override ValidationSummary Validate()
    {
        ClearValidationResults();

        if (string.IsNullOrWhiteSpace(Name))
            AddError("Project name cannot be empty.", "PROJECT_001");

        if (Version < 1)
            AddError("Version must be at least 1.", "PROJECT_002");

        // Gate: všechny třídy a enumy musí být Valid nebo Ready
        foreach (var cls in Classes)
        {
            if (cls.State is MetadataState.Draft or MetadataState.Invalid)
                AddError($"Class '{cls.Name}' is not valid (State: {cls.State}).", "PROJECT_003");
        }

        foreach (var enm in Enums)
        {
            if (enm.State is MetadataState.Draft or MetadataState.Invalid)
                AddError($"Enum '{enm.Name}' is not valid (State: {enm.State}).", "PROJECT_004");
        }

        return FinalizeValidation(Name);
    }
}
