namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Druh výrazu pro dispatch a rozlišení konkrétních Expression potomků.
/// </summary>
public enum ExpressionKind
{
    /// <summary>Konstantní hodnota: 42, "hello", true.</summary>
    Constant,

    /// <summary>Přístup ke členu: entity.FirstName.</summary>
    MemberAccess,

    /// <summary>Binární operace: a + b, a > b, a AND b.</summary>
    Binary,

    /// <summary>Unární operace: !a, -a, ~a.</summary>
    Unary,

    /// <summary>Volání metody: string.IsNullOrEmpty(name).</summary>
    MethodCall,

    /// <summary>Lambda výraz: (x) => x.FirstName.</summary>
    Lambda,

    /// <summary>Vytvoření instance: new Customer { Name = "..." }.</summary>
    New,

    /// <summary>Podmíněný výraz: a ? b : c.</summary>
    Conditional,

    /// <summary>Výchozí hodnota: default(int), default(string).</summary>
    Default,

    /// <summary>Konverze typu: (decimal)price.</summary>
    Conversion,

    /// <summary>Vzor pro pattern matching: `is int i`, `case > 0`, `case Customer c`.</summary>
    Pattern,

    /// <summary>Složený výraz (obecný).</summary>
    Computed,
}
