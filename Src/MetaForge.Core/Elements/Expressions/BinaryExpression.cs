using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Binární výraz — kombinuje dva podvýrazy operátorem.
/// Např. a + b, x > 5, firstName + " " + lastName.
/// </summary>
public sealed class BinaryExpression : Expression
{
    public override string Kind => "Binary";
    public override ExpressionKind ExpressionKind => ExpressionKind.Binary;

    /// <summary>Levý operand.</summary>
    public Expression Left { get; init; }

    /// <summary>Binární operátor.</summary>
    public BinaryOperator Operator { get; init; }

    /// <summary>Pravý operand.</summary>
    public Expression Right { get; init; }

    /// <summary>
    /// Vytvoří binární výraz.
    /// </summary>
    public BinaryExpression(Expression left, BinaryOperator op, Expression right, TypeModel? resultType = null)
    {
        Left = left;
        Operator = op;
        Right = right;
        ResultType = resultType ?? InferResultType(left, op, right);
    }

    private static TypeModel InferResultType(Expression left, BinaryOperator op, Expression right)
    {
        return op switch
        {
            BinaryOperator.And or BinaryOperator.Or
                or BinaryOperator.Equal or BinaryOperator.NotEqual
                or BinaryOperator.GreaterThan or BinaryOperator.LessThan
                or BinaryOperator.GreaterThanOrEqual or BinaryOperator.LessThanOrEqual
                => TypeModel.Bool,
            BinaryOperator.Add or BinaryOperator.Subtract or BinaryOperator.Multiply
                or BinaryOperator.Divide or BinaryOperator.Modulo
                => left.ResultType == TypeModel.Decimal || right.ResultType == TypeModel.Decimal
                    ? TypeModel.Decimal : TypeModel.Int32,
            BinaryOperator.Concat => TypeModel.String,
            _ => left.ResultType,
        };
    }
}
