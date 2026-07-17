// ---------------------------------------------------------------------------
// MetaForge.Core — ReferenceKind
// Classification of dependency types between elements.
// Vrstva: Core / ReferenceGraph
//
// PROPOSAL: PROP-055 — ReferenceGraph
// ---------------------------------------------------------------------------

namespace MetaForge.Core.ReferenceGraph;

/// <summary>
/// Druh typové reference mezi elementy.
/// Používá se pro klasifikaci hran v <see cref="ReferenceGraph"/>.
/// </summary>
public enum ReferenceKind
{
    /// <summary>BaseClassName — dědičnost.</summary>
    Inheritance,

    /// <summary>ImplementedInterfaces — implementace interfacu.</summary>
    InterfaceImplementation,

    /// <summary>PropertyElement.Type.CustomTypeName.</summary>
    PropertyType,

    /// <summary>MethodElement.ReturnType.CustomTypeName.</summary>
    MethodReturn,

    /// <summary>FieldElement.Type.CustomTypeName.</summary>
    FieldType,

    /// <summary>GenericConstraint.BaseTypeName.</summary>
    GenericConstraint,

    /// <summary>NewExpression.TypeName.</summary>
    NewExpression,

    /// <summary>ParameterElement.Type.CustomTypeName.</summary>
    ParameterType,
}
