using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Switch výraz (C# 8+) — <c>x switch { 1 => "one", _ => "many" }</c>.
/// </summary>
public sealed class SwitchExpression : Expression
{
    public override string Kind => "Switch";
    public override ExpressionKind ExpressionKind => ExpressionKind.Switch;

    /// <summary>Výraz, který se vyhodnocuje (selector).</summary>
    public Expression Selector { get; init; } = null!;

    /// <summary>Větve (arms) switch výrazu.</summary>
    public IReadOnlyList<SwitchArm> Arms { get; init; } = Array.Empty<SwitchArm>();

    public SwitchExpression(Expression selector, IReadOnlyList<SwitchArm> arms, TypeModel? resultType = null)
    {
        Selector = selector;
        Arms = arms;
        ResultType = resultType ?? TypeModel.Object;
    }
}

/// <summary>
/// Jedna větev switch výrazu — <c>pattern => value</c>.
/// </summary>
public sealed class SwitchArm
{
    /// <summary>Pattern (např. ConstantExpression pro "1", DiscardPattern pro "_").</summary>
    public Expression Pattern { get; init; } = null!;

    /// <summary>Hodnota vrácená při shodě.</summary>
    public Expression Value { get; init; } = null!;
}
