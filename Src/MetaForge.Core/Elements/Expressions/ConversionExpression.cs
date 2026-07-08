using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Konverze typu — <c>(decimal)price</c>, <c>(int)Math.Round(x)</c>.
/// </summary>
public sealed class ConversionExpression : Expression
{
    public override string Kind => "Conversion";
    public override ExpressionKind ExpressionKind => ExpressionKind.Conversion;

    /// <summary>Cílový typ, na který se převádí.</summary>
    public TypeModel TargetType { get; init; } = TypeModel.Object;

    /// <summary>Výraz, který se převádí.</summary>
    public Expression Operand { get; init; } = null!;

    /// <summary>Je to explicitní konverze (cast) nebo implicitní?</summary>
    public bool IsExplicit { get; init; } = true;

    public ConversionExpression(TypeModel targetType, Expression operand, bool isExplicit = true)
    {
        TargetType = targetType;
        Operand = operand;
        IsExplicit = isExplicit;
        ResultType = targetType;
    }
}
