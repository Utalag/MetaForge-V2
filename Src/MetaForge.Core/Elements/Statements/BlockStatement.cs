using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Složený blok statementů — { stmt1; stmt2; ... }
/// Základní kontejner pro tělo metody nebo větve podmínek/cyklů.
/// </summary>
public sealed class BlockStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Block;

    /// <summary>Seznam statementů v bloku.</summary>
    public List<Statement> Statements { get; } = [];

    public BlockStatement() { }

    /// <summary>Vytvoří blok s danými statementy.</summary>
    public BlockStatement(params Statement[] statements) =>
        Statements.AddRange(statements);
}
