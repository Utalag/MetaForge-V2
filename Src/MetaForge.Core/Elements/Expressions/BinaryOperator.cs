namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Binární operátor pro BinaryExpression.
/// </summary>
public enum BinaryOperator
{
    // Aritmetické
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,

    // Porovnávací
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,

    // Logické
    And,
    Or,

    // Řetězcové
    Concat,

    // Null-coalescing
    NullCoalesce,
}
