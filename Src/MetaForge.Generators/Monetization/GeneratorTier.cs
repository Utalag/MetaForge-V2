namespace MetaForge.Generators.Monetization;

/// <summary>
/// Tier model pro generování kódu.
/// Určuje, jaké vrstvy kódu může uživatel generovat.
/// </summary>
public enum GeneratorTier
{
    /// <summary>TIER 0 — Náhled v sandboxu, bez možnosti exportu. Omezený počet entit.</summary>
    Sandbox = 0,

    /// <summary>TIER 1 — Domain vrstva: entity, value objects, rozhraní. Exportovatelné.</summary>
    Domain = 1,

    /// <summary>TIER 2 — Infrastructure: repository, services, API controllery, mapping.</summary>
    Infrastructure = 2,

    /// <summary>TIER 3 — Full: CI/CD, Docker, deployment, monitoring, custom ForgeBlock SDK.</summary>
    Full = 3,
}
