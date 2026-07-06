using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje C# enum.
/// </summary>
public sealed class EnumElement : RootElement
{
    public override string Kind => "enum";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    /// <summary>Podkladový typ enumu (výchozí Int32).</summary>
    public DataType UnderlyingType { get; set; } = DataType.Int32;

    /// <summary>Má atribut [Flags]?</summary>
    public bool IsFlags { get; set; }

    /// <summary>Členové enumu.</summary>
    public List<EnumMemberElement> Members { get; } = new();

    public override int TotalCoin => Coin + Members.Sum(m => m.Coin);

    // === Statické factory metody (E1-E4 matice) ===

    /// <summary>E1: public enum Status (Int32).</summary>
    public static EnumElement Basic(string name) => new() { Name = name };

    /// <summary>E2: public enum Status : byte.</summary>
    public static EnumElement ByteEnum(string name) => new()
    {
        Name = name,
        UnderlyingType = DataType.Byte,
    };

    /// <summary>E3: public enum Status : long.</summary>
    public static EnumElement Int64Enum(string name) => new()
    {
        Name = name,
        UnderlyingType = DataType.Int64,
    };

    /// <summary>E4: [Flags] public enum Status (Int32).</summary>
    public static EnumElement Flags(string name) => new()
    {
        Name = name,
        IsFlags = true,
    };

    /// <summary>E4 varianta: [Flags] s vlastním underlying typem.</summary>
    public static EnumElement Flags(string name, DataType underlyingType) => new()
    {
        Name = name,
        UnderlyingType = underlyingType,
        IsFlags = true,
    };

    // === Fluent rozšiřovací metody ===

    /// <summary>Nastaví access modifier.</summary>
    public EnumElement WithAccess(AccessModifier access)
    {
        AccessModifier = access;
        return this;
    }

    /// <summary>Přidá člena enumu.</summary>
    public EnumElement WithMember(EnumMemberElement member)
    {
        Members.Add(member);
        return this;
    }

    /// <summary>Přidá členy enumu.</summary>
    public EnumElement WithMembers(params EnumMemberElement[] members)
    {
        Members.AddRange(members);
        return this;
    }
}
