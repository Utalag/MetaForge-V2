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

    /// <summary>Argumenty volání metody — poziční i pojmenované.</summary>
    public IReadOnlyList<NamedArgument> Arguments { get; init; } = Array.Empty<NamedArgument>();

    /// <summary>
    /// Vytvoří výraz volání metody s pozičními argumenty.
    /// </summary>
    /// <param name="methodName">Plně kvalifikovaný název metody.</param>
    /// <param name="arguments">Argumenty volání (poziční).</param>
    /// <param name="resultType">Návratový typ metody.</param>
    public MethodCallExpression(string methodName, IReadOnlyList<Expression> arguments, TypeModel? resultType = null)
    {
        MethodName = methodName;
        Arguments = arguments.Select(a => new NamedArgument(a)).ToList();
        ResultType = resultType ?? TypeModel.Object;
    }

    /// <summary>
    /// Vytvoří výraz volání metody s pozičními i pojmenovanými argumenty.
    /// Samostatná factory metoda (místo přetížení konstruktoru), aby se předešlo
    /// nejednoznačnosti při volání s prázdným seznamem argumentů (`[]`).
    /// </summary>
    /// <param name="methodName">Plně kvalifikovaný název metody.</param>
    /// <param name="arguments">Argumenty volání (poziční nebo pojmenované).</param>
    /// <param name="resultType">Návratový typ metody.</param>
    public static MethodCallExpression WithNamedArguments(
        string methodName, IReadOnlyList<NamedArgument> arguments, TypeModel? resultType = null) =>
        new(methodName, Array.Empty<Expression>(), resultType) { Arguments = arguments };
}
