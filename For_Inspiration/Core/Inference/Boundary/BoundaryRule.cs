namespace MetaForge.Core.Inference.Boundary;

/// <summary>
/// Jedno konfigurační pravidlo pro RulesBoundaryAnalyzer.
///
/// Pokrývá 80 % jednoduchých boundary případů bez nutnosti psát vlastní IDomainAnalyzer.
/// Pro komplexní logiku závislou na kontextu (např. amount > Balance) použij plugin.
///
/// Příklady:
/// <code>
/// new BoundaryRule
/// {
///     ParamNamePattern = "amount|value|price|fee",
///     Condition        = "{param} &lt;= 0",
///     ExceptionType    = "ArgumentException",
///     ExceptionMessage = "Parametr '{param}' musí být kladné číslo."
/// }
///
/// new BoundaryRule
/// {
///     MethodNamePattern = "log|ln",
///     ParamNamePattern  = "x|value|input",
///     Condition         = "{param} &lt;= 0",
///     ExceptionType     = "ArgumentOutOfRangeException",
///     ExceptionMessage  = "Parametr '{param}' musí být kladné číslo (logaritmus)."
/// }
/// </code>
/// </summary>
public sealed record BoundaryRule
{
    /// <summary>
    /// Regex pattern pro název parametru (case-insensitive).
    /// Null = pravidlo platí pro všechny parametry.
    /// Příklad: "amount|value|price|fee"
    /// </summary>
    public string? ParamNamePattern { get; init; }

    /// <summary>
    /// Regex pattern pro název metody (case-insensitive).
    /// Null = pravidlo platí pro všechny metody.
    /// Příklad: "deposit|withdraw|pay"
    /// </summary>
    public string? MethodNamePattern { get; init; }

    /// <summary>
    /// Typ parametru pro které pravidlo platí.
    /// Null = všechny typy.
    /// Příklad: "double", "int", "decimal"
    /// </summary>
    public string? ParamTypeName { get; init; }

    /// <summary>
    /// C# podmínka triggerjující guard clause.
    /// Placeholder {param} se nahradí skutečným názvem parametru.
    /// Příklad: "{param} &lt;= 0"
    /// </summary>
    public required string Condition { get; init; }

    /// <summary>
    /// Typ vyhazované výjimky.
    /// Výchozí: "ArgumentException"
    /// </summary>
    public string ExceptionType { get; init; } = "ArgumentException";

    /// <summary>
    /// Zpráva výjimky. Placeholder {param} se nahradí názvem parametru.
    /// Příklad: "Parametr '{param}' musí být kladné číslo."
    /// </summary>
    public required string ExceptionMessage { get; init; }

    /// <summary>
    /// Popis pravidla pro dokumentaci / diagnostiku.
    /// Placeholder {param} se nahradí názvem parametru.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Aplikuje placeholdery a vrátí instanci s vyplněnými hodnotami pro konkrétní parametr.
    /// </summary>
    internal BoundaryRule Resolve(string paramName) => this with
    {
        Condition        = Condition.Replace("{param}", paramName),
        ExceptionMessage = ExceptionMessage.Replace("{param}", paramName),
        Description      = Description?.Replace("{param}", paramName)
    };
}
