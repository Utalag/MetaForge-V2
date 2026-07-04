using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Reprezentuje C# property (vlastnost) na třídě, interfacu nebo structu.
/// </summary>
public sealed class PropertyElement
{
    /// <summary>Název property.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Datový typ property.</summary>
    public TypeModel Type { get; set; } = TypeModel.Object;

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool HasGetter { get; set; } = true;
    public bool HasSetter { get; set; } = true;
    public bool IsInitOnly { get; set; }
    public bool IsRequired { get; set; }
    public bool IsStatic { get; set; }

    /// <summary>Výchozí hodnota jako string (např. "0", "null", "\"hello\"").</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 2;
}
