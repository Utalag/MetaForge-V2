using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Reprezentuje parametr metody.
/// </summary>
public sealed class ParameterElement
{
    /// <summary>Stable identity for cross-layer traceability (PROP-060).</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Název parametru.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Datový typ parametru.</summary>
    public TypeModel Type { get; set; } = TypeModel.Object;

    /// <summary>Má parametr výchozí hodnotu?</summary>
    public bool HasDefaultValue { get; set; }

    /// <summary>Výchozí hodnota jako string.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Modifikátor parametru (ref, out, in, params).</summary>
    public ParameterModifier Modifier { get; set; } = ParameterModifier.None;

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 1;
}

/// <summary>
/// Modifikátor parametru metody.
/// </summary>
public enum ParameterModifier
{
    None,
    Ref,
    Out,
    In,
    Params,
}
