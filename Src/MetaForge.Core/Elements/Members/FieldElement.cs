// ---------------------------------------------------------------------------
// MetaForge.Core — FieldElement
// Represents a C# field declaration.
// Vrstva: Core / Elements / Members
// 
// PROPOSAL: PROP-041 — ConstructorElement + FieldElement
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Represents a C# field.
/// Example: private readonly ILogger _logger;
/// </summary>
public sealed class FieldElement : IMemberElement
{
    /// <summary>Field name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Field type.</summary>
    public DataTypes.TypeModel Type { get; set; } = DataTypes.TypeModel.Object;

    /// <summary>Access modifier (default: Private for fields).</summary>
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Private;

    /// <summary>Is the field readonly?</summary>
    public bool IsReadOnly { get; set; }

    /// <summary>Is the field static?</summary>
    public bool IsStatic { get; set; }

    /// <summary>Is this a const field? (const fields are implicitly static)</summary>
    public bool IsConst { get; set; }

    /// <summary>
    /// Default/initializer value as string (e.g., "0", "null", "\"hello\"", "new List<int>()").
    /// Null = no initializer.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>Attributes applied to the field.</summary>
    public List<AttributeElement> Attributes { get; init; } = new();

    /// <summary>XML documentation summary for this field.</summary>
    public string? XmlSummary { get; set; }

    /// <summary>Metadata annotations.</summary>
    public MetadataBag Metadata { get; init; } = new();

    /// <summary>Coin cost.</summary>
    public int Coin { get; set; } = 1;

    // === Factory methods ===

    /// <summary>Creates a basic private field.</summary>
    public static FieldElement Basic(string name, DataTypes.TypeModel type) => new()
    {
        Name = name,
        Type = type
    };

    /// <summary>Creates a private readonly field (typical DI pattern).</summary>
    public static FieldElement ReadOnly(string name, DataTypes.TypeModel type) => new()
    {
        Name = name,
        Type = type,
        IsReadOnly = true
    };

    /// <summary>Creates a private static readonly field.</summary>
    public static FieldElement StaticReadOnly(string name, DataTypes.TypeModel type) => new()
    {
        Name = name,
        Type = type,
        IsReadOnly = true,
        IsStatic = true
    };

    /// <summary>Creates a const field.</summary>
    public static FieldElement Const(string name, DataTypes.TypeModel type, string value) => new()
    {
        Name = name,
        Type = type,
        IsConst = true,
        IsStatic = true,  // const je implicitně static
        DefaultValue = value
    };

    // === Fluent extensions ===

    public FieldElement WithAccess(AccessModifier access) { AccessModifier = access; return this; }
    public FieldElement WithDefault(string defaultValue) { DefaultValue = defaultValue; return this; }
}
