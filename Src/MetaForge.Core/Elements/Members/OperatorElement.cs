// ---------------------------------------------------------------------------
// MetaForge.Core — OperatorElement + OperatorKind
// Represents a C# operator overload declaration.
// Vrstva: Core / Elements / Members
// 
// PROPOSAL: PROP-037 — C# Completeness
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Statements;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Kinds of C# operator overloads.
/// </summary>
public enum OperatorKind
{
    // Unary operators
    UnaryPlus,
    UnaryMinus,
    LogicalNot,
    BitwiseNot,
    Increment,
    Decrement,
    True,
    False,

    // Binary operators
    Addition,
    Subtraction,
    Multiply,
    Divide,
    Modulo,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    LeftShift,
    RightShift,

    // Comparison operators
    Equality,
    Inequality,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,

    // Conversion operators
    Implicit,
    Explicit
}

/// <summary>
/// Represents a C# operator overload.
/// Example: public static MyType operator +(MyType a, MyType b) { ... }
/// </summary>
public sealed class OperatorElement : IMemberElement
{
    /// <summary>The kind of operator being overloaded.</summary>
    public OperatorKind OperatorKind { get; set; }

    /// <summary>Return type of the operator.</summary>
    public DataTypes.TypeModel ReturnType { get; set; } = DataTypes.TypeModel.Void;

    /// <summary>Parameters of the operator (1 for unary, 2 for binary, 1 for conversion).</summary>
    public List<ParameterElement> Parameters { get; init; } = new();

    /// <summary>Operator body as AST (optional — can use ExpressionBody instead).</summary>
    public BlockStatement? Body { get; set; }

    /// <summary>Operator body as expression (for expression-bodied operators).</summary>
    public Expression? ExpressionBody { get; set; }

    /// <summary>Access modifier. Always Public for operators, but stored for metadata.</summary>
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    /// <summary>Whether the operator is static (C# operators are always static). Read-only.</summary>
    public bool IsStatic => true;

    /// <summary>Operator name (based on OperatorKind, e.g. "op_Addition").</summary>
    public string Name => $"op_{OperatorKind}";

    /// <summary>Attributes applied to the operator.</summary>
    public List<AttributeElement> Attributes { get; init; } = new();

    /// <summary>XML documentation summary for this operator.</summary>
    public string? XmlSummary { get; set; }

    /// <summary>Metadata annotations.</summary>
    public MetadataBag Metadata { get; init; } = new();

    /// <summary>Coin cost.</summary>
    public int Coin { get; set; } = 3;

    /// <summary>Creates a unary operator (e.g., operator +, operator -).</summary>
    public static OperatorElement Unary(OperatorKind kind, DataTypes.TypeModel returnType, ParameterElement operand) => new()
    {
        OperatorKind = kind,
        ReturnType = returnType,
        Parameters = { operand }
    };

    /// <summary>Creates a binary operator (e.g., operator +, operator ==).</summary>
    public static OperatorElement Binary(OperatorKind kind, DataTypes.TypeModel returnType, ParameterElement left, ParameterElement right) => new()
    {
        OperatorKind = kind,
        ReturnType = returnType,
        Parameters = { left, right }
    };

    /// <summary>Creates a conversion operator (implicit/explicit).</summary>
    public static OperatorElement Conversion(OperatorKind kind, DataTypes.TypeModel targetType, ParameterElement source) => new()
    {
        OperatorKind = kind is OperatorKind.Implicit or OperatorKind.Explicit ? kind : throw new ArgumentException("Conversion operator must be Implicit or Explicit.", nameof(kind)),
        ReturnType = targetType,
        Parameters = { source }
    };
}
