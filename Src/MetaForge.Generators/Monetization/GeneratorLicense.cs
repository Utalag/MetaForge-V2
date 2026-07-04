namespace MetaForge.Generators.Monetization;

/// <summary>
/// Licenční model pro generování kódu.
/// Určuje tier, limity a oprávnění pro generování.
/// </summary>
public sealed record GeneratorLicense
{
    /// <summary>Aktuální tier licence.</summary>
    public GeneratorTier Tier { get; init; } = GeneratorTier.Sandbox;

    /// <summary>Licenční klíč (null = sandbox/domain).</summary>
    public string? LicenseKey { get; init; }

    /// <summary>Maximální počet entit ke generování (Sandbox = 3, ostatní = neomezeno).</summary>
    public int MaxEntities { get; init; } = int.MaxValue;

    /// <summary>Povolit export vygenerovaného kódu? (Sandbox = false).</summary>
    public bool AllowExport { get; init; } = true;

    /// <summary>Přidat vodoznak do generovaného kódu? (Sandbox = true).</summary>
    public bool AddWatermark { get; init; } = false;

    /// <summary>Mohou se generovat partial classes? (Domain+ = true).</summary>
    public bool AllowPartialClasses { get; init; } = true;

    /// <summary>Mohou se používat ForgeBlock Source Generatory? (Domain+ = true).</summary>
    public bool AllowSourceGenerators { get; init; } = false;

    /// <summary>
    /// Vytvoří výchozí licenci pro daný tier.
    /// </summary>
    public static GeneratorLicense Create(GeneratorTier tier, string? licenseKey = null) => tier switch
    {
        GeneratorTier.Sandbox => new GeneratorLicense
        {
            Tier = GeneratorTier.Sandbox,
            MaxEntities = 3,
            AllowExport = false,
            AddWatermark = true,
            AllowPartialClasses = false,
            AllowSourceGenerators = false,
        },
        GeneratorTier.Domain => new GeneratorLicense
        {
            Tier = GeneratorTier.Domain,
            AllowExport = true,
            AddWatermark = false,
            AllowPartialClasses = true,
            AllowSourceGenerators = true,
        },
        GeneratorTier.Infrastructure => new GeneratorLicense
        {
            Tier = GeneratorTier.Infrastructure,
            LicenseKey = licenseKey,
            AllowExport = true,
            AddWatermark = false,
            AllowPartialClasses = true,
            AllowSourceGenerators = true,
        },
        GeneratorTier.Full => new GeneratorLicense
        {
            Tier = GeneratorTier.Full,
            LicenseKey = licenseKey,
            AllowExport = true,
            AddWatermark = false,
            AllowPartialClasses = true,
            AllowSourceGenerators = true,
        },
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Neznámý tier."),
    };
}
