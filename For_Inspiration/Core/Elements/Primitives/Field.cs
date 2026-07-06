using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Modifiers;
using MetaForge.Core.Validation;
using MetaForge.Core.ValueObjects;

namespace MetaForge.Core.Elements.Primitives;

/// <summary>
/// Field (pole) třídy.
/// </summary>
public class Field : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private StrongType _strongType = new() { UnderlyingType = new TypeModel() };
    private AccessModifier _accessModifier = AccessModifier.Private;
    private bool _isStatic;
    private bool _isReadOnly;
    private bool _isConst;
    private string? _defaultValue;

    /// <summary>
    /// Název fieldu.
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
    /// Typ fieldu jako StrongType (pokrývá primitivy, kolekce i Value Objects).
    /// </summary>
    public StrongType StrongType
    {
        get => _strongType;
        set
        {
            if (_strongType != value)
            {
                _strongType = value;
                _strongType.UnderlyingType.TargetLanguage = TargetLanguage;
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
    /// Je field statický?
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
    /// Je field readonly?
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

                // Readonly a const se vylučují
                if (value && _isConst)
                {
                    _isConst = false;
                    OnPropertyChanged(nameof(IsConst));
                }
            }
        }
    }

    /// <summary>
    /// Je field konstanta?
    /// </summary>
    public bool IsConst
    {
        get => _isConst;
        set
        {
            if (_isConst != value)
            {
                _isConst = value;
                OnPropertyChanged();

                // Readonly a const se vylučují
                if (value && _isReadOnly)
                {
                    _isReadOnly = false;
                    OnPropertyChanged(nameof(IsReadOnly));
                }
            }
        }
    }

    /// <summary>
    /// Výchozí hodnota fieldu.
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

    public override Common.ProgramLanguage TargetLanguage
    {
        get => base.TargetLanguage;
        set
        {
            base.TargetLanguage = value;
            StrongType.UnderlyingType.TargetLanguage = value;
        }
    }

    /// <summary>
    /// Validuje invarianty fieldu. Nastaví State na Valid nebo Invalid.
    /// </summary>
    public override ValidationSummary Validate()
    {
        ClearValidationResults();

        if (string.IsNullOrWhiteSpace(Name))
            AddError("Field name cannot be empty.", "FIELD_001");

        if (IsConst && IsReadOnly)
            AddError("Field cannot be both const and readonly.", "FIELD_002");

        return FinalizeValidation(Name);
    }


    private string GetAccessModifierSyntax()
    {
        return TargetLanguage switch
        {
            Common.ProgramLanguage.CSharp => AccessModifier switch
            {
                AccessModifier.Public => "public",
                AccessModifier.Private => "private",
                AccessModifier.Protected => "protected",
                AccessModifier.Internal => "internal",
                AccessModifier.ProtectedInternal => "protected internal",
                AccessModifier.PrivateProtected => "private protected",
                _ => "private"
            },
            Common.ProgramLanguage.TypeScript => AccessModifier switch
            {
                AccessModifier.Public => "public",
                AccessModifier.Private => "private",
                AccessModifier.Protected => "protected",
                _ => "private"
            },
            Common.ProgramLanguage.Java => AccessModifier switch
            {
                AccessModifier.Public => "public",
                AccessModifier.Private => "private",
                AccessModifier.Protected => "protected",
                _ => "private"
            },
            Common.ProgramLanguage.Python => "", // Python používá konvenci _name pro private
            Common.ProgramLanguage.Go => Name[0] >= 'A' && Name[0] <= 'Z' ? "" : "", // Go používá velikost prvního písmene
            _ => "private"
        };
    }
}
