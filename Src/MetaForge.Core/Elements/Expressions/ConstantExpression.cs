using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Konstantní výraz — reprezentuje literál (42, "hello", true, null).
/// </summary>
public sealed class ConstantExpression : Expression
{
    public override string Kind => "Constant";
    public override ExpressionKind ExpressionKind => ExpressionKind.Constant;

    /// <summary>Hodnota konstanty (může být null).</summary>
    public object? Value { get; init; }

    /// <summary>
    /// Vytvoří konstantní výraz.
    /// </summary>
    /// <param name="value">Hodnota konstanty.</param>
    /// <param name="resultType">Typ hodnoty.</param>
    public ConstantExpression(object? value, TypeModel? resultType = null)
    {
        Value = value;
        ResultType = resultType ?? InferType(value);
    }

    private static TypeModel InferType(object? value) => value switch
    {
        null => TypeModel.Object,
        string => TypeModel.String,
        int or long or short or byte => TypeModel.Int32,
        decimal or double or float => TypeModel.Decimal,
        bool => TypeModel.Bool,
        DateTime or DateTimeOffset => TypeModel.DateTime,
        Guid => TypeModel.Guid,
        _ => TypeModel.Object,
    };
}
