using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.ForgeBlocks.Math;

/// <summary>
/// ForgeBlock pro matematické operace.
/// Poskytuje capabilities: sčítání, odčítání, násobení, dělení, zaokrouhlování.
/// </summary>
public sealed class MathForgeBlock : IForgeBlockCapabilityPackage
{
    public string Handle => "math";
    public string Version => "1.0.0";

    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
    {
        new("math_add", "Sčítání", "Sečte dvě čísla", new[] { "math", "arithmetic" }),
        new("math_subtract", "Odčítání", "Odečte dvě čísla", new[] { "math", "arithmetic" }),
        new("math_multiply", "Násobení", "Vynásobí dvě čísla", new[] { "math", "arithmetic" }),
        new("math_divide", "Dělení", "Vydělí dvě čísla", new[] { "math", "arithmetic" }),
        new("math_round", "Zaokrouhlení", "Zaokrouhlí číslo na daný počet desetinných míst", new[] { "math", "rounding" }),
        new("math_abs", "Absolutní hodnota", "Vrátí absolutní hodnotu čísla", new[] { "math" }),
        new("math_pow", "Mocnina", "Umocní číslo na daný exponent", new[] { "math", "power" }),
        new("math_sqrt", "Odmocnina", "Vrátí druhou odmocninu čísla", new[] { "math" }),
    };

    public DiscoveryMetadata Discovery { get; } = new(
        DisplayName: "Math ForgeBlock",
        Description: "Základní matematické operace pro MetaForge",
        Author: "MetaForge Team",
        Tags: new[] { "math", "arithmetic", "rounding" },
        Categories: new[] { "Math", "Core" }
    );

    public ForgeBlockPackageDescriptor Descriptor { get; } = new(
        Handle: "math",
        Version: "1.0.0",
        DisplayName: "Math",
        Description: "Matematické operace"
    );

    public IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; } = new List<ForgeBlockCatalogEntryDescriptor>
    {
        new("MathHelper", "MetaForge.ForgeBlocks.Math.MathHelper", "Pomocná třída pro matematické operace"),
    };

    public void Register(ForgeBlockRegistry registry)
    {
        // Registrace do katalogu — přidá presety pro matematické typy
        // (implementace závisí na CatalogManager — volitelné pro v1)
    }
}
