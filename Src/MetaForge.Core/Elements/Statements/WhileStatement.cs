using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// While cyklus — while (Condition) Body
/// </summary>
public sealed class WhileStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.While;

    /// <summary>Podmínka cyklu — musí být bool výraz.</summary>
    public Expression Condition { get; init; } = default!;

    /// <summary>Tělo cyklu.</summary>
    public Statement Body { get; init; } = default!;
}
