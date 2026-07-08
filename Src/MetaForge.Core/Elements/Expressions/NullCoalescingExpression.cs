using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Null coalescing výraz — <c>a ?? b</c>, <c>a ?? throw new Exception()</c>.
/// </summary>
public sealed class NullCoalescingExpression : Expression
{
    public override string Kind => "NullCoalescing";
    public override ExpressionKind ExpressionKind => ExpressionKind.NullCoalescing;

    /// <summary>Levý operand (testovaný na null).</summary>
    public Expression Left { get; init; } = null!;

    /// <summary>Pravý operand (fallback hodnota).</summary>
    public Expression Right { get; init; } = null!;

    public NullCoalescingExpression(Expression left, Expression right, TypeModel? resultType = null)
    {
        Left = left;
        Right = right;
        ResultType = resultType ?? TypeModel.Object;
    }
}
