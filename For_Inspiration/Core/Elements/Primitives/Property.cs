using System.Collections.ObjectModel;
using System.Collections.Generic;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Modifiers;
using MetaForge.Core.Validation;
using MetaForge.Core.ValueObjects;

namespace MetaForge.Core.Elements.Primitives;

/// <summary>
/// Property (vlastnost) třídy.
/// </summary>
public class Property : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private StrongType _strongType = new() { UnderlyingType = new TypeModel() };
    private AccessModifier _accessModifier = AccessModifier.Public;
    private bool _isStatic;
    private bool _isVirtual;
    private bool _isAbstract;
    private bool _isOverride;
    private bool _hasGetter = true;
    private bool _hasSetter = true;
    private string? _defaultValue;
    private string? _getterBody;
    private string? _setterBody;
    private Field? _backingField;
    private Comment? _documentation;
    private ComputedExpression? _getterExpression;

    /// <summary>
    /// Název property.
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
    /// Typ property jako StrongType (pokrývá primitivy, kolekce i Value Objects).
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
    /// Je property statická?
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
    /// Je property virtuální?
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
    /// Je property abstraktní?
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
    /// Je property override?
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
    /// Má property getter?
    /// </summary>
    public bool HasGetter
    {
        get => _hasGetter;
        set
        {
            if (_hasGetter != value)
            {
                _hasGetter = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Má property setter?
    /// </summary>
    public bool HasSetter
    {
        get => _hasSetter;
        set
        {
            if (_hasSetter != value)
            {
                _hasSetter = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Výchozí hodnota property.
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
    /// Vlastní tělo getteru. Pokud je nastaveno, generuje se rozvinutá syntaxe místo auto-property.
    /// Příklad: "return _name.Trim();"
    /// </summary>
    public string? GetterBody
    {
        get => _getterBody;
        set
        {
            if (_getterBody != value)
            {
                _getterBody = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Vlastní tělo setteru. Pokud je nastaveno, generuje se rozvinutá syntaxe místo auto-property.
    /// Příklad: "if (value != null) _name = value.Trim();"
    /// </summary>
    public string? SetterBody
    {
        get => _setterBody;
        set
        {
            if (_setterBody != value)
            {
                _setterBody = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Deklarativní computed výraz pro getter (jazykově-neutrální).
    /// Má přednost před GetterBody (raw string).
    /// </summary>
    public ComputedExpression? GetterExpression
    {
        get => _getterExpression;
        set
        {
            if (_getterExpression != value)
            {
                _getterExpression = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Deklarativní computed výrazy pro setter (jazykově-neutrální).
    /// Mají přednost před SetterBody (raw string).
    /// Výrazy se generují v pořadí — typicky: validace, pak přiřazení.
    /// </summary>
    public ObservableCollection<ComputedExpression> SetterExpressions { get; } = new();

    /// <summary>
    /// Backing field tohoto property.
    /// Pokud je nastaven a GetterBody/SetterBody jsou null,
    /// těla se automaticky odvodí: getter = "return {field};", setter = "{field} = value;"
    /// </summary>
    public Field? BackingField
    {
        get => _backingField;
        set
        {
            if (_backingField != value)
            {
                _backingField = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Vrátí efektivní tělo getteru.
    /// Priorita: GetterExpression → GetterBody → BackingField → null (auto-property).
    /// </summary>
    public string? ResolvedGetterBody
    {
        get
        {
            // 1. ComputedExpression (jazykově-neutrální)
            if (GetterExpression != null)
            {
                GetterExpression.TargetLanguage = TargetLanguage;
                return GetterExpression.GenerateCode();
            }

            // 2. Raw string body (fallback)
            if (GetterBody != null)
                return GetterBody;

            // 3. BackingField
            if (BackingField != null && HasGetter)
                return $"return {BackingField.Name};";

            return null;
        }
    }

    /// <summary>
    /// Vrátí efektivní tělo setteru.
    /// Priorita: SetterExpressions → SetterBody → BackingField → null (auto-property).
    /// </summary>
    public string? ResolvedSetterBody
    {
        get
        {
            // 1. ComputedExpressions (jazykově-neutrální)
            if (SetterExpressions.Count > 0)
            {
                var lines = new List<string>();
                foreach (var expr in SetterExpressions)
                {
                    expr.TargetLanguage = TargetLanguage;
                    lines.Add(expr.GenerateCode());
                }
                return string.Join("\n        ", lines);
            }

            // 2. Raw string body (fallback)
            if (SetterBody != null)
                return SetterBody;

            // 3. BackingField
            if (BackingField != null && HasSetter)
                return $"{BackingField.Name} = value;";

            return null;
        }
    }

    /// <summary>
    /// True pokud property má computed logiku (GetterExpression nebo SetterExpressions).
    /// </summary>
    public bool IsComputed => GetterExpression != null || SetterExpressions.Count > 0;

    /// <summary>
    /// Dokumentační komentář property (XML dokumentace).
    /// </summary>
    public Comment? Documentation
    {
        get => _documentation;
        set
        {
            if (_documentation != value)
            {
                _documentation = value;
                if (_documentation != null)
                    _documentation.TargetLanguage = TargetLanguage;
                OnPropertyChanged();
            }
        }
    }

    public override ProgramLanguage TargetLanguage
    {
        get => base.TargetLanguage;
        set
        {
            base.TargetLanguage = value;
            StrongType.UnderlyingType.TargetLanguage = value;
            if (_getterExpression != null)
                _getterExpression.TargetLanguage = value;
            foreach (var expr in SetterExpressions)
                expr.TargetLanguage = value;
            if (_documentation != null)
                _documentation.TargetLanguage = value;
        }
    }

    /// <summary>
    /// Validuje invarianty property. Nastaví State na Valid nebo Invalid.
    /// BackingField musí mít State == Ready.
    /// </summary>
    public override ValidationSummary Validate()
    {
        ClearValidationResults();

        if (string.IsNullOrWhiteSpace(Name))
            AddError("Property name cannot be empty.", "PROP_001");

        if (!HasGetter && !HasSetter)
            AddError("Property must have at least a getter or setter.", "PROP_002");

        if (IsAbstract && IsVirtual)
            AddError("Property cannot be both abstract and virtual.", "PROP_003");

        if (BackingField != null && BackingField.State != MetadataState.Ready)
            AddError($"BackingField '{BackingField.Name}' is not Ready (State: {BackingField.State}).", "PROP_004");

        // Validace ComputedExpressions
        if (GetterExpression != null)
        {
            var getterResult = GetterExpression.ValidateWithContext(null);
            if (!getterResult.IsValid)
                AddError($"GetterExpression is invalid: {string.Join(", ", getterResult.Errors)}", "PROP_005");
        }

        var setterParameterContext = new HashSet<string>(StringComparer.Ordinal)
        {
            "value"
        };

        foreach (var expr in SetterExpressions)
        {
            var exprResult = expr.ValidateWithContext(setterParameterContext);
            if (!exprResult.IsValid)
                AddError($"SetterExpression is invalid: {string.Join(", ", exprResult.Errors)}", "PROP_006");
        }

        return FinalizeValidation(Name);
    }


    private string GetAccessModifierSyntax()
    {
        return TargetLanguage switch
        {
            ProgramLanguage.CSharp => AccessModifier switch
            {
                AccessModifier.Public => "public",
                AccessModifier.Private => "private",
                AccessModifier.Protected => "protected",
                AccessModifier.Internal => "internal",
                _ => "public"
            },
            ProgramLanguage.TypeScript => AccessModifier switch
            {
                AccessModifier.Public => "public",
                AccessModifier.Private => "private",
                AccessModifier.Protected => "protected",
                _ => "public"
            },
            _ => "public"
        };
    }
}
