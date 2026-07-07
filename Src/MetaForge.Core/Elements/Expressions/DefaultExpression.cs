using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Výchozí hodnota typu — reprezentuje `default(T)` nebo `default`.
/// </summary>
public sealed class DefaultExpression : Expression
{
    public override string Kind => "Default";
    public override ExpressionKind ExpressionKind => ExpressionKind.Default;

    /// <summary>
    /// Vytvoří výraz `default(T)` pro daný typ.
    /// </summary>
    /// <param name="type">Typ, pro který se vytváří výchozí hodnota.</param>
    public DefaultExpression(TypeModel type)
    {
        ResultType = type;
    }
}
