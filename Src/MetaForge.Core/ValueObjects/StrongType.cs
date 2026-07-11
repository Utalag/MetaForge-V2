using MetaForge.Core.DataTypes;

namespace MetaForge.Core.ValueObjects;

/// <summary>
/// Silně typovaná doménová hodnota — obaluje TypeModel s pojmenovaným typem.
/// Např. "Email" → TypeModel.String s validačními pravidly.
/// </summary>
/// <remarks>
/// StrongType je čistá metareprezentace — neobsahuje konverzní flags.
/// Konverze (VogenConversions) se nastavují na <see cref="Elements.Types.ValueObjectElement"/>
/// až při výběru cílové infrastruktury.
/// </remarks>
public sealed record StrongType(
    string Name,
    TypeModel Underlying,
    IReadOnlyList<ValueObjectValidationRule>? ValidationRules = null
);
