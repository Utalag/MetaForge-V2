// ---------------------------------------------------------------------------
// MetaForge.Core — ConventionRegistry
// Global naming and structural conventions with per-element override support.
// Vrstva: Core / Composability
// 
// PROPOSAL: PROP-039 — Core Composability
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Transforms;

namespace MetaForge.Core.Composability;

/// <summary>
/// Scope of a convention — what kind of elements it applies to.
/// </summary>
public enum ConventionScope
{
    /// <summary>Applies to type declarations (Class, Struct, Interface, Enum).</summary>
    Type = 0,

    /// <summary>Applies to members (Method, Property, Parameter).</summary>
    Member = 1,

    /// <summary>Applies globally to the entire model.</summary>
    Global = 2
}

/// <summary>
/// A naming or structural convention that can be applied to Core elements.
/// Conventions are overrideable per-element via MetadataBag.
/// </summary>
public interface IConvention
{
    /// <summary>Human-readable name of the convention.</summary>
    string Name { get; }

    /// <summary>What scope this convention operates at.</summary>
    ConventionScope Scope { get; }

    /// <summary>Returns true if this convention should be applied to the given element.</summary>
    bool AppliesTo(RootElement element);

    /// <summary>Applies the convention to the element, returning the (possibly modified) element.</summary>
    RootElement Apply(RootElement element, ConventionContext context);
}

/// <summary>
/// Context provided when applying a convention.
/// </summary>
public sealed class ConventionContext
{
    /// <summary>Per-element override options stored in MetadataBag.</summary>
    public MetadataBag Options { get; init; } = new();

    /// <summary>Collector for diagnostics during convention application.</summary>
    public Diagnostics.IDiagnosticCollector Diagnostics { get; init; } = new Diagnostics.DiagnosticBag();
}

/// <summary>
/// Registry of naming/structural conventions.
/// Can be applied as an IModelTransform in the TransformPipeline.
/// </summary>
public sealed class ConventionRegistry
{
    private readonly List<IConvention> _conventions = new();

    /// <summary>Registers a single convention.</summary>
    public void Register(IConvention convention)
    {
        ArgumentNullException.ThrowIfNull(convention);
        _conventions.Add(convention);
    }

    /// <summary>Registers multiple conventions at once.</summary>
    public void RegisterRange(IEnumerable<IConvention> conventions)
    {
        ArgumentNullException.ThrowIfNull(conventions);
        _conventions.AddRange(conventions);
    }

    /// <summary>Returns all registered conventions (for enumeration).</summary>
    public IReadOnlyList<IConvention> Conventions => _conventions.AsReadOnly();

    /// <summary>
    /// Applies all registered conventions to a collection of RootElements.
    /// </summary>
    public IReadOnlyList<RootElement> ApplyTo(IReadOnlyList<RootElement> elements, ConventionContext? context = null)
    {
        var ctx = context ?? new ConventionContext();
        var results = new List<RootElement>(elements.Count);

        foreach (var element in elements)
        {
            var current = element;
            foreach (var convention in _conventions)
            {
                if (convention.AppliesTo(current))
                {
                    current = convention.Apply(current, ctx);
                }
            }
            results.Add(current);
        }

        return results;
    }
}

// ─────────────────────────────────────────────────────────
// Built-in conventions
// ─────────────────────────────────────────────────────────

/// <summary>
/// Ensures property names use PascalCase (first letter uppercase).
/// </summary>
public sealed class PascalCasePropertiesConvention : IConvention
{
    public string Name => "PascalCaseProperties";
    public ConventionScope Scope => ConventionScope.Member;

    public bool AppliesTo(RootElement element) => element is ClassElement or StructElement or InterfaceElement;

    public RootElement Apply(RootElement element, ConventionContext context)
    {
        // Apply PascalCase to all properties by convention.
        // For now, this is a validation pass — it doesn't rename, it diagnoses.
        if (element is ClassElement cls)
        {
            foreach (var prop in cls.Properties)
            {
                if (prop.Name.Length > 0 && !char.IsUpper(prop.Name[0]))
                {
                    context.Diagnostics.Report(new Diagnostics.Diagnostic(
                        "MF-CONV-001",
                        $"Property '{prop.Name}' should use PascalCase.",
                        Diagnostics.DiagnosticSeverity.Warning,
                        new Diagnostics.ElementPath(element.Name, prop.Name, null, null)));
                }
            }
        }
        return element;
    }
}

/// <summary>
/// Ensures interface names start with 'I' prefix.
/// </summary>
public sealed class InterfacePrefixConvention : IConvention
{
    public string Name => "InterfacePrefix";
    public ConventionScope Scope => ConventionScope.Type;

    public bool AppliesTo(RootElement element) => element is InterfaceElement;

    public RootElement Apply(RootElement element, ConventionContext context)
    {
        if (element is InterfaceElement iface && iface.Name.Length > 0 && !iface.Name.StartsWith("I"))
        {
            context.Diagnostics.Report(new Diagnostics.Diagnostic(
                "MF-CONV-002",
                $"Interface '{iface.Name}' should start with 'I' prefix.",
                Diagnostics.DiagnosticSeverity.Warning,
                new Diagnostics.ElementPath(iface.Name, iface.Name, null, null)));
        }
        return element;
    }
}

/// <summary>
/// Ensures async methods end with 'Async' suffix.
/// </summary>
public sealed class AsyncSuffixConvention : IConvention
{
    public string Name => "AsyncSuffix";
    public ConventionScope Scope => ConventionScope.Member;

    public bool AppliesTo(RootElement element) => element is ClassElement or StructElement or InterfaceElement;

    public RootElement Apply(RootElement element, ConventionContext context)
    {
        if (element is ClassElement cls)
        {
            foreach (var method in cls.Methods)
            {
                if (method.IsAsync && !method.Name.EndsWith("Async", StringComparison.Ordinal))
                {
                    context.Diagnostics.Report(new Diagnostics.Diagnostic(
                        "MF-CONV-003",
                        $"Async method '{method.Name}' should end with 'Async' suffix.",
                        Diagnostics.DiagnosticSeverity.Warning,
                        new Diagnostics.ElementPath(cls.Name, method.Name, null, null)));
                }
            }
        }
        return element;
    }
}
