using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Návratový statement — return X;
/// Value je null pro void metody (return;)
/// </summary>
public sealed class ReturnStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Return;

    /// <summary>Návratová hodnota. Null pro void návrat.</summary>
    public Expression? Value { get; init; }
}
