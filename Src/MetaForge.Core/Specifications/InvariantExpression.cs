// ---------------------------------------------------------------------------
// MetaForge.Core — InvariantExpression
// Serialisable boolean AST for invariant conditions (When / Must).
// Vrstva: Core / Specifications
// 
// PROPOSAL: PROP-036 — Core Specification Layer
// ---------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace MetaForge.Core.Specifications;

/// <summary>
/// Abstract base for all invariant expression nodes.
/// Forms a serialisable boolean AST used in When and Must clauses of InvariantDefinition.
/// </summary>
[JsonDerivedType(typeof(PropertyRef), typeDiscriminator: "propertyRef")]
[JsonDerivedType(typeof(ConstantExpression), typeDiscriminator: "constant")]
[JsonDerivedType(typeof(EqExpression), typeDiscriminator: "eq")]
[JsonDerivedType(typeof(NotExpression), typeDiscriminator: "not")]
[JsonDerivedType(typeof(AndExpression), typeDiscriminator: "and")]
[JsonDerivedType(typeof(OrExpression), typeDiscriminator: "or")]
[JsonDerivedType(typeof(ExistsExpression), typeDiscriminator: "exists")]
public abstract record InvariantExpression;

/// <summary>
/// References a property path on the target element using JSONPath-like syntax.
/// Example: "$.IsAbstract" or "$.Parameters.Length".
/// </summary>
public sealed record PropertyRef(string Path) : InvariantExpression
{
    /// <summary>Creates a PropertyRef that checks if the property equals a given value (shorthand).</summary>
    public EqExpression Eq(object? value) => new(this, new ConstantExpression(value));
}

/// <summary>
/// Represents a literal constant value in an invariant expression.
/// </summary>
public sealed record ConstantExpression(object? Value) : InvariantExpression;

/// <summary>
/// Equality comparison between two invariant sub-expressions.
/// </summary>
public sealed record EqExpression(InvariantExpression Left, InvariantExpression Right) : InvariantExpression;

/// <summary>
/// Logical negation of an invariant sub-expression.
/// </summary>
public sealed record NotExpression(InvariantExpression Inner) : InvariantExpression;

/// <summary>
/// Logical AND of multiple invariant sub-expressions. Empty list evaluates to true (identity).
/// </summary>
public sealed record AndExpression(IReadOnlyList<InvariantExpression> Items) : InvariantExpression
{
    /// <summary>Creates an And with the given items.</summary>
    public AndExpression(params InvariantExpression[] items) : this((IReadOnlyList<InvariantExpression>)items) { }
}

/// <summary>
/// Logical OR of multiple invariant sub-expressions. Empty list evaluates to false (identity).
/// </summary>
public sealed record OrExpression(IReadOnlyList<InvariantExpression> Items) : InvariantExpression
{
    /// <summary>Creates an Or with the given items.</summary>
    public OrExpression(params InvariantExpression[] items) : this((IReadOnlyList<InvariantExpression>)items) { }
}

/// <summary>
/// Checks existence of a property or element at the given path.
/// Example: `Exists("$.Body")` returns true if the element has a non-null body.
/// </summary>
public sealed record ExistsExpression(string Path) : InvariantExpression
{
    /// <summary>Creates a negated Exists (i.e., "does not exist").</summary>
    public NotExpression Not() => new(this);
}

/// <summary>
/// Helper for building invariant expressions fluently.
/// </summary>
public static class InvariantExpressionBuilder
{
    /// <summary>Creates a property reference (JSONPath).</summary>
    public static PropertyRef Prop(string path) => new(path);

    /// <summary>Creates a constant value.</summary>
    public static ConstantExpression Const(object? value) => new(value);

    /// <summary>Logical implication: when(a) must follow that must(b). Normalised to Or(Not(a), b).</summary>
    public static InvariantExpression Implies(InvariantExpression when, InvariantExpression must) =>
        new OrExpression(new[] { new NotExpression(when), must });
}
