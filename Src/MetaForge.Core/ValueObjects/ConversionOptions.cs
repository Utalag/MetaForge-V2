namespace MetaForge.Core.ValueObjects;

/// <summary>
/// Možnosti konverze pro StrongType (např. implicitní/explicitní operátory).
/// </summary>
public sealed record ConversionOptions(
    bool GenerateImplicitConversion = false,
    bool GenerateExplicitConversion = false,
    bool GenerateToString = true,
    bool GenerateEquals = true,
    bool GenerateGetHashCode = true
);
