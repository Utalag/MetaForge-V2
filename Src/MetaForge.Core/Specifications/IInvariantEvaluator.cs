// ---------------------------------------------------------------------------
// MetaForge.Core — IInvariantEvaluator
// Evaluates invariants against a target element with a given context.
// Vrstva: Core / Specifications
// 
// PROPOSAL: PROP-036 — Core Specification Layer
// ---------------------------------------------------------------------------

namespace MetaForge.Core.Specifications;

/// <summary>
/// Context provided to the invariant evaluator for resolving paths and lookups.
/// </summary>
public sealed class InvariantEvaluationContext
{
    /// <summary>
    /// Optional parent element for Scoped evaluation.
    /// </summary>
    public object? Parent { get; init; }

    /// <summary>
    /// Optional model registry for Relational evaluation (lookup types, strong types, etc.).
    /// </summary>
    public IModelLookup? ModelLookup { get; init; }

    /// <summary>
    /// Optional full document context for Global evaluation.
    /// </summary>
    public object? DocumentRoot { get; init; }

    /// <summary>Creates a context for Local-scope evaluation (target only).</summary>
    public static InvariantEvaluationContext Local() => new();

    /// <summary>Creates a context for Scoped evaluation (target + parent).</summary>
    public static InvariantEvaluationContext Scoped(object parent) => new() { Parent = parent };

    /// <summary>Creates a context for Relational evaluation (target + model lookup).</summary>
    public static InvariantEvaluationContext Relational(IModelLookup lookup) => new() { ModelLookup = lookup };

    /// <summary>Creates a context for Global evaluation (target + full document).</summary>
    public static InvariantEvaluationContext Global(object documentRoot) => new() { DocumentRoot = documentRoot };
}

/// <summary>
/// Abstraction for looking up model elements (types, strong types, etc.) during Relational evaluation.
/// </summary>
public interface IModelLookup
{
    /// <summary>Returns true if a type with the given name exists in the model.</summary>
    bool TypeExists(string typeName);

    /// <summary>Returns true if a strong type with the given name is registered.</summary>
    bool StrongTypeExists(string strongTypeName);

    /// <summary>Gets all element names of the given kind within a parent context.</summary>
    IReadOnlyList<string> GetElementNames(string parentPath, string elementKind);
}

/// <summary>
/// Evaluates a set of invariants against a target element.
/// Implementations resolve property paths, evaluate the boolean AST, and collect violations.
/// </summary>
public interface IInvariantEvaluator
{
    /// <summary>
    /// Evaluates all invariants matching the target's kind against the target element.
    /// </summary>
    /// <param name="target">The element to evaluate (MethodElement, ClassElement, etc.).</param>
    /// <param name="context">Evaluation context (parent, model lookup, document root).</param>
    /// <param name="invariants">The set of invariants to evaluate.</param>
    /// <returns>Evaluation result with any violations found.</returns>
    EvaluationResult Evaluate(
        object target,
        InvariantEvaluationContext context,
        IReadOnlyList<InvariantDefinition> invariants);
}
