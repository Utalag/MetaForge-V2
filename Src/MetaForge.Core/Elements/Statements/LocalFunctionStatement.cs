using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Lokální funkce — <c>static int Helper(int x) { return x * 2; }</c> uvnitř těla metody.
/// </summary>
public sealed class LocalFunctionStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.LocalFunction;

    /// <summary>Metoda reprezentující lokální funkci.</summary>
    public MethodElement Function { get; init; } = null!;

    public LocalFunctionStatement() { }
    public LocalFunctionStatement(MethodElement function)
    {
        Function = function;
    }
}
