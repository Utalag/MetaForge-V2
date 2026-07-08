using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Výraz vytvoření instance: <c>new Customer { Name = "John" }</c>.
/// Podporuje object initializer s pojmenovanými přiřazeními.
/// </summary>
public sealed class NewExpression : Expression
{
    public override string Kind => "New";
    public override ExpressionKind ExpressionKind => ExpressionKind.New;

    /// <summary>Název typu (např. "Customer", "Point").</summary>
    public string TypeName { get; init; } = string.Empty;

    /// <summary>Argumenty konstruktoru (pro <c>new Point(3, 5)</c>).</summary>
    public IReadOnlyList<Expression> ConstructorArguments { get; init; } = Array.Empty<Expression>();

    /// <summary>
    /// Pojmenovaná přiřazení v object initializeru.
    /// Např. <c>new Customer { Name = "John", Age = 30 }</c> → [("Name", const), ("Age", const)].
    /// </summary>
    public IReadOnlyList<MemberBinding> MemberBindings { get; init; } = Array.Empty<MemberBinding>();

    public NewExpression(string typeName, IReadOnlyList<Expression>? constructorArgs = null, TypeModel? resultType = null)
    {
        TypeName = typeName;
        ConstructorArguments = constructorArgs ?? Array.Empty<Expression>();
        ResultType = resultType ?? TypeModel.Object;
    }
}

/// <summary>
/// Pojmenované přiřazení v object initializeru (<c>Name = "John"</c>).
/// </summary>
public sealed class MemberBinding
{
    /// <summary>Název členu (property nebo field).</summary>
    public string MemberName { get; init; } = string.Empty;

    /// <summary>Hodnota přiřazená členu.</summary>
    public Expression Value { get; init; } = null!;
}
