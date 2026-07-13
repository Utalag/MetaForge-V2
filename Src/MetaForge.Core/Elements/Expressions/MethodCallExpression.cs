using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Výraz volání metody — reprezentuje volání statické nebo instanční metody.
/// Např. string.IsNullOrEmpty(name), collection.Contains(item).
/// </summary>
public sealed class MethodCallExpression : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.MethodCall;

    /// <summary>Název metody (např. "string.IsNullOrEmpty").</summary>
    public string MethodName { get; init; } = string.Empty;

    /// <summary>Argumenty volání metody.</summary>
    public IReadOnlyList<Expression> Arguments { get; init; } = Array.Empty<Expression>();

    /// <summary>
    /// Pojmenované argumenty (C# 4+). Prázdné = poziční.
    /// Např. <c>MethodCall(p1: 5, p2: "hello")</c> → ["p1", "p2"].
    /// Pokud je položka null, znamená poziční argument na dané pozici.
    /// </summary>
    public IReadOnlyList<string?> ArgumentNames { get; init; } = Array.Empty<string?>();

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
