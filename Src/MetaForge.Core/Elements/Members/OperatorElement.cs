using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Statements;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Reprezentuje přetížený C# operátor — vždy `public static`.
/// Např. `public static Money operator +(Money a, Money b) { ... }`.
/// </summary>
public sealed class OperatorElement
{
    /// <summary>Druh přetěžovaného operátoru.</summary>
    public OperatorKind Operator { get; set; }

    /// <summary>Návratový typ operátoru.</summary>
    public TypeModel ReturnType { get; set; } = TypeModel.Object;

    /// <summary>Parametry operátoru (1 pro unární, 2 pro binární).</summary>
    public List<ParameterElement> Parameters { get; } = new();

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    /// <summary>Atributy na operátoru.</summary>
    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>Tělo operátoru jako AST.</summary>
    public BlockStatement? Body { get; set; }

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 5;

    /// <summary>Vytvoří binární operátor (2 parametry).</summary>
    public static OperatorElement Binary(OperatorKind op, TypeModel returnType, ParameterElement left, ParameterElement right)
    {
        var element = new OperatorElement { Operator = op, ReturnType = returnType };
        element.Parameters.Add(left);
        element.Parameters.Add(right);
        return element;
    }

    /// <summary>Vytvoří unární operátor (1 parametr).</summary>
    public static OperatorElement Unary(OperatorKind op, TypeModel returnType, ParameterElement operand)
    {
        var element = new OperatorElement { Operator = op, ReturnType = returnType };
        element.Parameters.Add(operand);
        return element;
    }

    /// <summary>Nastaví tělo operátoru.</summary>
    public OperatorElement WithBody(BlockStatement? body)
    {
        Body = body;
        return this;
    }
}

/// <summary>Druh přetěžovaného operátoru.</summary>
public enum OperatorKind
{
    Add,            // +
    Subtract,       // -
    Multiply,       // *
    Divide,         // /
    Modulo,         // %
    Equality,       // ==
    Inequality,     // !=
    GreaterThan,    // >
    LessThan,       // <
    GreaterThanOrEqual, // >=
    LessThanOrEqual,    // <=
    UnaryPlus,      // +x
    UnaryNegation,  // -x
    LogicalNot,     // !x
    BitwiseComplement, // ~x
    Increment,      // ++x
    Decrement,      // --x
    True,           // operator true
    False,          // operator false
    BitwiseAnd,     // &
    BitwiseOr,      // |
    ExclusiveOr,    // ^
    LeftShift,      // <<
    RightShift,     // >>
}
