using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Catalog;

/// <summary>
/// Předdefinovaný typ — mapuje název na TypeModel.
/// Např. "Email" → TypeModel.String, "Price" → TypeModel.Decimal.
/// </summary>
public sealed record PresetDefinition(
    string Name,
    TypeModel Type,
    string? Description = null,
    IReadOnlyList<string>? Tags = null
);
