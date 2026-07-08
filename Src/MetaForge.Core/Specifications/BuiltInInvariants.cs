// ---------------------------------------------------------------------------
// MetaForge.Core — BuiltInInvariants
// Standard invariant definitions for Core element types.
// Vrstva: Core / Specifications
// 
// PROPOSAL: PROP-036 — Core Specification Layer
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using static MetaForge.Core.Specifications.InvariantExpressionBuilder;

namespace MetaForge.Core.Specifications;

/// <summary>
/// Registry of standard built-in invariants for Core element types.
/// These invariants encode the validity rules that were previously implicit in code.
/// </summary>
public static class BuiltInInvariants
{
    // ──────────────────────────────────────────────
    // MethodElement invariants
    // ──────────────────────────────────────────────

    /// <summary>Abstract methods must not have a body.</summary>
    public static readonly InvariantDefinition Method_AbstractCannotHaveBody = InvariantDefinition.WhenCondition(
        code: "MF_METHOD_001",
        targetKind: "MethodElement",
        description: "Abstract methods must not have a body.",
        when: Prop("IsAbstract").Eq(true),
        must: new NotExpression(new ExistsExpression("Body")),
        severity: InvariantSeverity.Error);

    /// <summary>Abstract methods must not be static.</summary>
    public static readonly InvariantDefinition Method_AbstractCannotBeStatic = InvariantDefinition.WhenCondition(
        code: "MF_METHOD_002",
        targetKind: "MethodElement",
        description: "Abstract methods cannot be static.",
        when: Prop("IsAbstract").Eq(true),
        must: Prop("IsStatic").Eq(false),
        severity: InvariantSeverity.Error);

    /// <summary>Abstract methods must not be private.</summary>
    public static readonly InvariantDefinition Method_AbstractCannotBePrivate = InvariantDefinition.WhenCondition(
        code: "MF_METHOD_003",
        targetKind: "MethodElement",
        description: "Abstract methods cannot be private.",
        when: Prop("IsAbstract").Eq(true),
        must: new NotExpression(Prop("AccessModifier").Eq((int)Abstractions.AccessModifier.Private)),
        severity: InvariantSeverity.Error);

    /// <summary>Async methods must return Task, Task&lt;T&gt;, or ValueTask.</summary>
    public static readonly InvariantDefinition Method_AsyncMustReturnTask = InvariantDefinition.WhenCondition(
        code: "MF_METHOD_004",
        targetKind: "MethodElement",
        description: "Async methods should return a task-like type.",
        when: Prop("IsAsync").Eq(true),
        must: Const(true), // Placeholder — requires TypeModel inspection (Relational scope)
        severity: InvariantSeverity.Warning,
        scope: InvariantScope.Relational);

    /// <summary>Virtual methods cannot be static.</summary>
    public static readonly InvariantDefinition Method_VirtualCannotBeStatic = InvariantDefinition.WhenCondition(
        code: "MF_METHOD_005",
        targetKind: "MethodElement",
        description: "Virtual methods cannot be static.",
        when: Prop("IsVirtual").Eq(true),
        must: Prop("IsStatic").Eq(false),
        severity: InvariantSeverity.Error);

    /// <summary>Override methods must have a corresponding virtual/abstract/override in the base.</summary>
    public static readonly InvariantDefinition Method_OverrideRequiresBase = InvariantDefinition.WhenCondition(
        code: "MF_METHOD_006",
        targetKind: "MethodElement",
        description: "Override methods require a base virtual/abstract/override method.",
        when: Prop("IsOverride").Eq(true),
        must: Const(true), // Placeholder — requires base class inspection (Relational scope)
        severity: InvariantSeverity.Warning,
        scope: InvariantScope.Relational);

    // ──────────────────────────────────────────────
    // ClassElement invariants
    // ──────────────────────────────────────────────

