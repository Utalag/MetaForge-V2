using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Jedna catch klauzule — `catch (ExceptionType VariableName) { Body }`.
/// </summary>
public sealed class CatchClause
{
    /// <summary>
    /// Typ zachytávané výjimky. Null = obecné `catch { }` bez typu.
    /// </summary>
    public TypeModel? ExceptionType { get; init; }

    /// <summary>Název proměnné výjimky. Null pokud se nepoužívá.</summary>
    public string? VariableName { get; init; }

    /// <summary>Volitelná `when` podmínka (exception filter).</summary>
    public Elements.Expressions.Expression? Filter { get; init; }

    /// <summary>Tělo catch bloku.</summary>
    public BlockStatement Body { get; init; } = default!;
}

/// <summary>
/// Try/catch/finally statement — zachytávání výjimek.
/// </summary>
public sealed class TryCatchStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.TryCatch;

    /// <summary>Tělo try bloku.</summary>
    public BlockStatement TryBlock { get; init; } = default!;

    /// <summary>Seznam catch klauzulí (v pořadí vyhodnocení).</summary>
    public List<CatchClause> CatchClauses { get; init; } = [];

    /// <summary>Volitelný finally blok. Null pokud chybí.</summary>
    public BlockStatement? FinallyBlock { get; init; }
}
