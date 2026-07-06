using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Jazykově-neutrální deklarativní výraz pro computed logiku property a metod.
/// Nahrazuje raw string body za strukturovaný, validovatelný a multi-language model.
/// 
/// <example>
/// // Setter s validací minimálního věku:
/// var expr = new ComputedExpression
/// {
///     Operation = ComputedOperation.ThrowIfOutOfRange,
///     LeftOperand = "value",
///     MinValue = "18",
///     ParameterName = "Age"
/// };
/// // C# → if (value &lt; 18) throw new ArgumentOutOfRangeException("Age", ...);
/// // Python → if value &lt; 18: raise ValueError(...)
/// </example>
/// </summary>
public class ComputedExpression : RootElement, ILanguageElement
{
    private ComputedOperation _operation = ComputedOperation.Return;
    private string _leftOperand = string.Empty;
    private string? _rightOperand;
    private string? _minValue;
    private string? _maxValue;
    private string? _parameterName;
    private string? _message;
    private string? _formatTemplate;
    private string? _rawCode;
    private TypeModel? _declaredType;
    private string? _constructedTypeName;
    private ComparisonOperator _comparisonOperator = ComparisonOperator.Equal;
    private ComputedExpression? _condition;
    private ComputedExpression? _thenBranch;
    private ComputedExpression? _elseBranch;
    private List<string> _arguments = new();

