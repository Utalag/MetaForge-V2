namespace MetaForge.Core.Abstractions;

/// <summary>
/// Reprezentuje C# atribut — název a argumenty.
/// </summary>
public sealed class AttributeElement
{
    /// <summary>Název atributu (např. "Obsolete", "JsonProperty").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Argumenty atributu — mohou být null pro bezparametrové atributy.</summary>
    public List<object?> Arguments { get; } = new();
}
