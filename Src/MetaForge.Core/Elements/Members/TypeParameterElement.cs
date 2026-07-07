namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Generický typový parametr deklarace — např. `T` v `class Repository&lt;T&gt; where T : class`.
/// </summary>
public sealed class TypeParameterElement
{
    /// <summary>Název typového parametru (např. "T", "TKey", "TValue").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Omezení (constraints) tohoto typového parametru.</summary>
    public List<GenericConstraint> Constraints { get; } = new();

    /// <summary>Varianta parametru (in/out) — relevantní jen pro interfacy a delegáty.</summary>
    public GenericVariance Variance { get; set; } = GenericVariance.None;

    /// <summary>Vytvoří typový parametr bez omezení.</summary>
    public static TypeParameterElement Of(string name) => new() { Name = name };

    /// <summary>Přidá omezení a vrátí this pro fluent zápis.</summary>
    public TypeParameterElement WithConstraint(GenericConstraint constraint)
    {
        Constraints.Add(constraint);
        return this;
    }
}

/// <summary>Variance generického typového parametru.</summary>
public enum GenericVariance
{
    /// <summary>Invariantní (výchozí).</summary>
    None,

    /// <summary>Kovariantní — `out T`.</summary>
    Out,

    /// <summary>Kontravariantní — `in T`.</summary>
    In,
}
