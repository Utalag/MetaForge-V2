namespace MetaForge.Generators.Monetization;

/// <summary>
/// Výjimka vyhozená při pokusu o generování kódu nad rámec licence.
/// </summary>
public sealed class LicenseException : Exception
{
    /// <summary>Požadovaný tier, který nebyl splněn.</summary>
    public GeneratorTier RequiredTier { get; }

    /// <summary>Aktuální tier licence.</summary>
    public GeneratorTier CurrentTier { get; }

    /// <summary>
    /// Vytvoří licenční výjimku.
    /// </summary>
    public LicenseException(string message, GeneratorTier required, GeneratorTier current)
        : base(message)
    {
        RequiredTier = required;
        CurrentTier = current;
    }

    /// <summary>
    /// Vytvoří licenční výjimku s výchozí zprávou.
    /// </summary>
    public static LicenseException TierTooLow(GeneratorTier required, GeneratorTier current)
        => new(
            $"Tato operace vyžaduje tier '{required}' nebo vyšší. Aktuální tier: '{current}'.",
            required,
            current);

    /// <summary>
    /// Vytvoří licenční výjimku pro překročení limitu entit.
    /// </summary>
    public static LicenseException EntityLimitExceeded(int maxEntities, int actual)
        => new(
            $"Překročen limit entit: maximum {maxEntities}, požadováno {actual}. Upgradujte licenci.",
            GeneratorTier.Domain,
            GeneratorTier.Sandbox);
}
