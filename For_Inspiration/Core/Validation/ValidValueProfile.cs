namespace MetaForge.Core.Validation;

/// <summary>
/// Profil platných hodnot pro datový typ.
/// Obsahuje ukázkové C# literály které jsou vždy validní.
/// Používá se v kombinatorických testech (ClassTestProfile)
/// a jako pozitivní kontrola v izolovaných testech.
/// </summary>
public sealed record ValidValueProfile
{
    /// <summary>
    /// Prázdný profil bez vzorků.
    /// </summary>
    public static readonly ValidValueProfile Empty = new();

    /// <summary>
    /// Ukázkové platné C# literály.
    /// Např. pro DataType.Email: ["\"user@example.com\"", "\"a@b.cz\""]
    /// Doporučeno 2–3 hodnoty pro rozumný počet kombinací.
    /// </summary>
    public IReadOnlyList<string> Samples { get; init; } = [];
}
