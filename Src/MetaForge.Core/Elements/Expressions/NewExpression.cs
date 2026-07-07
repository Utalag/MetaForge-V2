using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Výraz vytvoření instance — reprezentuje `new Customer(...) { Name = "..." }`.
/// </summary>
public sealed class NewExpression : Expression
{
    public override string Kind => "New";
    public override ExpressionKind ExpressionKind => ExpressionKind.New;

    /// <summary>Název vytvářeného typu (např. "Customer", "List&lt;string&gt;").</summary>
    public string TypeName { get; init; } = string.Empty;

    /// <summary>Argumenty konstruktoru — poziční nebo pojmenované.</summary>
    public IReadOnlyList<NamedArgument> Arguments { get; init; } = Array.Empty<NamedArgument>();

    /// <summary>Object initializer — dvojice (název členu, hodnota), např. `{ Name = "..." }`.</summary>
    public IReadOnlyList<NamedArgument> Initializers { get; init; } = Array.Empty<NamedArgument>();

    /// <summary>
    /// Vytvoří výraz `new`.
    /// </summary>
    /// <param name="typeName">Název vytvářeného typu.</param>
    /// <param name="arguments">Argumenty konstruktoru.</param>
    /// <param name="initializers">Object initializer páry.</param>
    /// <param name="resultType">Výsledný typ (typicky Entity/Struct/Record odkazující na typeName).</param>
    public NewExpression(
        string typeName,
        IReadOnlyList<NamedArgument>? arguments = null,
        IReadOnlyList<NamedArgument>? initializers = null,
        TypeModel? resultType = null)
    {
        TypeName = typeName;
        Arguments = arguments ?? Array.Empty<NamedArgument>();
        Initializers = initializers ?? Array.Empty<NamedArgument>();
        ResultType = resultType ?? TypeModel.Object.WithCustomName(typeName);
    }
}
