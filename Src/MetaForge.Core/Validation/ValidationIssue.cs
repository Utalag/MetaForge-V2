namespace MetaForge.Core.Validation;

/// <summary>
/// Validační problém — reprezentuje jednu nalezenou nevalidní kombinaci.
/// </summary>
/// <param name="Code">Kód z matice (např. "C9", "M11").</param>
/// <param name="Category">Kategorie (např. "ConflictingModifiers", "InvalidType").</param>
/// <param name="Message">Lidsky čitelná zpráva.</param>
public sealed record ValidationIssue(
    string Code,
    string Category,
    string Message
)
{
    /// <inheritdoc />
    public override string ToString() => $"[{Code}] {Category}: {Message}";
}

/// <summary>
/// Kategorie validačních problémů — odpovídají sekcím matice.
/// </summary>
public static class ValidationCategories
{
    /// <summary>Konfliktní modifikátory — např. abstract + sealed.</summary>
    public const string ConflictingModifiers = nameof(ConflictingModifiers);

    /// <summary>Nevalidní access modifier pro daný kontext.</summary>
    public const string InvalidAccess = nameof(InvalidAccess);

    /// <summary>Neplatný typ (např. void jako property type).</summary>
    public const string InvalidType = nameof(InvalidType);

    /// <summary>Neplatná dědičnost (např. sealed base class).</summary>
    public const string InvalidInheritance = nameof(InvalidInheritance);

    /// <summary>Chybějící povinná vlastnost (např. property bez getteru).</summary>
    public const string MissingRequired = nameof(MissingRequired);

    /// <summary>Typová chyba ve statementu (např. if s ne-bool podmínkou).</summary>
    public const string StatementTypeError = nameof(StatementTypeError);

    /// <summary>Varování — technicky validní, ale podezřelé.</summary>
    public const string Warning = nameof(Warning);
}
