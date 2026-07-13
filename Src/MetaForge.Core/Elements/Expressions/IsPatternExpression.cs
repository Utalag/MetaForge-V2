using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Pattern matching výraz — <c>x is string</c>, <c>x is not null</c>.
/// </summary>
public sealed class IsPatternExpression : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.IsPattern;

    /// <summary>Výraz, který se testuje.</summary>
    public Expression Operand { get; init; } = null!;

    /// <summary>Druh patternu (Type, Null, NotNull, Constant).</summary>
    public PatternKind PatternKind { get; init; }

    /// <summary>Název typu pro Type pattern (např. "string").</summary>
    public string? TargetTypeName { get; init; }

    /// <summary>Je pattern negovaný? (<c>x is not null</c>).</summary>
    public bool IsNegated { get; init; }

    public IsPatternExpression(Expression operand, PatternKind patternKind, string? targetTypeName = null, bool isNegated = false)
    {
        Operand = operand;
        PatternKind = patternKind;
        TargetTypeName = targetTypeName;
        IsNegated = isNegated;
        ResultType = TypeModel.Bool; // is-pattern vždy vrací bool
    }
}

/// <summary>
/// Druhy patternů podporovaných v IsPatternExpression.
/// </summary>
public enum PatternKind
{
    /// <summary>Type pattern: <c>x is string</c>.</summary>
    Type,

    /// <summary>Null pattern: <c>x is null</c>.</summary>
    Null,

    /// <summary>Constant pattern: <c>x is 42</c>.</summary>
    Constant,
}
