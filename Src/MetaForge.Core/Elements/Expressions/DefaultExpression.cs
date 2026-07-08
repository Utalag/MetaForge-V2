using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Výchozí hodnota výraz — <c>default(int)</c>, <c>default(string)</c>.
/// </summary>
public sealed class DefaultExpression : Expression
{
    public override string Kind => "Default";
    public override ExpressionKind ExpressionKind => ExpressionKind.Default;

    /// <summary>Typ, pro který se zjišťuje defaultní hodnota.</summary>
    public TypeModel TargetType { get; init; } = TypeModel.Object;

    public DefaultExpression(TypeModel targetType)
    {
        TargetType = targetType;
        ResultType = targetType;
    }

    /// <summary>Vytvoří <c>default</c> bez explicitního typu (inference z kontextu).</summary>
    public static DefaultExpression Untyped() => new(TypeModel.Object);
}
