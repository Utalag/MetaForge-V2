using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Jeden člen enumu — název a volitelná hodnota.
/// </summary>
public sealed class EnumMemberElement
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Explicitní hodnota (null = automatická).</summary>
    public object? Value { get; set; }

    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 1;
}
