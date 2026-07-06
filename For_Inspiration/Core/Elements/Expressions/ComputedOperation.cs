namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Jazykově-neutrální operace pro deklarativní logiku property a metod.
/// Každá operace má odpovídající generátor v per-language šablonách.
/// </summary>
public enum ComputedOperation
{
    /// <summary>
    /// Vrácení hodnoty: return {expression}.
    /// C# → return expr; | Python → return expr | TS → return expr;
    /// </summary>
    Return,

    /// <summary>
    /// Konstrukce instance custom typu s argumenty.
    /// Jazykove specificka syntaxe konstrukce se materializuje az pri renderu.
    /// </summary>
    ConstructInstance,

    /// <summary>
    /// Vrátí formátovaný string podle šablony s placeholdery.
    /// C# → return $"..."; | Python → return f"..." | TS → return `...`;
    /// </summary>
    ReturnFormatted,

    /// <summary>
    /// Přiřazení hodnoty: {target} = {expression}.
    /// </summary>
    Assign,

    /// <summary>
    /// Deklarace lokální proměnné: {type} {name} = {expression}.
    /// Typ se ukládá semanticky v DeclaredType a mapuje se až při renderu do cílového jazyka.
    /// </summary>
    DeclareVariable,

    /// <summary>
    /// Omezení hodnoty do rozsahu: Clamp(value, min, max).
    /// C# → Math.Clamp(...) | Python → max(min(...), ...) | TS → Math.max(Math.min(...))
    /// </summary>
    Clamp,

    /// <summary>
    /// Vyhození výjimky pokud je hodnota null.
    /// C# → ArgumentNullException | Python → ValueError | TS → TypeError
    /// </summary>
    ThrowIfNull,

    /// <summary>
    /// Vyhození výjimky pokud je hodnota mimo rozsah (min/max).
    /// C# → ArgumentOutOfRangeException | Python → ValueError | TS → RangeError
    /// </summary>
    ThrowIfOutOfRange,

    /// <summary>
    /// Vyhození výjimky pokud string je prázdný nebo whitespace.
    /// C# → ArgumentException | Python → ValueError | TS → Error
    /// </summary>
    ThrowIfEmpty,

    /// <summary>
    /// Porovnání dvou hodnot: {left} {operator} {right}.
    /// Operátor se ukládá v ComparisonOperator.
    /// </summary>
    Comparison,

    /// <summary>
    /// Podmíněný výraz: if ({condition}) { thenBranch } else { elseBranch }.
    /// </summary>
    Conditional,

    /// <summary>
    /// Komplexní if/else-if/else řetězec s více větvemi.
    /// Ukládá seznam podmínek a příslušných větví pro generování složitějších podmínkových struktur.
    /// </summary>
    IfChain,

    /// <summary>
    /// Přístup k členu objektu: {target}.{member}.
    /// </summary>
    MemberAccess,

    /// <summary>
    /// Řetězcová interpolace / formátování.
    /// C# → $"..." | Python → f"..." | TS → `...`
    /// </summary>
    StringFormat,

    /// <summary>
    /// Vlastní (raw) výraz — fallback pro případy, které nelze vyjádřit deklarativně.
    /// Hodnota v RawCode, jazykově specifická.
    /// </summary>
    Raw
}

/// <summary>
/// Operátor porovnání pro ComputedOperation.Comparison.
/// </summary>
public enum ComparisonOperator
{
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}
