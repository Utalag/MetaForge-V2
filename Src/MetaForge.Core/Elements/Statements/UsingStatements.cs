using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Using statement — <c>using (var resource = new StreamReader(...)) { }</c>.
/// </summary>
public sealed class UsingStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Using;

    /// <summary>Deklarace resource (např. <c>var reader = new StreamReader(...)</c>).</summary>
    public Statement? ResourceDeclaration { get; set; }

    /// <summary>Tělo using bloku.</summary>
    public Statement? Body { get; set; }

    public UsingStatement() { }
    public UsingStatement(Statement resourceDeclaration, Statement body)
    {
        ResourceDeclaration = resourceDeclaration;
        Body = body;
    }
}

/// <summary>
/// Using deklarace (C# 8+) — <c>using var reader = new StreamReader(...);</c>.
/// Není blok — resource je uvolněn na konci scope.
/// </summary>
public sealed class UsingDeclarationStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.UsingDeclaration;

    /// <summary>Název proměnné.</summary>
    public string VariableName { get; init; } = string.Empty;

    /// <summary>Inicializační výraz (např. <c>new StreamReader(...)</c>).</summary>
    public Expression Initializer { get; init; } = null!;

    public UsingDeclarationStatement() { }
    public UsingDeclarationStatement(string variableName, Expression initializer)
    {
        VariableName = variableName;
        Initializer = initializer;
    }
}
