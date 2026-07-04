using MetaForge.Core.Abstractions;
using MetaForge.Generators.Monetization;

namespace MetaForge.Generators;

/// <summary>
/// Tier-aware generátor kódu — kontroluje licenci před generováním.
/// Dědí z CodeGenerator a přidává licenční omezení.
/// </summary>
public class TieredCodeGenerator : CodeGenerator
{
    private readonly GeneratorLicense _license;

    /// <summary>Vodoznak přidávaný do kódu v Sandbox tieru.</summary>
    private const string Watermark = """
        // ============================================
        // GENEROVÁNO POMOCÍ MetaForge (SANDSBOX TIER)
        // Tento soubor nelze použít v produkci.
        // Upgradujte na DOMAIN tier pro export.
        // ============================================

        """;

    /// <summary>
    /// Vytvoří tier-aware generátor.
    /// </summary>
    public TieredCodeGenerator(GeneratorLicense license)
    {
        _license = license;
    }

    /// <inheritdoc />
    public override GeneratedCodeArtifact Generate(RootElement element)
    {
        // Kontrola limitu entit (pro Sandbox)
        if (_license.Tier == GeneratorTier.Sandbox && _license.MaxEntities > 0)
        {
            // Limit check je prováděn v IncrementalCodeGenerator
        }

        var artifact = base.Generate(element);

        // Přidat vodoznak pro Sandbox tier
        if (_license.AddWatermark && artifact.SourceCode.Length > 0)
        {
            return artifact with { SourceCode = Watermark + artifact.SourceCode };
        }

        return artifact;
    }
}