    /// <summary>
    /// Typ operace (Return, Assign, ThrowIfNull, ThrowIfOutOfRange, ...).
    /// </summary>
    public ComputedOperation Operation
    {
        get => _operation;
        set
        {
            if (_operation != value)
            {
                _operation = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Levý operand (název proměnné, fieldu, property).
    /// Příklad: "value", "_backingField", "this.Name"
    /// </summary>
    public string LeftOperand
    {
        get => _leftOperand;
        set
        {
            if (_leftOperand != value)
            {
                _leftOperand = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Pravý operand (literál, název proměnné, výraz).
    /// Příklad: "18", "defaultName", "string.Empty"
    /// </summary>
    public string? RightOperand
    {
        get => _rightOperand;
        set
        {
            if (_rightOperand != value)
            {
                _rightOperand = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Minimální hodnota pro ThrowIfOutOfRange / Clamp.
    /// </summary>
    public string? MinValue
    {
        get => _minValue;
        set
        {
            if (_minValue != value)
            {
                _minValue = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Maximální hodnota pro ThrowIfOutOfRange / Clamp.
    /// </summary>
    public string? MaxValue
    {
        get => _maxValue;
        set
        {
            if (_maxValue != value)
            {
                _maxValue = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Název parametru pro chybové hlášky (ThrowIfNull, ThrowIfOutOfRange).
    /// </summary>
    public string? ParameterName
    {
        get => _parameterName;
        set
        {
            if (_parameterName != value)
            {
                _parameterName = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Vlastní chybová zpráva pro Throw operace.
    /// </summary>
    public string? Message
    {
        get => _message;
        set
        {
            if (_message != value)
            {
                _message = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Šablona pro StringFormat operaci.
    /// Příklad: "{FirstName} {LastName}"
    /// </summary>
    public string? FormatTemplate
    {
        get => _formatTemplate;
        set
        {
            if (_formatTemplate != value)
            {
                _formatTemplate = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Raw kód pro Operation == Raw. Jazykově specifický fallback.
    /// </summary>
    public string? RawCode
    {
        get => _rawCode;
        set
        {
            if (_rawCode != value)
            {
                _rawCode = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Sémantický datový typ deklarované lokální proměnné pro Operation == DeclareVariable.
    /// </summary>
    public TypeModel? DeclaredType
    {
        get => _declaredType;
        set
        {
            if (_declaredType != value)
            {
                _declaredType = value;
                if (_declaredType != null)
                    _declaredType.TargetLanguage = TargetLanguage;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Název konstruovaného custom typu pro Operation == ConstructInstance.
    /// </summary>
    public string? ConstructedTypeName
    {
        get => _constructedTypeName;
        set
        {
            if (_constructedTypeName != value)
            {
                _constructedTypeName = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Argumenty konstrukce pro Operation == ConstructInstance.
    /// </summary>
    public List<string> Arguments
    {
        get => _arguments;
        set => _arguments = value ?? [];
    }

    /// <summary>
    /// Operátor porovnání pro Operation == Comparison.
    /// </summary>
    public ComparisonOperator ComparisonOperator
    {
        get => _comparisonOperator;
        set
        {
            if (_comparisonOperator != value)
            {
                _comparisonOperator = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Podmínka pro Operation == Conditional.
    /// </summary>
    public ComputedExpression? Condition
    {
        get => _condition;
        set
        {
            if (_condition != value)
            {
                _condition = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Then větev pro Operation == Conditional.
    /// </summary>
    public ComputedExpression? ThenBranch
    {
        get => _thenBranch;
        set
        {
            if (_thenBranch != value)
            {
                _thenBranch = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Else větev pro Operation == Conditional.
    /// </summary>
    public ComputedExpression? ElseBranch
    {
        get => _elseBranch;
        set
        {
            if (_elseBranch != value)
            {
                _elseBranch = value;
                OnPropertyChanged();
            }
        }
    }

    // IfChain properties - pro Operation == IfChain
    private List<string> _conditions = new();
    private List<string> _branchCodes = new();
    private List<List<ComputedExpression>> _branchExpressions = new();
    private string? _elseCode;
    private List<ComputedExpression> _elseExpressions = new();
    private bool _hasElseBranch;

    /// <summary>
    /// Seznam podmínek pro if/else-if řetězec.
    /// První položka je pro initial if, další pro else-if.
    /// </summary>
    public List<string> Conditions
    {
        get => _conditions;
        set => _conditions = value;
    }

    /// <summary>
    /// Seznam kódů pro then-větve odpovídající podmínkám.
    /// </summary>
    public List<string> BranchCodes
    {
        get => _branchCodes;
        set => _branchCodes = value;
    }

    /// <summary>
    /// Sémantické výrazy jednotlivých větví if/else-if řetězce.
    /// Má přednost před BranchCodes pokud je vyplněno.
    /// </summary>
    public List<List<ComputedExpression>> BranchExpressions
    {
        get => _branchExpressions;
        set
        {
            _branchExpressions = value ?? [];
            foreach (var branch in _branchExpressions)
                foreach (var expr in branch)
                    expr.TargetLanguage = TargetLanguage;
        }
    }

    /// <summary>
    /// Kód pro else větev (pokud existuje).
    /// </summary>
    public string? ElseCode
    {
        get => _elseCode;
        set
        {
            if (_elseCode != value)
            {
                _elseCode = value;
                _hasElseBranch = value != null;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Sémantické výrazy else větve.
    /// Má přednost před ElseCode pokud je vyplněno.
    /// </summary>
    public List<ComputedExpression> ElseExpressions
    {
        get => _elseExpressions;
        set
        {
            _elseExpressions = value ?? [];
            foreach (var expr in _elseExpressions)
                expr.TargetLanguage = TargetLanguage;
        }
    }

    /// <summary>
    /// Příznak, zda existuje else větev.
    /// </summary>
    public bool HasElseBranch
    {
        get => _hasElseBranch;
        set => _hasElseBranch = value;
    }

    public override ProgramLanguage TargetLanguage
    {
        get => base.TargetLanguage;
        set
        {
            base.TargetLanguage = value;

            if (DeclaredType != null)
                DeclaredType.TargetLanguage = value;

            if (Condition != null)
                Condition.TargetLanguage = value;

            if (ThenBranch != null)
                ThenBranch.TargetLanguage = value;

            if (ElseBranch != null)
                ElseBranch.TargetLanguage = value;

            foreach (var branch in BranchExpressions)
                foreach (var expr in branch)
                    expr.TargetLanguage = value;

            foreach (var expr in ElseExpressions)
                expr.TargetLanguage = value;
        }
    }

    /// <summary>
    /// Validuje ComputedExpression podle typu operace.
    /// </summary>
    public override ValidationSummary Validate()
    {
        return ValidateWithContext(null);
    }

    /// <summary>
    /// Validuje ComputedExpression podle typu operace a volitelného kontextu dostupných parametrů.
    /// Kontext se používá pouze u operací, které očekávají vstupní argument (např. ThrowIfNull).
    /// </summary>
    public ValidationSummary ValidateWithContext(ISet<string>? availableParameterNames)
    {
        ClearValidationResults();

        switch (Operation)
        {
            case ComputedOperation.Return:
                if (string.IsNullOrWhiteSpace(LeftOperand))
                    AddError("Return expression requires LeftOperand (expression to return).", "COMP_001");
                break;

            case ComputedOperation.ConstructInstance:
                if (string.IsNullOrWhiteSpace(ConstructedTypeName))
                    AddError("ConstructInstance requires ConstructedTypeName.", "COMP_027");
                break;

            case ComputedOperation.ReturnFormatted:
                if (string.IsNullOrWhiteSpace(FormatTemplate))
                    AddError("ReturnFormatted requires FormatTemplate.", "COMP_028");
                break;

            case ComputedOperation.Assign:
                if (string.IsNullOrWhiteSpace(LeftOperand))
                    AddError("Assign requires LeftOperand (target).", "COMP_002");
                if (string.IsNullOrWhiteSpace(RightOperand))
                    AddError("Assign requires RightOperand (value).", "COMP_003");
                break;

            case ComputedOperation.DeclareVariable:
                if (string.IsNullOrWhiteSpace(LeftOperand))
                    AddError("DeclareVariable requires LeftOperand (variable name).", "COMP_023");
                if (DeclaredType == null)
                    AddError("DeclareVariable requires DeclaredType.", "COMP_024");
                break;

            case ComputedOperation.ThrowIfNull:
                if (string.IsNullOrWhiteSpace(LeftOperand))
                    AddError("ThrowIfNull requires LeftOperand (value to check).", "COMP_004");
                break;

            case ComputedOperation.ThrowIfOutOfRange:
                if (string.IsNullOrWhiteSpace(LeftOperand))
                    AddError("ThrowIfOutOfRange requires LeftOperand (value to check).", "COMP_005");
                if (string.IsNullOrWhiteSpace(MinValue) && string.IsNullOrWhiteSpace(MaxValue))
                    AddError("ThrowIfOutOfRange requires at least MinValue or MaxValue.", "COMP_006");
                break;

            case ComputedOperation.ThrowIfEmpty:
                if (string.IsNullOrWhiteSpace(LeftOperand))
                    AddError("ThrowIfEmpty requires LeftOperand (string to check).", "COMP_007");
                break;

            case ComputedOperation.Clamp:
                if (string.IsNullOrWhiteSpace(LeftOperand))
                    AddError("Clamp requires LeftOperand (value to clamp).", "COMP_008");
                if (string.IsNullOrWhiteSpace(MinValue) || string.IsNullOrWhiteSpace(MaxValue))
                    AddError("Clamp requires both MinValue and MaxValue.", "COMP_009");
                break;

            case ComputedOperation.Comparison:
                if (string.IsNullOrWhiteSpace(LeftOperand))
                    AddError("Comparison requires LeftOperand.", "COMP_010");
                if (string.IsNullOrWhiteSpace(RightOperand))
                    AddError("Comparison requires RightOperand.", "COMP_011");
                break;

            case ComputedOperation.Conditional:
                if (Condition == null)
                    AddError("Conditional requires Condition.", "COMP_012");
                if (ThenBranch == null)
                    AddError("Conditional requires ThenBranch.", "COMP_013");
                break;

            case ComputedOperation.MemberAccess:
                if (string.IsNullOrWhiteSpace(LeftOperand))
                    AddError("MemberAccess requires LeftOperand (target).", "COMP_014");
                if (string.IsNullOrWhiteSpace(RightOperand))
                    AddError("MemberAccess requires RightOperand (member name).", "COMP_015");
                break;

            case ComputedOperation.StringFormat:
                if (string.IsNullOrWhiteSpace(FormatTemplate))
                    AddError("StringFormat requires FormatTemplate.", "COMP_016");
                break;

            case ComputedOperation.Raw:
                if (string.IsNullOrWhiteSpace(RawCode))
                    AddError("Raw requires RawCode.", "COMP_017");
                break;

            case ComputedOperation.IfChain:
                if (Conditions.Count == 0)
                    AddError("IfChain requires at least one condition in Conditions.", "COMP_020");
                var branchCount = BranchExpressions.Count > 0 ? BranchExpressions.Count : BranchCodes.Count;
                if (branchCount == 0)
                    AddError("IfChain requires at least one branch body.", "COMP_021");
                if (Conditions.Count != branchCount)
                    AddError($"IfChain requires matching Conditions ({Conditions.Count}) and branch bodies ({branchCount}).", "COMP_022");
                break;
        }

        if (availableParameterNames is { Count: > 0 })
        {
            ValidateParameterReferenceIfApplicable(availableParameterNames);
        }

        ValidateNestedExpressions(availableParameterNames);

        return FinalizeValidation("ComputedExpression");
    }

    private void ValidateNestedExpressions(ISet<string>? availableParameterNames)
    {
        ValidateNestedExpression(Condition, "Condition", availableParameterNames);
        ValidateNestedExpression(ThenBranch, "ThenBranch", availableParameterNames);
        ValidateNestedExpression(ElseBranch, "ElseBranch", availableParameterNames);

        for (int i = 0; i < BranchExpressions.Count; i++)
        {
            foreach (var expr in BranchExpressions[i])
            {
                var nestedResult = expr.ValidateWithContext(availableParameterNames);
                if (!nestedResult.IsValid)
                    AddError($"BranchExpressions[{i}] is invalid: {string.Join(", ", nestedResult.Errors)}", "COMP_025");
            }
        }

        for (int i = 0; i < ElseExpressions.Count; i++)
        {
            var nestedResult = ElseExpressions[i].ValidateWithContext(availableParameterNames);
            if (!nestedResult.IsValid)
                AddError($"ElseExpressions[{i}] is invalid: {string.Join(", ", nestedResult.Errors)}", "COMP_026");
        }
    }

    private void ValidateNestedExpression(ComputedExpression? nestedExpression, string branchName, ISet<string>? availableParameterNames)
    {
        if (nestedExpression is null)
        {
            return;
        }

        var nestedResult = nestedExpression.ValidateWithContext(availableParameterNames);
        if (!nestedResult.IsValid)
        {
            AddError($"{branchName} is invalid: {string.Join(", ", nestedResult.Errors)}", "COMP_019");
        }
    }

    private void ValidateParameterReferenceIfApplicable(ISet<string> availableParameterNames)
    {
        if (Operation is not (ComputedOperation.ThrowIfNull
            or ComputedOperation.ThrowIfEmpty
            or ComputedOperation.ThrowIfOutOfRange
            or ComputedOperation.Clamp))
        {
            return;
        }

        var identifier = TryExtractIdentifier(LeftOperand);
        if (identifier is null)
        {
            return;
        }

        if (!availableParameterNames.Contains(identifier))
        {
            AddError($"Operand '{identifier}' does not match any parameter in current context.", "COMP_018");
        }
    }

    private static string? TryExtractIdentifier(string? operand)
    {
        if (string.IsNullOrWhiteSpace(operand))
        {
            return null;
        }

        var trimmed = operand.Trim();
        if (trimmed.StartsWith("_"))
        {
            return null;
        }

        if (trimmed.Contains(' ') || trimmed.Contains('(') || trimmed.Contains(')') || trimmed.Contains('"') || trimmed.Contains('\''))
        {
            return null;
        }

        var separatorIndex = trimmed.IndexOfAny(['.', '?', '[', ']', '+', '-', '*', '/', '%', '&', '|', '!', '=', '<', '>', ':', ',']);
        var root = separatorIndex >= 0 ? trimmed[..separatorIndex] : trimmed;
        if (string.IsNullOrWhiteSpace(root))
        {
            return null;
        }

        if (root is "this" or "base" or "null" or "true" or "false")
        {
            return null;
        }

        if (char.IsDigit(root[0]))
        {
            return null;
        }

        return root;
    }

    /// <summary>
    /// Vygeneruje kód pro aktuální TargetLanguage.
    /// Deleguje na ExpressionRendererRegistry pokud je renderer registrován.
    /// </summary>
    public string GenerateCode()
    {
        var renderer = ExpressionRendererRegistry.Get(TargetLanguage);
        if (renderer != null)
            return renderer.Render(this);

        return RenderCodeFallback(TargetLanguage);
    }

    /// <summary>
    /// Vyrenderuje výraz pro explicitně zadaný jazyk.
    /// Deleguje na ExpressionRendererRegistry pokud je renderer registrován.
    /// </summary>
    public string RenderCode(ProgramLanguage language)
    {
        var renderer = ExpressionRendererRegistry.Get(language);
        if (renderer != null)
            return renderer.Render(this);

        return RenderCodeFallback(language);
    }

    /// <summary>
    /// Fallback rendering pokud ExpressionRendererRegistry není naplněn.
    /// </summary>
    private string RenderCodeFallback(ProgramLanguage language)
    {
        return language switch
        {
            ProgramLanguage.CSharp => GenerateCSharp(),
            ProgramLanguage.TypeScript => GenerateTypeScript(),
            ProgramLanguage.Python => GeneratePython(),
            ProgramLanguage.Java => GenerateJava(),
            ProgramLanguage.Go => GenerateGo(),
            _ => throw new NotSupportedException($"Rendering for language '{language}' is not supported.")
        };
    }

    /// <summary>
    /// Explicitní C# render pro AI/Roslyn cestu.
    /// </summary>
    public string RenderCSharpCode() => RenderCode(ProgramLanguage.CSharp);

    /// <summary>
    /// Generuje C# kód pro danou operaci.
    /// </summary>
    private string GenerateCSharp()
    {
        return Operation switch
        {
            ComputedOperation.Return => $"return {RenderSemanticSnippet(LeftOperand, ProgramLanguage.CSharp)};",

            ComputedOperation.ConstructInstance => GenerateConstructedInstance(ProgramLanguage.CSharp),

            ComputedOperation.ReturnFormatted => GenerateFormattedReturn(ProgramLanguage.CSharp),

            ComputedOperation.Assign => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.CSharp)} = {RenderSemanticSnippet(RightOperand, ProgramLanguage.CSharp)};",

            ComputedOperation.DeclareVariable => GenerateVariableDeclaration(ProgramLanguage.CSharp),

            ComputedOperation.ThrowIfNull =>
                $"if ({RenderSemanticSnippet(LeftOperand, ProgramLanguage.CSharp)} is null) throw new ArgumentNullException(nameof({ParameterName ?? LeftOperand}){(Message != null ? $", \"{Message}\"" : "")});",

            ComputedOperation.ThrowIfOutOfRange => GenerateCSharpThrowIfOutOfRange(),

            ComputedOperation.ThrowIfEmpty =>
                $"if (string.IsNullOrWhiteSpace({RenderSemanticSnippet(LeftOperand, ProgramLanguage.CSharp)})) throw new ArgumentException(\"{Message ?? $"{ParameterName ?? LeftOperand} cannot be empty."}\", nameof({ParameterName ?? LeftOperand}));",

            ComputedOperation.Clamp => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.CSharp)} = Math.Clamp({RenderSemanticSnippet(LeftOperand, ProgramLanguage.CSharp)}, {RenderSemanticSnippet(MinValue, ProgramLanguage.CSharp)}, {RenderSemanticSnippet(MaxValue, ProgramLanguage.CSharp)});",

            ComputedOperation.Comparison =>
                $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.CSharp)} {GetComparisonOperator()} {RenderSemanticSnippet(RightOperand, ProgramLanguage.CSharp)}",

            ComputedOperation.Conditional => GenerateCSharpConditional(),

            ComputedOperation.MemberAccess => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.CSharp)}.{RightOperand}",

            ComputedOperation.StringFormat =>
                $"$\"{FormatTemplate}\"",

            ComputedOperation.IfChain => GenerateCSharpIfChain(),

            ComputedOperation.Raw => RawCode ?? string.Empty,

            _ => $"/* Unknown operation: {Operation} */"
        };
    }

    private string GenerateTypeScript()
    {
        return Operation switch
        {
            ComputedOperation.Return => $"return {RenderSemanticSnippet(LeftOperand, ProgramLanguage.TypeScript)};",
            ComputedOperation.ConstructInstance => GenerateConstructedInstance(ProgramLanguage.TypeScript),
            ComputedOperation.ReturnFormatted => GenerateFormattedReturn(ProgramLanguage.TypeScript),
            ComputedOperation.Assign => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.TypeScript)} = {RenderSemanticSnippet(RightOperand, ProgramLanguage.TypeScript)};",
            ComputedOperation.DeclareVariable => GenerateVariableDeclaration(ProgramLanguage.TypeScript),
            ComputedOperation.ThrowIfNull => $"if ({RenderSemanticSnippet(LeftOperand, ProgramLanguage.TypeScript)} == null) throw new TypeError({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} cannot be null.")});",
            ComputedOperation.ThrowIfOutOfRange => GenerateTypeScriptThrowIfOutOfRange(),
            ComputedOperation.ThrowIfEmpty => $"if (!{RenderSemanticSnippet(LeftOperand, ProgramLanguage.TypeScript)}?.trim()) throw new Error({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} cannot be empty.")});",
            ComputedOperation.Clamp => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.TypeScript)} = Math.max({RenderSemanticSnippet(MinValue, ProgramLanguage.TypeScript)}, Math.min({RenderSemanticSnippet(MaxValue, ProgramLanguage.TypeScript)}, {RenderSemanticSnippet(LeftOperand, ProgramLanguage.TypeScript)}));",
            ComputedOperation.Comparison => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.TypeScript)} {GetComparisonOperator()} {RenderSemanticSnippet(RightOperand, ProgramLanguage.TypeScript)}",
            ComputedOperation.Conditional => GenerateBraceConditional(ProgramLanguage.TypeScript),
            ComputedOperation.MemberAccess => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.TypeScript)}.{RightOperand}",
            ComputedOperation.StringFormat => $"`{ConvertTemplateToInterpolation("${", "}")}`",
            ComputedOperation.IfChain => GenerateBraceIfChain(ProgramLanguage.TypeScript),
            ComputedOperation.Raw => RawCode ?? string.Empty,
            _ => throw new NotSupportedException($"Operation '{Operation}' is not supported for TypeScript rendering.")
        };
    }

    private string GeneratePython()
    {
        return Operation switch
        {
            ComputedOperation.Return => $"return {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Python)}",
            ComputedOperation.ConstructInstance => GenerateConstructedInstance(ProgramLanguage.Python),
            ComputedOperation.ReturnFormatted => GenerateFormattedReturn(ProgramLanguage.Python),
            ComputedOperation.Assign => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Python)} = {RenderSemanticSnippet(RightOperand, ProgramLanguage.Python)}",
            ComputedOperation.DeclareVariable => GenerateVariableDeclaration(ProgramLanguage.Python),
            ComputedOperation.ThrowIfNull => $"if {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Python)} is None:\n    raise ValueError({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} cannot be null.")})",
            ComputedOperation.ThrowIfOutOfRange => GeneratePythonThrowIfOutOfRange(),
            ComputedOperation.ThrowIfEmpty => $"if not {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Python)} or not {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Python)}.strip():\n    raise ValueError({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} cannot be empty.")})",
            ComputedOperation.Clamp => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Python)} = max({RenderSemanticSnippet(MinValue, ProgramLanguage.Python)}, min({RenderSemanticSnippet(MaxValue, ProgramLanguage.Python)}, {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Python)}))",
            ComputedOperation.Comparison => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Python)} {GetComparisonOperator()} {RenderSemanticSnippet(RightOperand, ProgramLanguage.Python)}",
            ComputedOperation.Conditional => GeneratePythonConditional(),
            ComputedOperation.MemberAccess => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Python)}.{RightOperand}",
            ComputedOperation.StringFormat => $"f\"{ConvertTemplateToInterpolation("{", "}")}\"",
            ComputedOperation.IfChain => GeneratePythonIfChain(),
            ComputedOperation.Raw => RawCode ?? string.Empty,
            _ => throw new NotSupportedException($"Operation '{Operation}' is not supported for Python rendering.")
        };
    }

    private string GenerateJava()
    {
        return Operation switch
        {
            ComputedOperation.Return => $"return {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Java)};",
            ComputedOperation.ConstructInstance => GenerateConstructedInstance(ProgramLanguage.Java),
            ComputedOperation.ReturnFormatted => GenerateFormattedReturn(ProgramLanguage.Java),
            ComputedOperation.Assign => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Java)} = {RenderSemanticSnippet(RightOperand, ProgramLanguage.Java)};",
            ComputedOperation.DeclareVariable => GenerateVariableDeclaration(ProgramLanguage.Java),
            ComputedOperation.ThrowIfNull => $"if ({RenderSemanticSnippet(LeftOperand, ProgramLanguage.Java)} == null) throw new IllegalArgumentException({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} cannot be null.")});",
            ComputedOperation.ThrowIfOutOfRange => GenerateJavaThrowIfOutOfRange(),
            ComputedOperation.ThrowIfEmpty => $"if ({RenderSemanticSnippet(LeftOperand, ProgramLanguage.Java)} == null || {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Java)}.isBlank()) throw new IllegalArgumentException({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} cannot be empty.")});",
            ComputedOperation.Clamp => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Java)} = Math.max({RenderSemanticSnippet(MinValue, ProgramLanguage.Java)}, Math.min({RenderSemanticSnippet(MaxValue, ProgramLanguage.Java)}, {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Java)}));",
            ComputedOperation.Comparison => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Java)} {GetComparisonOperator()} {RenderSemanticSnippet(RightOperand, ProgramLanguage.Java)}",
            ComputedOperation.Conditional => GenerateBraceConditional(ProgramLanguage.Java),
            ComputedOperation.MemberAccess => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Java)}.{RightOperand}",
            ComputedOperation.StringFormat => BuildQuotedMessage(FormatTemplate ?? string.Empty),
            ComputedOperation.IfChain => GenerateBraceIfChain(ProgramLanguage.Java),
            ComputedOperation.Raw => RawCode ?? string.Empty,
            _ => throw new NotSupportedException($"Operation '{Operation}' is not supported for Java rendering.")
        };
    }

    private string GenerateGo()
    {
        return Operation switch
        {
            ComputedOperation.Return => $"return {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Go)}",
            ComputedOperation.ConstructInstance => GenerateConstructedInstance(ProgramLanguage.Go),
            ComputedOperation.ReturnFormatted => GenerateFormattedReturn(ProgramLanguage.Go),
            ComputedOperation.Assign => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Go)} = {RenderSemanticSnippet(RightOperand, ProgramLanguage.Go)}",
            ComputedOperation.DeclareVariable => GenerateVariableDeclaration(ProgramLanguage.Go),
            ComputedOperation.ThrowIfNull => $"if {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Go)} == nil {{\n    panic({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} cannot be nil.")})\n}}",
            ComputedOperation.ThrowIfOutOfRange => GenerateGoThrowIfOutOfRange(),
            ComputedOperation.ThrowIfEmpty => $"if strings.TrimSpace({RenderSemanticSnippet(LeftOperand, ProgramLanguage.Go)}) == \"\" {{\n    panic({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} cannot be empty.")})\n}}",
            ComputedOperation.Clamp => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Go)} = max({RenderSemanticSnippet(MinValue, ProgramLanguage.Go)}, min({RenderSemanticSnippet(MaxValue, ProgramLanguage.Go)}, {RenderSemanticSnippet(LeftOperand, ProgramLanguage.Go)}))",
            ComputedOperation.Comparison => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Go)} {GetComparisonOperator()} {RenderSemanticSnippet(RightOperand, ProgramLanguage.Go)}",
            ComputedOperation.Conditional => GenerateBraceConditional(ProgramLanguage.Go),
            ComputedOperation.MemberAccess => $"{RenderSemanticSnippet(LeftOperand, ProgramLanguage.Go)}.{RightOperand}",
            ComputedOperation.StringFormat => BuildQuotedMessage(FormatTemplate ?? string.Empty),
            ComputedOperation.IfChain => GenerateBraceIfChain(ProgramLanguage.Go),
            ComputedOperation.Raw => RawCode ?? string.Empty,
            _ => throw new NotSupportedException($"Operation '{Operation}' is not supported for Go rendering.")
        };
    }

    /// <summary>
    /// Generuje C# kód pro komplexní if/else-if/else řetězec.
    /// </summary>
    private string GenerateCSharpIfChain()
    {
        var sb = new StringBuilder();

        var branchBlocks = GetRenderedBranchBlocks(ProgramLanguage.CSharp);
        var elseBlock = GetRenderedElseBlock(ProgramLanguage.CSharp);

        if (Conditions.Count == 0)
        {
            return "// Empty IfChain - no conditions defined";
        }

        // Initial if
        if (Conditions.Count > 0 && branchBlocks.Count > 0)
        {
            sb.AppendLine($"if ({RenderSemanticSnippet(Conditions[0], ProgramLanguage.CSharp)})");
            sb.AppendLine("{");
            AppendIndentedBlock(sb, branchBlocks[0], 1);
            sb.AppendLine("}");
        }

        // Else-if branches
        for (int i = 1; i < Conditions.Count && i < branchBlocks.Count; i++)
        {
            sb.AppendLine($"else if ({RenderSemanticSnippet(Conditions[i], ProgramLanguage.CSharp)})");
            sb.AppendLine("{");
            AppendIndentedBlock(sb, branchBlocks[i], 1);
            sb.AppendLine("}");
        }

        // Else branch
        if (HasElseBranch && elseBlock != null)
        {
            sb.AppendLine("else");
            sb.AppendLine("{");
            AppendIndentedBlock(sb, elseBlock, 1);
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    private string GenerateCSharpThrowIfOutOfRange()
    {
        var paramName = ParameterName ?? LeftOperand;

        var operand = RenderSemanticSnippet(LeftOperand, ProgramLanguage.CSharp);
        var minValue = RenderSemanticSnippet(MinValue, ProgramLanguage.CSharp);
        var maxValue = RenderSemanticSnippet(MaxValue, ProgramLanguage.CSharp);

        if (MinValue != null && MaxValue != null)
        {
            var msg = Message ?? $"{paramName} must be between {MinValue} and {MaxValue}.";
            return $"if ({operand} < {minValue} || {operand} > {maxValue}) throw new ArgumentOutOfRangeException(nameof({paramName}), {operand}, \"{msg}\");";
        }

        if (MinValue != null)
        {
            var msg = Message ?? $"{paramName} must be at least {MinValue}.";
            return $"if ({operand} < {minValue}) throw new ArgumentOutOfRangeException(nameof({paramName}), {operand}, \"{msg}\");";
        }

        // MaxValue only
        var maxMsg = Message ?? $"{paramName} must be at most {MaxValue}.";
        return $"if ({operand} > {maxValue}) throw new ArgumentOutOfRangeException(nameof({paramName}), {operand}, \"{maxMsg}\");";
    }

    private string GenerateCSharpConditional()
    {
        var condCode = Condition?.RenderCode(ProgramLanguage.CSharp) ?? "true";
        var thenCode = ThenBranch?.RenderCode(ProgramLanguage.CSharp) ?? "{ }";
        var elseCode = ElseBranch != null ? $"\nelse\n{{\n    {ElseBranch.RenderCode(ProgramLanguage.CSharp)}\n}}" : "";

        return $"if ({condCode})\n{{\n    {thenCode}\n}}{elseCode}";
    }

    private string GenerateBraceConditional(ProgramLanguage language)
    {
        var condCode = Condition?.RenderCode(language) ?? "true";
        var thenCode = ThenBranch?.RenderCode(language) ?? string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine($"if ({condCode})");
        sb.AppendLine("{");
        AppendIndentedBlock(sb, thenCode, 1);
        sb.Append("}");

        if (ElseBranch != null)
        {
            sb.AppendLine();
            sb.AppendLine("else");
            sb.AppendLine("{");
            AppendIndentedBlock(sb, ElseBranch!.RenderCode(language), 1);
            sb.Append("}");
        }

        return sb.ToString();
    }

    private string GeneratePythonConditional()
    {
        var condCode = Condition?.RenderCode(ProgramLanguage.Python) ?? "True";
        var thenCode = IndentBlock(ThenBranch?.RenderCode(ProgramLanguage.Python) ?? "pass", 1);
        var elseCode = ElseBranch != null ? $"\nelse:\n{IndentBlock(ElseBranch.RenderCode(ProgramLanguage.Python), 1)}" : string.Empty;
        return $"if {condCode}:\n{thenCode}{elseCode}";
    }

    private string GenerateBraceIfChain(ProgramLanguage language)
    {
        var branchBlocks = GetRenderedBranchBlocks(language);
        var elseBlock = GetRenderedElseBlock(language);

        if (Conditions.Count == 0)
            return "";

        var sb = new StringBuilder();

        if (branchBlocks.Count > 0)
        {
            sb.AppendLine($"if ({RenderSemanticSnippet(Conditions[0], language)})");
            sb.AppendLine("{");
            AppendIndentedBlock(sb, branchBlocks[0], 1);
            sb.AppendLine("}");
        }

        for (int i = 1; i < Conditions.Count && i < branchBlocks.Count; i++)
        {
            sb.AppendLine($"else if ({RenderSemanticSnippet(Conditions[i], language)})");
            sb.AppendLine("{");
            AppendIndentedBlock(sb, branchBlocks[i], 1);
            sb.AppendLine("}");
        }

        if (HasElseBranch && elseBlock != null)
        {
            sb.AppendLine("else");
            sb.AppendLine("{");
            AppendIndentedBlock(sb, elseBlock, 1);
            sb.AppendLine("}");
        }

        return sb.ToString().TrimEnd();
    }

    private string GeneratePythonIfChain()
    {
        var branchBlocks = GetRenderedBranchBlocks(ProgramLanguage.Python);
        var elseBlock = GetRenderedElseBlock(ProgramLanguage.Python);

        if (Conditions.Count == 0)
            return "";

        var sb = new StringBuilder();

        if (branchBlocks.Count > 0)
        {
            sb.AppendLine($"if {RenderSemanticSnippet(Conditions[0], ProgramLanguage.Python)}:");
            sb.AppendLine(IndentBlock(branchBlocks[0], 1));
        }

        for (int i = 1; i < Conditions.Count && i < branchBlocks.Count; i++)
        {
            sb.AppendLine($"elif {RenderSemanticSnippet(Conditions[i], ProgramLanguage.Python)}:");
            sb.AppendLine(IndentBlock(branchBlocks[i], 1));
        }

        if (HasElseBranch && elseBlock != null)
        {
            sb.AppendLine("else:");
            sb.AppendLine(IndentBlock(elseBlock, 1));
        }

        return sb.ToString().TrimEnd();
    }

    private string GenerateVariableDeclaration(ProgramLanguage language)
    {
        if (DeclaredType == null)
            return string.Empty;

        var typeSyntax = DeclaredType.GetSyntax(language);
        var initializerExpression = RightOperand != null ? RenderSemanticSnippet(RightOperand, language) : null;
        var initializer = initializerExpression != null ? $" = {initializerExpression}" : string.Empty;

        return language switch
        {
            ProgramLanguage.CSharp => $"{typeSyntax} {LeftOperand}{initializer};",
            ProgramLanguage.TypeScript => $"let {LeftOperand}: {typeSyntax}{initializer};",
            ProgramLanguage.Python => initializerExpression != null ? $"{LeftOperand}: {typeSyntax} = {initializerExpression}" : $"{LeftOperand}: {typeSyntax}",
            ProgramLanguage.Java => $"{typeSyntax} {LeftOperand}{initializer};",
            ProgramLanguage.Go => initializerExpression != null ? $"var {LeftOperand} {typeSyntax} = {initializerExpression}" : $"var {LeftOperand} {typeSyntax}",
            _ => throw new NotSupportedException($"Variable declaration is not supported for language '{language}'.")
        };
    }

    private string GenerateConstructedInstance(ProgramLanguage language)
    {
        var args = string.Join(", ", Arguments.Select(arg => RenderSemanticSnippet(arg, language)));

        return language switch
        {
            ProgramLanguage.CSharp => $"return new {ConstructedTypeName}({args});",
            ProgramLanguage.TypeScript => $"return new {ConstructedTypeName}({args});",
            ProgramLanguage.Python => $"return {ConstructedTypeName}({args})",
            ProgramLanguage.Java => $"return new {ConstructedTypeName}({args});",
            ProgramLanguage.Go => $"return {ConstructedTypeName}({args})",
            _ => throw new NotSupportedException($"ConstructInstance is not supported for language '{language}'.")
        };
    }

    private string GenerateFormattedReturn(ProgramLanguage language)
    {
        var template = FormatTemplate ?? string.Empty;

        return language switch
        {
            ProgramLanguage.CSharp => $"return $\"{template}\";",
            ProgramLanguage.TypeScript => $"return `{ConvertTemplateToInterpolation("${", "}")}`;",
            ProgramLanguage.Python => $"return f\"{ConvertTemplateToInterpolation("{", "}")}\"",
            ProgramLanguage.Java => $"return {BuildQuotedMessage(template)};",
            ProgramLanguage.Go => $"return {BuildQuotedMessage(template)}",
            _ => throw new NotSupportedException($"ReturnFormatted is not supported for language '{language}'.")
        };
    }

    private string GenerateTypeScriptThrowIfOutOfRange()
    {
        var operand = RenderSemanticSnippet(LeftOperand, ProgramLanguage.TypeScript);
        var minValue = RenderSemanticSnippet(MinValue, ProgramLanguage.TypeScript);
        var maxValue = RenderSemanticSnippet(MaxValue, ProgramLanguage.TypeScript);
        if (MinValue != null && MaxValue != null)
            return $"if ({operand} < {minValue} || {operand} > {maxValue}) throw new RangeError({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be between {MinValue} and {MaxValue}.")});";
        if (MinValue != null)
            return $"if ({operand} < {minValue}) throw new RangeError({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be at least {MinValue}.")});";
        return $"if ({operand} > {maxValue}) throw new RangeError({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be at most {MaxValue}.")});";
    }

    private string GeneratePythonThrowIfOutOfRange()
    {
        var operand = RenderSemanticSnippet(LeftOperand, ProgramLanguage.Python);
        var minValue = RenderSemanticSnippet(MinValue, ProgramLanguage.Python);
        var maxValue = RenderSemanticSnippet(MaxValue, ProgramLanguage.Python);
        if (MinValue != null && MaxValue != null)
            return $"if {operand} < {minValue} or {operand} > {maxValue}:\n    raise ValueError({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be between {MinValue} and {MaxValue}.")})";
        if (MinValue != null)
            return $"if {operand} < {minValue}:\n    raise ValueError({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be at least {MinValue}.")})";
        return $"if {operand} > {maxValue}:\n    raise ValueError({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be at most {MaxValue}.")})";
    }

    private string GenerateJavaThrowIfOutOfRange()
    {
        var operand = RenderSemanticSnippet(LeftOperand, ProgramLanguage.Java);
        var minValue = RenderSemanticSnippet(MinValue, ProgramLanguage.Java);
        var maxValue = RenderSemanticSnippet(MaxValue, ProgramLanguage.Java);
        if (MinValue != null && MaxValue != null)
            return $"if ({operand} < {minValue} || {operand} > {maxValue}) throw new IllegalArgumentException({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be between {MinValue} and {MaxValue}.")});";
        if (MinValue != null)
            return $"if ({operand} < {minValue}) throw new IllegalArgumentException({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be at least {MinValue}.")});";
        return $"if ({operand} > {maxValue}) throw new IllegalArgumentException({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be at most {MaxValue}.")});";
    }

    private string GenerateGoThrowIfOutOfRange()
    {
        var operand = RenderSemanticSnippet(LeftOperand, ProgramLanguage.Go);
        var minValue = RenderSemanticSnippet(MinValue, ProgramLanguage.Go);
        var maxValue = RenderSemanticSnippet(MaxValue, ProgramLanguage.Go);
        if (MinValue != null && MaxValue != null)
            return $"if {operand} < {minValue} || {operand} > {maxValue} {{\n    panic({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be between {MinValue} and {MaxValue}.")})\n}}";
        if (MinValue != null)
            return $"if {operand} < {minValue} {{\n    panic({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be at least {MinValue}.")})\n}}";
        return $"if {operand} > {maxValue} {{\n    panic({BuildQuotedMessage(Message ?? $"{ParameterName ?? LeftOperand} must be at most {MaxValue}.")})\n}}";
    }

    private static string RenderSemanticSnippet(string? expression, ProgramLanguage language)
        => SemanticStandardLibrary.RenderInExpression(MathSemanticRenderer.RenderInExpression(expression, language), language);

    private string GetComparisonOperator()
    {
        return ComparisonOperator switch
        {
            ComparisonOperator.Equal => "==",
            ComparisonOperator.NotEqual => "!=",
            ComparisonOperator.LessThan => "<",
            ComparisonOperator.LessThanOrEqual => "<=",
            ComparisonOperator.GreaterThan => ">",
            ComparisonOperator.GreaterThanOrEqual => ">=",
            _ => "=="
        };
    }

    private IReadOnlyList<string> GetRenderedBranchBlocks(ProgramLanguage language)
    {
        if (BranchExpressions.Count > 0)
            return BranchExpressions.Select(branch => RenderExpressionSequence(branch, language)).ToList();

        return BranchCodes;
    }

    private string? GetRenderedElseBlock(ProgramLanguage language)
    {
        if (ElseExpressions.Count > 0)
            return RenderExpressionSequence(ElseExpressions, language);

        return ElseCode;
    }

    private static string RenderExpressionSequence(IEnumerable<ComputedExpression> expressions, ProgramLanguage language)
        => string.Join("\n", expressions.Select(expr => expr.RenderCode(language)));

    private static void AppendIndentedBlock(StringBuilder sb, string block, int indentLevel)
    {
        foreach (var line in block.Replace("\r", string.Empty).Split('\n'))
        {
            if (line.Length == 0)
            {
                sb.AppendLine();
                continue;
            }

            sb.AppendLine($"{new string(' ', indentLevel * 4)}{line}");
        }
    }

    private static string IndentBlock(string block, int indentLevel)
    {
        var lines = block.Replace("\r", string.Empty).Split('\n');
        var prefix = new string(' ', indentLevel * 4);
        return string.Join("\n", lines.Select(line => string.IsNullOrEmpty(line) ? line : prefix + line));
    }

    private static string BuildQuotedMessage(string message)
        => $"\"{message.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";

    private string ConvertTemplateToInterpolation(string openToken, string closeToken)
    {
        if (string.IsNullOrEmpty(FormatTemplate))
            return string.Empty;

        return System.Text.RegularExpressions.Regex.Replace(
            FormatTemplate,
            @"\{([^{}]+)\}",
            match => $"{openToken}{match.Groups[1].Value}{closeToken}");
    }

    public ComputedExpression DeepClone()
    {
        return new ComputedExpression
        {
            Operation = Operation,
            LeftOperand = LeftOperand,
            RightOperand = RightOperand,
            MinValue = MinValue,
            MaxValue = MaxValue,
            ParameterName = ParameterName,
            Message = Message,
            FormatTemplate = FormatTemplate,
            RawCode = RawCode,
            DeclaredType = DeclaredType == null ? null : CloneTypeModel(DeclaredType),
            ConstructedTypeName = ConstructedTypeName,
            Arguments = [.. Arguments],
            ComparisonOperator = ComparisonOperator,
            Condition = Condition?.DeepClone(),
            ThenBranch = ThenBranch?.DeepClone(),
            ElseBranch = ElseBranch?.DeepClone(),
            Conditions = [.. Conditions],
            BranchCodes = [.. BranchCodes],
            BranchExpressions = BranchExpressions.Select(branch => branch.Select(expr => expr.DeepClone()).ToList()).ToList(),
            ElseCode = ElseCode,
            ElseExpressions = ElseExpressions.Select(expr => expr.DeepClone()).ToList(),
            HasElseBranch = HasElseBranch,
            TargetLanguage = TargetLanguage,
            IsCustomized = IsCustomized,
            State = State
        };
    }

    private static TypeModel CloneTypeModel(TypeModel source)
    {
        return new TypeModel
        {
            BaseType = source.BaseType,
            CustomTypeName = source.CustomTypeName,
            IsNullable = source.IsNullable,
            IsCollection = source.IsCollection,
            SemanticCollection = source.SemanticCollection,
            KeyType = source.KeyType,
            KeyCustomTypeName = source.KeyCustomTypeName,
            EntityKind = source.EntityKind,
            TargetLanguage = source.TargetLanguage,
            IsCustomized = source.IsCustomized,
            State = source.State
        };
    }


}
