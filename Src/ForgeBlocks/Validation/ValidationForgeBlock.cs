using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.ForgeBlocks.Validation;

/// <summary>
/// ForgeBlock pro validační pravidla.
/// Poskytuje capabilities: not_empty, email_format, range, regex.
/// </summary>
public sealed class ValidationForgeBlock : IForgeBlockCapabilityPackage
{
    public string Handle => "validation";
    public string Version => "1.0.0";

    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
    {
        new("val_required", "Povinné pole", "Atribut nesmí být prázdný", new[] { "validation", "required" }),
        new("val_email", "Email formát", "Validace emailové adresy", new[] { "validation", "email" }),
        new("val_phone", "Telefonní formát", "Validace telefonního čísla", new[] { "validation", "phone" }),
        new("val_url", "URL formát", "Validace URL adresy", new[] { "validation", "url" }),
        new("val_range", "Rozsah", "Číselná hodnota v rozsahu", new[] { "validation", "range" }),
        new("val_regex", "Regulární výraz", "Validace podle regex patternu", new[] { "validation", "regex" }),
        new("val_max_length", "Maximální délka", "Maximální délka řetězce", new[] { "validation", "length" }),
        new("val_min_length", "Minimální délka", "Minimální délka řetězce", new[] { "validation", "length" }),
    };

    public DiscoveryMetadata Discovery { get; } = new(
        DisplayName: "Validation ForgeBlock",
        Description: "Validační pravidla pro MetaForge",
        Author: "MetaForge Team",
        Tags: new[] { "validation", "email", "phone", "url", "range", "regex" },
        Categories: new[] { "Validation", "Core" }
    );

    public ForgeBlockPackageDescriptor Descriptor { get; } = new(
        Handle: "validation",
        Version: "1.0.0",
        DisplayName: "Validation",
        Description: "Validační pravidla"
    );

    public IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; } = new List<ForgeBlockCatalogEntryDescriptor>
    {
        new("ValidationHelper", "MetaForge.ForgeBlocks.Validation.ValidationHelper", "Pomocná třída pro validační pravidla"),
    };

    public void Register(ForgeBlockRegistry registry)
    {
        // Registrace do katalogu
    }
}
