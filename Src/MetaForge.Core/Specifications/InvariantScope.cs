// ---------------------------------------------------------------------------
// MetaForge.Core — InvariantScope
// Scope defines what the invariant evaluates against.
// Vrstva: Core / Specifications
// ---------------------------------------------------------------------------

namespace MetaForge.Core.Specifications;

/// <summary>
/// Scope of an invariant — determines what context the evaluator provides.
/// </summary>
public enum InvariantScope
{
    /// <summary>
    /// Single element in isolation (e.g., MethodElement.IsAsync && MethodElement.IsAbstract => invalid).
    /// The evaluator receives only the target element.
    /// </summary>
    Local = 0,

    /// <summary>
    /// Element within its parent context (e.g., all PropertyElement in a ClassElement have unique Names).
    /// The evaluator receives the target element + its immediate parent.
    /// </summary>
    Scoped = 1,

    /// <summary>
    /// Element + lookup into the full model (e.g., PropertyElement.TypeModel.CustomType references an existing StrongType).
    /// The evaluator receives the target element + a model lookup context.
    /// </summary>
    Relational = 2,

    /// <summary>
    /// Rule over the entire document / all elements (e.g., all ClassElement have unique fully-qualified names).
    /// The evaluator receives the full document context.
    /// </summary>
    Global = 3
}
