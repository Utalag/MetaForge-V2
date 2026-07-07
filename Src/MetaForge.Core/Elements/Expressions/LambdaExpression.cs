using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Lambda výraz — reprezentuje anonymní funkci, např. (x) => x.FirstName.
/// </summary>
public sealed class LambdaExpression : Expression
{
    public override string Kind => "Lambda";
    public override ExpressionKind ExpressionKind => ExpressionKind.Lambda;

    /// <summary>Názvy parametrů lambdy (bez explicitních typů — odvozují se z kontextu).</summary>
    public IReadOnlyList<string> Parameters { get; init; } = Array.Empty<string>();

    /// <summary>Tělo lambdy jako výraz (expression-bodied lambda).</summary>
    public Expression Body { get; init; }

    /// <summary>
    /// Vytvoří lambda výraz.
    /// </summary>
    /// <param name="parameters">Názvy parametrů lambdy.</param>
    /// <param name="body">Tělo lambdy (výraz).</param>
    /// <param name="resultType">Typ celé lambdy (typicky delegate/Func/Action).</param>
    public LambdaExpression(IReadOnlyList<string> parameters, Expression body, TypeModel? resultType = null)
    {
        Parameters = parameters;
        Body = body;
        ResultType = resultType ?? TypeModel.Object;
    }
}
