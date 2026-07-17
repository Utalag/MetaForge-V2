// ---------------------------------------------------------------------------
// MetaForge.Core — ConstructorElement
// Represents a C# constructor declaration.
// Vrstva: Core / Elements / Members
// 
// PROPOSAL: PROP-041 — ConstructorElement + FieldElement
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Statements;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Represents a C# constructor.
/// Example: public Foo(int x, string y) : this(x) { _y = y; }
/// </summary>
public sealed class ConstructorElement : IMemberElement
{
    /// <summary>Stable identity for cross-layer traceability (PROP-060).</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Constructor name (typically matches the class name).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Constructor parameters.</summary>
    public List<ParameterElement> Parameters { get; init; } = new();

    /// <summary>Access modifier (public, private, protected, internal).</summary>
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    /// <summary>Constructor body as AST.</summary>
    public BlockStatement? Body { get; set; }

    /// <summary>
    /// Constructor initializer: "this(x, 0)" or "base(name)".
    /// Null means no explicit initializer (calls base() implicitly).
    /// </summary>
    public string? Initializer { get; set; }

    /// <summary>Is this a static constructor? (static Foo() { })</summary>
    public bool IsStatic { get; set; }

    /// <summary>Attributes applied to the constructor.</summary>
    public List<AttributeElement> Attributes { get; init; } = new();

    /// <summary>XML documentation summary for this constructor.</summary>
    public string? XmlSummary { get; set; }

    /// <summary>Metadata annotations.</summary>
    public MetadataBag Metadata { get; init; } = new();

    /// <summary>Coin cost.</summary>
    public int Coin { get; set; } = 3;

    /// <summary>Total coin including parameters.</summary>
    public int TotalCoin => Coin + Parameters.Sum(p => p.Coin);

    // === Factory methods ===

    /// <summary>Creates a basic public constructor with optional parameters.</summary>
    public static ConstructorElement Basic(string name, params ParameterElement[] parameters) => new()
    {
        Name = name,
        Parameters = parameters.ToList()
    };

    /// <summary>Creates a private constructor (singleton pattern, etc.).</summary>
    public static ConstructorElement Private(string name, params ParameterElement[] parameters) => new()
    {
        Name = name,
        Parameters = parameters.ToList(),
        AccessModifier = AccessModifier.Private
    };

    /// <summary>Creates a static constructor.</summary>
    public static ConstructorElement Static(string name) => new()
    {
        Name = name,
        IsStatic = true
    };

    // === Fluent extensions ===

    public ConstructorElement WithAccess(AccessModifier access) { AccessModifier = access; return this; }
    public ConstructorElement WithParameter(ParameterElement param) { Parameters.Add(param); return this; }
    public ConstructorElement WithParameters(params ParameterElement[] parameters) { Parameters.AddRange(parameters); return this; }
    public ConstructorElement WithInitializer(string initializer) { Initializer = initializer; return this; }
    public ConstructorElement WithBody(BlockStatement body) { Body = body; return this; }
}
