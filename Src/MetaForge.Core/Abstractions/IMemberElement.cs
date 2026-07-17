// ---------------------------------------------------------------------------
// MetaForge.Core — IMemberElement
// Common interface for all member-level elements (Method, Property, Event, Operator, etc.).
// Vrstva: Core / Abstractions
// 
// PROPOSAL: PROP-040 — Core Member Consistency
// ---------------------------------------------------------------------------

namespace MetaForge.Core.Abstractions;

/// <summary>
/// Common interface for type members — methods, properties, events, operators,
/// constructors, and fields. Enables polymorphic iteration and consistent
/// access to Name, Attributes, Metadata, XmlSummary, and Coin.
/// </summary>
public interface IMemberElement
{
    /// <summary>Stable identity for this member element. Used for cross-layer traceability (PROP-060).</summary>
    Guid Id { get; init; }

    /// <summary>Member name.</summary>
    string Name { get; }

    /// <summary>Attributes applied to this member (C# [Attribute]).</summary>
    List<AttributeElement> Attributes { get; }

    /// <summary>Key-value metadata annotations (documentation, validation, AI context).</summary>
    MetadataBag Metadata { get; }

    /// <summary>
    /// XML documentation summary for this member.
    /// Used for generating &lt;summary&gt; XML docs. Null means no summary.
    /// </summary>
    string? XmlSummary { get; set; }

    /// <summary>Credit cost of this member.</summary>
    int Coin { get; }
}
