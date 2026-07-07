using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// ForEach cyklus — `foreach (ElementType Variable in Collection) Body`.
/// </summary>
public sealed class ForEachStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.ForEach;

    /// <summary>Název iterační proměnné (např. "item").</summary>
    public string Variable { get; init; } = string.Empty;

    /// <summary>Typ iterační proměnné. Null = odvodit (`var`).</summary>
    public TypeModel? ElementType { get; init; }

    /// <summary>Kolekce, přes kterou se iteruje.</summary>
    public Expression Collection { get; init; } = default!;

    /// <summary>Tělo cyklu.</summary>
    public Statement Body { get; init; } = default!;
}
