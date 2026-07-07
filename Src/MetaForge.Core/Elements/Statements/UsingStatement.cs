using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Using statement — deterministické uvolnění IDisposable zdroje.
/// Podporuje jak blokovou formu `using (Resource) { Body }`,
/// tak deklarační formu `using var x = Resource;` (kdy <see cref="Body"/> je null).
/// </summary>
public sealed class UsingStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Using;

    /// <summary>Název proměnné pro uvolňovaný zdroj.</summary>
    public string Variable { get; init; } = string.Empty;

    /// <summary>Typ proměnné. Null = odvodit (`var`).</summary>
    public TypeModel? VariableType { get; init; }

    /// <summary>Výraz vytvářející/odkazující zdroj (IDisposable).</summary>
    public Expression Resource { get; init; } = default!;

    /// <summary>
    /// Tělo bloku `using (...) { ... }`. Null pro using-deklaraci
    /// (`using var x = ...;`), kdy se zdroj uvolní na konci obklopujícího bloku.
    /// </summary>
    public Statement? Body { get; init; }
}
