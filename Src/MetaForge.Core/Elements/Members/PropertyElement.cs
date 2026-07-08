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

    /// <summary>
    /// Univerzální key-value anotace (dokumentace, validace, generátorové hinty).
    /// </summary>
    public MetadataBag Metadata { get; init; } = new();

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 2;

    // === Statické factory metody (P1-P5, P8 matice) ===

    /// <summary>P1: public string Name { get; set; }</summary>
    public static PropertyElement GetSet(string name, TypeModel type) => new()
    {
        Name = name,
        Type = type,
        HasGetter = true,
        HasSetter = true,
    };

    /// <summary>P2: public string Name { get; }</summary>
    public static PropertyElement GetOnly(string name, TypeModel type) => new()
    {
        Name = name,
        Type = type,
        HasGetter = true,
        HasSetter = false,
    };

    /// <summary>P3: public string Name { get; init; }</summary>
    public static PropertyElement InitOnly(string name, TypeModel type) => new()
    {
        Name = name,
        Type = type,
        HasGetter = true,
        HasSetter = true,
        IsInitOnly = true,
    };

    /// <summary>P4: public required string Name { get; set; }</summary>
    public static PropertyElement Required(string name, TypeModel type) => new()
    {
        Name = name,
        Type = type,
        HasGetter = true,
        HasSetter = true,
        IsRequired = true,
    };

    /// <summary>P5: public static string Name { get; set; }</summary>
    public static PropertyElement Static(string name, TypeModel type) => new()
    {
        Name = name,
        Type = type,
        HasGetter = true,
        HasSetter = true,
        IsStatic = true,
    };

    /// <summary>P8: public required string Name { get; }</summary>
    public static PropertyElement RequiredGetOnly(string name, TypeModel type) => new()
    {
        Name = name,
        Type = type,
        HasGetter = true,
        HasSetter = false,
        IsRequired = true,
    };

    // === Fluent rozšiřovací metody ===

    /// <summary>Nastaví access modifier.</summary>
    public PropertyElement WithAccess(AccessModifier access)
    {
        AccessModifier = access;
        return this;
    }

    /// <summary>Nastaví výchozí hodnotu.</summary>
    public PropertyElement WithDefault(string defaultValue)
    {
        DefaultValue = defaultValue;
        return this;
    }

    /// <summary>Nastaví cenu v kreditech.</summary>
    public PropertyElement WithCoin(int coin)
    {
        Coin = coin;
        return this;
    }
}
