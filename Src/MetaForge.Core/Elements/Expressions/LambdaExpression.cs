using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Lambda výraz — reprezentuje anonymní funkci <c>(x) => x.Name</c> nebo <c>(a, b) => a + b</c>.
/// </summary>
public sealed class LambdaExpression : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.Lambda;

    /// <summary>Názvy parametrů lambda výrazu. Např. ["x"] nebo ["a", "b"].</summary>
    public IReadOnlyList<string> ParameterNames { get; init; } = Array.Empty<string>();

    /// <summary>Typy parametrů (volitelné — inferuje se z použití).</summary>
    public IReadOnlyList<TypeModel>? ParameterTypes { get; init; }

    /// <summary>Tělo lambda výrazu.</summary>
    public Expression Body { get; init; } = null!;

    /// <summary>Je lambda asynchronní? (<c>async (x) => await ...</c>).</summary>
    public bool IsAsync { get; init; }

    public LambdaExpression(string[] parameterNames, Expression body, TypeModel? resultType = null)
    {
        ParameterNames = parameterNames;
        Body = body;
        ResultType = resultType ?? TypeModel.Object;
    }
}
