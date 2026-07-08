namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Try-catch-finally statement — <c>try { } catch (Exception ex) { } finally { }</c>.
/// </summary>
public sealed class TryCatchStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.TryCatch;

    /// <summary>Tělo try bloku.</summary>
    public BlockStatement TryBody { get; init; } = null!;

    /// <summary>Seznam catch klauzulí (alespoň jedna).</summary>
    public List<CatchClause> Catches { get; init; } = [];

    /// <summary>Volitelný finally blok.</summary>
    public BlockStatement? FinallyBody { get; set; }

    public TryCatchStatement() { }
    public TryCatchStatement(BlockStatement tryBody, params CatchClause[] catches)
    {
        TryBody = tryBody;
        Catches = catches.ToList();
    }
}

/// <summary>
/// Jedna catch klauzule — <c>catch (ExceptionType ex) { }</c> nebo <c>catch { }</c>.
/// </summary>
public sealed class CatchClause
{
    /// <summary>Typ výjimky (např. "Exception", "InvalidOperationException"). Null = catch-all.</summary>
    public string? ExceptionType { get; init; }

    /// <summary>Název proměnné výjimky (např. "ex"). Null = bez proměnné.</summary>
    public string? VariableName { get; init; }

    /// <summary>Tělo catch bloku.</summary>
    public BlockStatement Body { get; init; } = null!;

    /// <summary>Podmíněný catch filtr — <c>catch (Exception ex) when (ex is ...)</c>.</summary>
    public string? Filter { get; init; }

    public CatchClause() { }
    public CatchClause(string? exceptionType, string? variableName, BlockStatement body, string? filter = null)
    {
        ExceptionType = exceptionType;
        VariableName = variableName;
        Body = body;
        Filter = filter;
    }
}
