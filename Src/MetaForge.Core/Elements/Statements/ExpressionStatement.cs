using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Výraz použitý jako statement — např. volání metody, inkrementace.
/// </summary>
public sealed class ExpressionStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Expression;

    /// <summary>Výraz, který se vykoná jako příkaz.</summary>
    public Expression Expr { get; init; } = default!;
}
