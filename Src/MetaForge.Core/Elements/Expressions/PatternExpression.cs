using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Vzor pro pattern matching — používá se ve `switch` větvích nebo `is` výrazech.
/// Např. `case int i when i &gt; 0:`, `case Customer c:`, `case null:`, `case _:`.
/// </summary>
public sealed class PatternExpression : Expression
{
    public override string Kind => "Pattern";
    public override ExpressionKind ExpressionKind => ExpressionKind.Pattern;

    /// <summary>Druh vzoru.</summary>
    public PatternKind PatternKind { get; init; } = PatternKind.Discard;

    /// <summary>
    /// Konstantní hodnota pro <see cref="PatternKind.Constant"/> (např. 0, "text", null).
    /// </summary>
    public object? ConstantValue { get; init; }

    /// <summary>
    /// Název typu pro <see cref="PatternKind.Type"/> (např. "Customer", "int").
    /// </summary>
    public string? TypeName { get; init; }

    /// <summary>
    /// Název proměnné, do které se vzor naváže (deklarace, např. `Customer c`).
    /// Null pokud se hodnota nepojmenovává.
    /// </summary>
    public string? BindingName { get; init; }

    /// <summary>
    /// Vytvoří vzor pro pattern matching.
    /// </summary>
    public PatternExpression(PatternKind patternKind, TypeModel? resultType = null)
    {
        PatternKind = patternKind;
        ResultType = resultType ?? TypeModel.Bool;
    }

    /// <summary>Vzor "zahoď hodnotu" — `_`.</summary>
    public static PatternExpression Discard() => new(PatternKind.Discard);

    /// <summary>Konstantní vzor — `case 42:`, `case "x":`, `case null:`.</summary>
    public static PatternExpression Constant(object? value) =>
        new(PatternKind.Constant) { ConstantValue = value };

    /// <summary>Typový vzor — `case Customer c:` (bindingName volitelný).</summary>
    public static PatternExpression Type(string typeName, string? bindingName = null) =>
        new(PatternKind.Type) { TypeName = typeName, BindingName = bindingName };
}

/// <summary>Druh vzoru pro pattern matching.</summary>
public enum PatternKind
{
    /// <summary>Zahoď hodnotu — `_`.</summary>
    Discard,

    /// <summary>Konstantní hodnota — `42`, `"text"`, `null`.</summary>
    Constant,

    /// <summary>Typový vzor s volitelnou deklarací — `Customer c`.</summary>
    Type,

    /// <summary>Relační vzor — `&gt; 0`, `&lt;= 100`.</summary>
    Relational,

    /// <summary>Var vzor — `var x`, vždy odpovídá a naváže hodnotu.</summary>
    Var,
}
