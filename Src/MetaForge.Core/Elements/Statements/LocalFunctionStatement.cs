using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Lokální funkce definovaná uvnitř těla metody.
/// Např. `int Square(int x) { return x * x; }` uvnitř jiné metody.
/// </summary>
public sealed class LocalFunctionStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.LocalFunction;

    /// <summary>Název lokální funkce.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Návratový typ (výchozí void).</summary>
    public TypeModel ReturnType { get; init; } = TypeModel.Void;

    /// <summary>Parametry lokální funkce.</summary>
    public List<ParameterElement> Parameters { get; init; } = [];

    /// <summary>Je funkce `static` (nemůže zachytávat proměnné z okolí)?</summary>
    public bool IsStatic { get; init; }

    /// <summary>Je funkce `async`?</summary>
    public bool IsAsync { get; init; }

    /// <summary>Tělo lokální funkce.</summary>
    public BlockStatement Body { get; init; } = default!;
}
