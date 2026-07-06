using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Modifiers;
using MetaForge.Core.Elements.Primitives;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Konstruktor třídy.
/// </summary>
public class Constructor : RootElement, ILanguageElement
{
    private string _className = string.Empty;
    private AccessModifier _accessModifier = AccessModifier.Public;
    private bool _isStatic;
    private string _body = string.Empty;

    /// <summary>
    /// Název třídy (konstruktor má stejný název jako třída).
    /// </summary>
    public string ClassName
    {
        get => _className;
        set
        {
            if (_className != value)
            {
                _className = value;
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
    /// Je konstruktor statický?
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
    /// Tělo konstruktoru.
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
    /// Deklarativní vyjádření těla konstruktoru (jazykově-neutrální).
    /// Má přednost před Body (raw string).
    /// </summary>
    public ObservableCollection<ComputedExpression> BodyExpressions { get; } = new();

    /// <summary>
    /// Vrátí efektivní tělo konstruktoru.
    /// Priorita: BodyExpressions → Body
    /// </summary>
    public string ResolvedBody
    {
        get
        {
            if (BodyExpressions.Count > 0)
            {
                var lines = new List<string>();
                foreach (var expr in BodyExpressions)
                {
                    expr.TargetLanguage = TargetLanguage;
                    lines.Add(expr.GenerateCode());
                }

                return string.Join("\n", lines);
            }

            return Body;
        }
    }

    /// <summary>
    /// Parametry konstruktoru.
    /// </summary>
    public ObservableCollection<Parameter> Parameters { get; } = new();

    /// <summary>
    /// Base/this volání (pro chain konstruktorů).
    /// </summary>
    public string? BaseCall { get; set; }

    public Constructor()
    {
        Parameters.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (Parameter parameter in e.NewItems)
                parameter.TargetLanguage = TargetLanguage;
        };

        BodyExpressions.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (ComputedExpression expression in e.NewItems)
                expression.TargetLanguage = TargetLanguage;
        };
    }

    public override Common.ProgramLanguage TargetLanguage
    {
        get => base.TargetLanguage;
        set
        {
            base.TargetLanguage = value;
            foreach (var parameter in Parameters) parameter.TargetLanguage = value;
            foreach (var expression in BodyExpressions) expression.TargetLanguage = value;
        }
    }

    public override ValidationSummary Validate()
    {
        ClearValidationResults();

        if (string.IsNullOrWhiteSpace(ClassName))
            AddError("Constructor class name cannot be empty.", "CTOR_001");

        if (IsStatic && Parameters.Count > 0)
            AddError("Static constructor cannot have parameters.", "CTOR_002");

        if (IsStatic && !string.IsNullOrWhiteSpace(BaseCall))
            AddError("Static constructor cannot use base/this call.", "CTOR_003");

        var parameterNames = Parameters
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var expr in BodyExpressions)
        {
            var expressionResult = expr.ValidateWithContext(parameterNames);
            if (!expressionResult.IsValid)
                AddError($"BodyExpression is invalid: {string.Join(", ", expressionResult.Errors)}", "CTOR_004");
        }

        return FinalizeValidation(ClassName);
    }

    /// <summary>
    /// Vygeneruje kód konstruktoru.
    /// </summary>
    public string GenerateCode()
    {
        var sb = new StringBuilder();

        // Access modifier
        sb.Append(GetAccessModifierSyntax());
        sb.Append(" ");

        // Static
        if (IsStatic)
        {
            sb.Append("static ");
        }

        // Name
        sb.Append(ClassName);

        // Parameters
        sb.Append("(");
        var paramStrings = Parameters.Select(p => p.GenerateCode());
        sb.Append(string.Join(", ", paramStrings));
        sb.Append(")");

        // Base/this call
        if (!string.IsNullOrWhiteSpace(BaseCall))
        {
            sb.Append($" : {BaseCall}");
        }

        // Body
        sb.AppendLine();
        sb.AppendLine("{");

        if (!string.IsNullOrWhiteSpace(ResolvedBody))
        {
            var bodyLines = ResolvedBody.Split('\n');
            foreach (var line in bodyLines)
            {
                sb.Append("    ");
                sb.AppendLine(line.TrimEnd());
            }
        }

        sb.Append("}");

        return sb.ToString();
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
                _ => "public"
            },
            _ => "public"
        };
    }
}
