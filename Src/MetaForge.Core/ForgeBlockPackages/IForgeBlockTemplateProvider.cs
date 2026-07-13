namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// ForgeBlock, který poskytuje vlastní Scriban šablony pro generování kódu.
/// Při registraci balíku se šablony automaticky zaregistrují do TemplateManageru.
/// Plugin pattern: každý ForgeBlock si nese vlastní šablony a logiku.
/// </summary>
public interface IForgeBlockTemplateProvider
{
    /// <summary>
    /// Vrací seznam šablon, které ForgeBlock poskytuje.
    /// Voláno při registraci balíku do ForgeBlockRegistry.
    /// </summary>
    IReadOnlyList<ForgeBlockTemplate> GetTemplates();
}

/// <summary>
/// Jedna Scriban šablona poskytovaná ForgeBlockem.
/// </summary>
public sealed record ForgeBlockTemplate(
    /// <summary>Název šablony (bez přípony, např. "DbContext", "AutoMapperProfile").</summary>
    string Name,

    /// <summary>Kategorie pro organizaci (např. "EfCore", "AutoMapper").</summary>
    string Category,

    /// <summary>Obsah Scriban šablony.</summary>
    string Content
);
