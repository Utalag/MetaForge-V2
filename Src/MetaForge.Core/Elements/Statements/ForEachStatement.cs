using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Foreach statement — <c>foreach (var item in collection) { }</c>.
/// </summary>
public sealed class ForEachStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.ForEach;

    /// <summary>Název iterační proměnné (např. "item").</summary>
    public string VariableName { get; init; } = string.Empty;

    /// <summary>Typ iterační proměnné (volitelný — používá se <c>var</c>).</summary>
    public string? VariableType { get; init; }

    /// <summary>Výraz reprezentující kolekci (např. MemberAccessExpression).</summary>
    public Expression Collection { get; init; } = null!;

    /// <summary>Tělo cyklu (typicky BlockStatement).</summary>
    public Statement? Body { get; set; }

    public ForEachStatement() { }
    public ForEachStatement(string variableName, Expression collection, Statement? body)
    {
        VariableName = variableName;
        Collection = collection;
        Body = body;
    }
}
