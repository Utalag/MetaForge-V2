using MetaForge.Core.DataTypes;

namespace MetaForge.Core.ValueObjects;

/// <summary>
/// Silně typovaná doménová hodnota — obaluje TypeModel s pojmenovaným typem.
/// Např. "Email" → TypeModel.String s validačními pravidly.
/// </summary>
public sealed record StrongType(
    string Name,
    TypeModel Underlying,
    IReadOnlyList<ValueObjectValidationRule>? ValidationRules = null,
    ConversionOptions? Conversion = null
);
