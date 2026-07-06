using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// For cyklus — for (Variable = Start; Variable < End; Variable++) Body
/// </summary>
public sealed class ForStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.For;

    /// <summary>Název iterační proměnné (např. "i").</summary>
    public string Variable { get; init; } = string.Empty;

    /// <summary>Počáteční hodnota.</summary>
    public Expression Start { get; init; } = default!;

    /// <summary>Koncová hodnota (exclusive — while Variable < End).</summary>
    public Expression End { get; init; } = default!;

    /// <summary>Tělo cyklu.</summary>
    public Statement? Body { get; init; }
}
