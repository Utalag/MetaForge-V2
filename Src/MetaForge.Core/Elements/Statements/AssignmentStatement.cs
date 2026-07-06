using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Přiřazení hodnoty do proměnné — varName = Value;
/// </summary>
public sealed class AssignmentStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Assignment;

    /// <summary>Název proměnné (levá strana).</summary>
    public string Variable { get; init; } = string.Empty;

    /// <summary>Hodnota k přiřazení (pravá strana).</summary>
    public Expression Value { get; init; } = default!;
}
