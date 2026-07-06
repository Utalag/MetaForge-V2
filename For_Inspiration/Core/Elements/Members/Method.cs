using System.Collections.ObjectModel;
using System.Linq;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Modifiers;
using MetaForge.Core.Elements.Primitives;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Metoda třídy s customizovatelnou logikou.
/// </summary>
public class Method : RootElement, ILanguageElement
{
    private string _name = string.Empty;
    private TypeModel _returnType = new();
    private AccessModifier _accessModifier = AccessModifier.Public;
    private bool _isStatic;
    private bool _isAsync;
    private bool _isAbstract;
    private bool _isVirtual;
    private bool _isOverride;
    private bool _isExtensionMethod;
    private bool _isOperator;
    private string? _operatorSymbol;
    private Comment? _documentation;
    private string _body = string.Empty;

    /// <summary>
    /// Název metody.
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
    /// Návratový typ metody.
    /// </summary>
    public TypeModel ReturnType
    {
        get => _returnType;
        set
        {
            if (_returnType != value)
            {
                _returnType = value;
                _returnType.TargetLanguage = TargetLanguage;
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
    /// Je metoda statická?
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
    /// Je metoda asynchronní?
    /// </summary>
    public bool IsAsync
    {
        get => _isAsync;
        set
        {
            if (_isAsync != value)
            {
                _isAsync = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Je metoda abstraktní?
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
    /// Je metoda virtuální?
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
    /// Je metoda override?
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
    /// Je metoda extension method? (C# this param, první parametr)
    /// </summary>
    public bool IsExtensionMethod
    {
        get => _isExtensionMethod;
        set
        {
            if (_isExtensionMethod != value)
            {
                _isExtensionMethod = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Je metoda operator overload?
    /// </summary>
    public bool IsOperator
    {
        get => _isOperator;
        set
        {
            if (_isOperator != value)
            {
                _isOperator = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Symbol operatoru pro overload (např. "+", "-", "==", "!=").
    /// Používá se když IsOperator = true.
    /// </summary>
    public string? OperatorSymbol
    {
        get => _operatorSymbol;
        set
        {
            if (_operatorSymbol != value)
            {
                _operatorSymbol = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Tělo metody - customizovatelná logika (raw string, fallback).
    /// Priorita: BodyExpressions → Body
    /// </summary>
    public string Body
    {
        get => _body;
        set
        {
            if (_body != value)
            {
                _body = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Deklarativní vyjádření těla metody (jazykově-neutrální).
    /// Má přednost před Body (raw string).
    /// Výrazy se generují v pořadí — typicky: return, assign, apod.
    /// </summary>
    public ObservableCollection<ComputedExpression> BodyExpressions { get; } = new();

    /// <summary>
    /// Vrátí efektivní tělo metody.
    /// Priorita: BodyExpressions → Body
    /// </summary>
    public string ResolvedBody => ResolvedBodyFor(TargetLanguage);

    /// <summary>
    /// Vrátí efektivní tělo metody pro explicitně zadaný jazyk.
    /// Používej pro oddělení běžného exportu od explicitní C# materializace pro AI/Roslyn.
    /// </summary>
    public string ResolvedBodyFor(ProgramLanguage language)
    {
        // 1. ComputedExpressions (jazykově-neutrální)
        if (BodyExpressions.Count > 0)
        {
            var lines = new List<string>();
            foreach (var expr in BodyExpressions)
            {
                expr.TargetLanguage = language;
                lines.Add(expr.RenderCode(language));
            }
            return string.Join("\n        ", lines);
        }

        // 2. Raw string body (fallback)
        return Body;
    }

    /// <summary>
    /// Vstupní parametry metody.
    /// </summary>
    public ObservableCollection<Parameter> Parameters { get; } = new();

    /// <summary>
    /// Property/Fieldy které metoda ovlivňuje (pro testovatelnost).
    /// </summary>
    //public ObservableCollection<string> AffectedMembers { get; } = new();

    public ObservableCollection<Field> AffectedFields { get; } = new();
    public ObservableCollection<Property> AffectedProperties { get; } = new();

    /// <summary>
    /// Kontraktní omezení metody (preconditions, invariants, postconditions).
    /// </summary>
    public ObservableCollection<MethodConstraint> Constraints { get; } = new();

    public Method()
    {
        Parameters.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (Parameter param in e.NewItems)
                param.TargetLanguage = TargetLanguage;
        };

        BodyExpressions.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (ComputedExpression expr in e.NewItems)
                expr.TargetLanguage = TargetLanguage;
        };

        Constraints.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (MethodConstraint c in e.NewItems)
                c.TargetLanguage = TargetLanguage;
        };
    }

    /// <summary>
    /// Dokumentační komentář metody (XML dokumentace).
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
            ReturnType.TargetLanguage = value;
            foreach (var param in Parameters) param.TargetLanguage = value;
            foreach (var expr in BodyExpressions) expr.TargetLanguage = value;
            foreach (var c in Constraints) c.TargetLanguage = value;
            if (_documentation != null)
                _documentation.TargetLanguage = value;
        }
    }

    /// <summary>
    /// Validuje invarianty metody. Nastaví State na Valid nebo Invalid.
    /// </summary>
    public override ValidationSummary Validate()
    {
        ClearValidationResults();

        if (string.IsNullOrWhiteSpace(Name))
            AddError("Method name cannot be empty.", "METHOD_001");

        if (IsAbstract && IsVirtual)
            AddError("Method cannot be both abstract and virtual.", "METHOD_002");

        if (IsAbstract && IsStatic)
            AddError("Method cannot be both abstract and static.", "METHOD_003");

        if (IsAbstract && IsOverride)
            AddError("Method cannot be both abstract and override.", "METHOD_005");

        if (IsVirtual && IsStatic)
            AddError("Method cannot be both virtual and static.", "METHOD_006");

        if (IsAsync && IsAbstract)
            AddError("Method cannot be both async and abstract.", "METHOD_007");

        if (IsAsync && IsVirtual)
            AddError("Method cannot be both async and virtual.", "METHOD_008");

        if (IsOverride && IsStatic)
            AddError("Method cannot be both override and static.", "METHOD_009");

        var parameterNames = Parameters
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var expr in BodyExpressions)
        {
            var expressionResult = expr.ValidateWithContext(parameterNames);
            if (!expressionResult.IsValid)
                AddError($"BodyExpression is invalid: {string.Join(", ", expressionResult.Errors)}", "METHOD_004");
        }

        // Kontrola pořadí Return výrazů - Return musí být poslední (pokud existuje)
        var returnIndex = -1;
        for (int i = 0; i < BodyExpressions.Count; i++)
        {
            if (BodyExpressions[i].Operation == ComputedOperation.Return)
            {
                if (returnIndex >= 0)
                    AddError($"Multiple Return expressions found. Only the last expression can be a Return.", "METHOD_010");
                returnIndex = i;
            }
            else if (returnIndex >= 0)
            {
                AddError($"Return expression must be the last expression in BodyExpressions (found at index {returnIndex}).", "METHOD_011");
                break;
            }
        }

        return FinalizeValidation(Name);
    }
}
