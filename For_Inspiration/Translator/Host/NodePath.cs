using MetaForge.Core.Catalog;

namespace MetaForge.Translator;

/// <summary>
/// Adresace node v ramci business dokumentu pro node-level assist.
/// </summary>
public sealed class NodePath
{
    /// <summary>Nazev nebo ID entity, v niz se node nachazi.</summary>
    public string EntityNameOrId { get; init; } = string.Empty;

    /// <summary>Nazev nebo ID konkretniho node (attribute, behavior). Null = cela entita.</summary>
    public string? NodeNameOrId { get; init; }

    /// <summary>Druh node. Null = odvodit z kontextu (muze byt nejednoznacne).</summary>
    public NodeKind? Kind { get; init; }
}