    /// <summary>Abstract and sealed modifiers are mutually exclusive.</summary>
    public static readonly InvariantDefinition Class_AbstractSealedConflict = InvariantDefinition.WhenCondition(
        code: "MF_CLASS_001",
        targetKind: "ClassElement",
        description: "A class cannot be both abstract and sealed.",
        when: Prop("IsAbstract").Eq(true),
        must: Prop("IsSealed").Eq(false),
        severity: InvariantSeverity.Error);

    /// <summary>Static and abstract modifiers are mutually exclusive.</summary>
    public static readonly InvariantDefinition Class_StaticAbstractConflict = InvariantDefinition.WhenCondition(
        code: "MF_CLASS_002",
        targetKind: "ClassElement",
        description: "A class cannot be both static and abstract.",
        when: Prop("IsStatic").Eq(true),
        must: Prop("IsAbstract").Eq(false),
        severity: InvariantSeverity.Error);

    /// <summary>Static and sealed modifiers are mutually exclusive (static classes are implicitly sealed).</summary>
    public static readonly InvariantDefinition Class_StaticSealedConflict = InvariantDefinition.WhenCondition(
        code: "MF_CLASS_003",
        targetKind: "ClassElement",
        description: "A static class is implicitly sealed — explicit sealed is redundant.",
        when: Prop("IsStatic").Eq(true),
        must: Prop("IsSealed").Eq(false),
        severity: InvariantSeverity.Warning);

    /// <summary>A class must have a name.</summary>
    public static readonly InvariantDefinition Class_MustHaveName = InvariantDefinition.Always(
        code: "MF_CLASS_004",
        targetKind: "ClassElement",
        description: "A class must have a non-empty name.",
        must: new NotExpression(new EqExpression(new PropertyRef("Name"), new ConstantExpression(""))),
        severity: InvariantSeverity.Error);

    // ──────────────────────────────────────────────
    // PropertyElement invariants
    // ──────────────────────────────────────────────

    /// <summary>Static properties cannot be required.</summary>
    public static readonly InvariantDefinition Property_StaticCannotBeRequired = InvariantDefinition.WhenCondition(
        code: "MF_PROP_001",
        targetKind: "PropertyElement",
        description: "Static properties cannot be required.",
        when: Prop("IsStatic").Eq(true),
        must: Prop("IsRequired").Eq(false),
        severity: InvariantSeverity.Error);

    /// <summary>Required properties must have a setter or init-only accessor.</summary>
    public static readonly InvariantDefinition Property_RequiredNeedsSetter = InvariantDefinition.WhenCondition(
        code: "MF_PROP_002",
        targetKind: "PropertyElement",
        description: "Required properties must have a setter or init-only accessor.",
        when: Prop("IsRequired").Eq(true),
        must: new OrExpression(Prop("HasSetter").Eq(true), Prop("IsInitOnly").Eq(true)),
        severity: InvariantSeverity.Error);

    // ──────────────────────────────────────────────
    // All built-in invariants
    // ──────────────────────────────────────────────

    /// <summary>All built-in invariants as a flat list.</summary>
    public static readonly IReadOnlyList<InvariantDefinition> All = new List<InvariantDefinition>
    {
        Method_AbstractCannotHaveBody,
        Method_AbstractCannotBeStatic,
        Method_AbstractCannotBePrivate,
        Method_VirtualCannotBeStatic,
        Class_AbstractSealedConflict,
        Class_StaticAbstractConflict,
        Class_StaticSealedConflict,
        Class_MustHaveName,
        Property_StaticCannotBeRequired,
        Property_RequiredNeedsSetter,
    }.AsReadOnly();

    /// <summary>
    /// Pending invariants that require additional context (Relational/Global scope)
    /// and are not yet fully evaluable. These are here for documentation and future implementation.
    /// </summary>
    public static readonly IReadOnlyList<InvariantDefinition> Pending = new List<InvariantDefinition>
    {
        Method_AsyncMustReturnTask,
        Method_OverrideRequiresBase,
    }.AsReadOnly();
}
