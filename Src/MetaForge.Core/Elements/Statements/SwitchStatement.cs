using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Jedna větev switch statementu — `case Pattern when Guard: Body`.
/// </summary>
public sealed class SwitchCase
{
    /// <summary>
    /// Vzor větve. Null reprezentuje `default:` větev.
    /// </summary>
    public PatternExpression? Pattern { get; init; }

    /// <summary>Volitelná `when` podmínka (guard clause).</summary>
    public Expression? Guard { get; init; }

    /// <summary>Tělo větve.</summary>
    public Statement Body { get; init; } = default!;
}

/// <summary>
/// Switch statement — `switch (Selector) { case ...: ... default: ... }`.
/// Podporuje pattern matching přes <see cref="PatternExpression"/> ve větvích.
/// </summary>
public sealed class SwitchStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Switch;

    /// <summary>Výraz, podle kterého se větví (selector).</summary>
    public Expression Selector { get; init; } = default!;

    /// <summary>Seznam větví (case + volitelně default).</summary>
    public List<SwitchCase> Cases { get; init; } = [];
}
