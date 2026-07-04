using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Unární výraz — aplikuje operátor na jeden podvýraz.
/// Např. !isDeleted, -amount, ~flags.
/// </summary>
public sealed class UnaryExpression : Expression
{
    public override string Kind => "Unary";
    public override ExpressionKind ExpressionKind => ExpressionKind.Unary;

    /// <summary>Unární operátor.</summary>
    public UnaryOperator Operator { get; init; }

    /// <summary>Operand.</summary>
    public Expression Operand { get; init; }

    /// <summary>
    /// Vytvoří unární výraz.
    /// </summary>
    public UnaryExpression(UnaryOperator op, Expression operand, TypeModel? resultType = null)
    {
        Operator = op;
        Operand = operand;
        ResultType = resultType ?? (op == UnaryOperator.Not ? TypeModel.Bool : operand.ResultType);
    }
}

/// <summary>
/// Unární operátor pro UnaryExpression.
/// </summary>
public enum UnaryOperator
{
    /// <summary>Logická negace: !a.</summary>
    Not,

    /// <summary>Aritmetická negace: -a.</summary>
    Negate,

    /// <summary>Bitový doplněk: ~a.</summary>
    BitwiseNot,

    /// <summary>Inkrement: ++a.</summary>
    Increment,

    /// <summary>Dekrement: --a.</summary>
    Decrement,
}
