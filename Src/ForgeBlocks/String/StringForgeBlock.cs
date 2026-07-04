using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.ForgeBlocks.String;

/// <summary>
/// ForgeBlock pro textové operace.
/// Poskytuje capabilities: konkatenace, formátování, ořezávání, vyhledávání.
/// </summary>
public sealed class StringForgeBlock : IForgeBlockCapabilityPackage
{
    public string Handle => "string";
    public string Version => "1.0.0";

    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
    {
        new("str_concat", "Konkatenace", "Spojí dva nebo více řetězců", new[] { "string", "concat" }),
        new("str_format", "Formátování", "Naformátuje řetězec podle šablony", new[] { "string", "format" }),
        new("str_trim", "Ořezání", "Odstraní bílé znaky ze začátku a konce", new[] { "string", "trim" }),
        new("str_contains", "Obsahuje", "Zkontroluje, zda řetězec obsahuje podřetězec", new[] { "string", "search" }),
        new("str_replace", "Nahrazení", "Nahradí část řetězce jiným", new[] { "string", "replace" }),
        new("str_split", "Rozdělení", "Rozdělí řetězec podle oddělovače", new[] { "string", "split" }),
        new("str_upper", "Velká písmena", "Převede řetězec na velká písmena", new[] { "string", "case" }),
        new("str_lower", "Malá písmena", "Převede řetězec na malá písmena", new[] { "string", "case" }),
    };

    public DiscoveryMetadata Discovery { get; } = new(
        DisplayName: "String ForgeBlock",
        Description: "Textové operace pro MetaForge",
        Author: "MetaForge Team",
        Tags: new[] { "string", "text", "format", "search" },
        Categories: new[] { "String", "Core" }
    );

    public ForgeBlockPackageDescriptor Descriptor { get; } = new(
        Handle: "string",
        Version: "1.0.0",
        DisplayName: "String",
        Description: "Textové operace"
    );

    public IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; } = new List<ForgeBlockCatalogEntryDescriptor>
    {
        new("StringHelper", "MetaForge.ForgeBlocks.String.StringHelper", "Pomocná třída pro textové operace"),
    };

    public void Register(ForgeBlockRegistry registry)
    {
        // V1: CatalogEntries jsou dostupné přes IForgeBlockCapabilityPackage.CatalogEntries.
        // Propojení s CatalogManager bude implementováno v Epic 11 — Infrastructure.
    }
}
