using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Výraz volání metody — reprezentuje volání statické nebo instanční metody.
/// Např. string.IsNullOrEmpty(name), collection.Contains(item).
/// </summary>
public sealed class MethodCallExpression : Expression
{
    public override string Kind => "MethodCall";
    public override ExpressionKind ExpressionKind => ExpressionKind.MethodCall;

    /// <summary>Název metody (např. "string.IsNullOrEmpty").</summary>
    public string MethodName { get; init; } = string.Empty;

    /// <summary>Argumenty volání metody.</summary>
    public IReadOnlyList<Expression> Arguments { get; init; } = Array.Empty<Expression>();

    /// <summary>
    /// Vytvoří výraz volání metody.
    /// </summary>
    /// <param name="methodName">Plně kvalifikovaný název metody.</param>
    /// <param name="arguments">Argumenty volání.</param>
    /// <param name="resultType">Návratový typ metody.</param>
    public MethodCallExpression(string methodName, IReadOnlyList<Expression> arguments, TypeModel? resultType = null)
    {
        MethodName = methodName;
        Arguments = arguments;
        ResultType = resultType ?? TypeModel.Object;
    }
}
