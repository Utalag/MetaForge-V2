using MetaForge.Core.Abstractions;
using MetaForge.Generators.Monetization;

namespace MetaForge.Generators;

/// <summary>
/// Tier-aware generátor kódu — kontroluje licenci před generováním.
/// [TieredCodeGenerator je Legacy — cíl je IGenerationCostPolicy ve Facade dle IDEA-026]
/// Používá kompozici místo dědičnosti: wrapuje CodeGenerator a přidává licenční omezení.
/// </summary>
public sealed class TieredCodeGenerator
{
    private readonly CodeGenerator _inner;
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
        _inner = new CodeGenerator();
        _license = license;
    }

    /// <summary>
    /// Vygeneruje kód s ohledem na licenci.
    /// </summary>
    public GeneratedCodeArtifact Generate(RootElement element)
    {
        var artifact = _inner.Generate(element);

        // Přidat vodoznak pro Sandbox tier
        if (_license.AddWatermark && artifact.SourceCode.Length > 0)
        {
            return artifact with { SourceCode = Watermark + artifact.SourceCode };
        }

        return artifact;
    }
}
