// ---------------------------------------------------------------------------
// MetaForge.Core — EventElement
// Represents a C# event declaration.
// Vrstva: Core / Elements / Members
// 
// PROPOSAL: PROP-037 — C# Completeness
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Represents a C# event declaration.
/// Example: public event EventHandler&lt;EventArgs&gt; MyEvent;
/// </summary>
public sealed class EventElement : IMemberElement
{
    /// <summary>Stable identity for cross-layer traceability (PROP-060).</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Event name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The delegate type of the event (e.g., EventHandler, Action&lt;T&gt;).</summary>
    public DataTypes.TypeModel EventType { get; set; } = DataTypes.TypeModel.Of(DataTypes.DataType.Entity).WithCustomName("EventHandler");

    /// <summary>Whether the event is static.</summary>
    public bool IsStatic { get; set; }

    /// <summary>Access modifier for the event.</summary>
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    /// <summary>Optional custom add accessor visibility (for explicit event accessors).</summary>
    public AccessModifier? AddAccessor { get; set; }

    /// <summary>Optional custom remove accessor visibility (for explicit event accessors).</summary>
    public AccessModifier? RemoveAccessor { get; set; }

    /// <summary>Attributes applied to the event.</summary>
    public List<AttributeElement> Attributes { get; init; } = new();

    /// <summary>XML documentation summary for this event.</summary>
    public string? XmlSummary { get; set; }

    /// <summary>Metadata annotations.</summary>
    public MetadataBag Metadata { get; init; } = new();

    /// <summary>Coin cost.</summary>
    public int Coin { get; set; } = 2;

    /// <summary>Creates a basic public event.</summary>
    public static EventElement Basic(string name, DataTypes.TypeModel eventType) => new()
    {
        Name = name,
        EventType = eventType
    };

    /// <summary>Creates a static event.</summary>
    public static EventElement Static(string name, DataTypes.TypeModel eventType) => new()
    {
        Name = name,
        EventType = eventType,
        IsStatic = true
    };

    // Fluent extensions
    public EventElement WithAccess(AccessModifier access) { AccessModifier = access; return this; }
    public EventElement WithAddRemove(AccessModifier add, AccessModifier remove) { AddAccessor = add; RemoveAccessor = remove; return this; }
}
