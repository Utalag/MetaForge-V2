using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Validation;

/// <summary>
/// Testovací profil pro konkrétní DataType.
/// Kombinuje ValidValueProfile (pozitivní testy) a InvalidValueProfile (negativní testy).
/// Default profily jsou registrovány v DataTypeTestRegistry.
/// Uživatel může přepsat per-field pomocí with {}.
/// </summary>
public sealed record DataTypeTestProfile
{
    /// <summary>
    /// Datový typ ke kterému profil patří.
    /// </summary>
    public DataType DataType { get; init; }

    /// <summary>
    /// Profil platných hodnot (pozitivní testy).
    /// </summary>
    public ValidValueProfile Valid { get; init; } = ValidValueProfile.Empty;

    /// <summary>
    /// Profil neplatných hodnot (negativní testy).
    /// </summary>
    public InvalidValueProfile Invalid { get; init; } = InvalidValueProfile.Empty;
}
