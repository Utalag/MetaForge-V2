using MetaForge.Core.ValueObjects;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje Vogen value object pro codegen.
/// Dědí z <see cref="StructElement"/> a přidává Vogen-specific properties.
/// </summary>
/// <remarks>
/// Conversions se defaultně nastavuje na <see cref="VogenConversions.None"/> —
/// žádné konvertory, dokud si uživatel nevybere cílovou infrastrukturu
/// (EF Core, Dapper, JSON, BSON, Orleans...).
/// Infrastructure ForgeBlock pak nastaví odpovídající <see cref="VogenConversions"/> flags.
/// </remarks>
public sealed class ValueObjectElement : StructElement
{
    public override string Kind => "value-object";

    // === Vogen-specific properties ===

    /// <summary>
    /// Vogen <c>Conversions</c> flags — jaké konvertory má Vogen source-generovat.
    /// Default: <see cref="VogenConversions.None"/> (žádné konvertory).
    /// Nastavuje se až při výběru infrastruktury.
    /// </summary>
    public VogenConversions Conversions { get; set; } = VogenConversions.None;

    /// <summary>
    /// Typ výjimky, kterou Vogen vyhodí při nevalidní hodnotě.
    /// Mapuje na <c>[ValueObject(throws: typeof(...))]</c>.
    /// null = Vogen default (<c>ValueObjectValidationException</c>).
    /// </summary>
    public string? ThrowsExceptionType { get; set; }

    public override int TotalCoin =>
        Coin + base.TotalCoin;

    // === Factory ===

    /// <summary>Vytvoří ValueObjectElement z StrongType metadat.</summary>
    public static ValueObjectElement FromStrongType(StrongType strongType)
    {
        return new ValueObjectElement
        {
            Name = strongType.Name,
            IsReadOnly = true,
        };
    }

    // === Fluent ===

    /// <summary>Nastaví Vogen Conversions flags.</summary>
    public ValueObjectElement WithConversions(VogenConversions conversions)
    {
        Conversions = conversions;
        return this;
    }

    /// <summary>Nastaví výjimkový typ pro validaci.</summary>
    public ValueObjectElement WithThrows(string throwsType)
    {
        ThrowsExceptionType = throwsType;
        return this;
    }
}
