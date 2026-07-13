using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Await výraz — <c>await GetDataAsync()</c>.
/// Používá se pro asynchronní volání uvnitř async metod.
/// </summary>
public sealed class AwaitExpression : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.Await;

    /// <summary>Výraz, na který se čeká (obvykle MethodCallExpression).</summary>
    public Expression Operand { get; init; } = null!;

    public AwaitExpression(Expression operand, TypeModel? resultType = null)
    {
        Operand = operand;
        ResultType = resultType ?? TypeModel.Object;
    }
}
