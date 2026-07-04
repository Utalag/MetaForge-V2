using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Podmíněný (ternární) výraz — reprezentuje a ? b : c.
/// </summary>
public sealed class ConditionalExpression : Expression
{
    public override string Kind => "Conditional";
    public override ExpressionKind ExpressionKind => ExpressionKind.Conditional;

    /// <summary>Podmínka (musí být bool).</summary>
    public Expression Condition { get; init; }

    /// <summary>Výraz pro true větev.</summary>
    public Expression WhenTrue { get; init; }

    /// <summary>Výraz pro false větev.</summary>
    public Expression WhenFalse { get; init; }

    /// <summary>
    /// Vytvoří podmíněný výraz.
    /// </summary>
    public ConditionalExpression(Expression condition, Expression whenTrue, Expression whenFalse)
    {
        Condition = condition;
        WhenTrue = whenTrue;
        WhenFalse = whenFalse;
        ResultType = whenTrue.ResultType; // true i false větev by měly mít stejný typ
    }
}
