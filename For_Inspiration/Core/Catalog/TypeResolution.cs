using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Catalog;

/// <summary>
/// Výsledek rozlišení typu z textového výrazu.
/// Buď jde o primitivní DataType, nebo o reference na katalogový StrongType preset.
/// </summary>
public sealed record TypeResolution
{
    /// <summary>Primitivní typ, pokud výraz odpovídá primitiv aliasu (string, int, decimal, ...).</summary>
    public DataType? Primitive { get; init; }

    /// <summary>ID katalogového presetu, pokud výraz odpovídá StrongType v katalogu.</summary>
    public string? CatalogId { get; init; }

    /// <summary>Je výsledek StrongType z katalogu?</summary>
    public bool IsStrongType => CatalogId is not null;

    /// <summary>Je výsledek primitivní typ?</summary>
    public bool IsPrimitive => Primitive.HasValue && CatalogId is null;

    /// <summary>Byl typ rozpoznán?</summary>
    public bool IsResolved => IsPrimitive || IsStrongType;

    /// <summary>Vytvoří rozlišení pro primitivní typ.</summary>
    public static TypeResolution FromPrimitive(DataType dataType) =>
        new() { Primitive = dataType };

    /// <summary>Vytvoří rozlišení pro katalogový preset.</summary>
    public static TypeResolution FromCatalog(string catalogId) =>
        new() { CatalogId = catalogId };

    /// <summary>Nerozpoznaný typ.</summary>
    public static TypeResolution Unresolved { get; } = new();
}
