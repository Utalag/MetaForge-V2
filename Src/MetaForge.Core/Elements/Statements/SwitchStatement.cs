using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Switch statement — <c>switch (expression) { case X: ... default: ... }</c>.
/// </summary>
public sealed class SwitchStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Switch;

    /// <summary>Výraz, který se vyhodnocuje (selector).</summary>
    public Expression Selector { get; init; } = null!;

    /// <summary>Jednotlivé case větve.</summary>
    public List<SwitchCase> Cases { get; init; } = [];

    /// <summary>Volitelná default větev.</summary>
    public BlockStatement? DefaultCase { get; set; }

    public SwitchStatement() { }
    public SwitchStatement(Expression selector, params SwitchCase[] cases)
    {
        Selector = selector;
        Cases = cases.ToList();
    }
}

/// <summary>
/// Jedna case větev switch statementu — <c>case pattern: { ... break; }</c>.
/// </summary>
public sealed class SwitchCase
{
    /// <summary>Pattern — konstantní hodnota nebo výraz.</summary>
    public Expression Pattern { get; init; } = null!;

    /// <summary>Tělo case větve (typicky BlockStatement).</summary>
    public Statement Body { get; init; } = null!;

    public SwitchCase() { }
    public SwitchCase(Expression pattern, Statement body)
    {
        Pattern = pattern;
        Body = body;
    }
}
