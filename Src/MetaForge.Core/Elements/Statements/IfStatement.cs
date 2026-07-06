using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Podmíněný statement — if (Condition) TrueBranch else FalseBranch.
/// FalseBranch je null pokud else větev chybí.
/// </summary>
public sealed class IfStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.If;

    /// <summary>Podmínka — musí být bool výraz.</summary>
    public Expression Condition { get; init; } = default!;

    /// <summary>Větev pro true. Null = prázdný blok.</summary>
    public Statement? TrueBranch { get; init; }

    /// <summary>Větev pro false (else). Null = chybí.</summary>
    public Statement? FalseBranch { get; init; }
}
