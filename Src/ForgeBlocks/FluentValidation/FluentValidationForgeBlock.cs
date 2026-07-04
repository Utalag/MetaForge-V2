using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Generators.Monetization;

namespace MetaForge.ForgeBlocks.FluentValidation;

/// <summary>
/// ForgeBlock pro FluentValidation — generuje validátory pro každou entitu.
/// TIER 2+ (Infrastructure).
/// </summary>
public sealed class FluentValidationForgeBlock : IForgeBlockCapabilityPackage
{
    public string Handle => "validation-fluent";
    public string Version => "1.0.0";

    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
    {
        new("generate-validator", "Generuje Validator", "Vygeneruje FluentValidation validátor pro každou entitu", new[] { "validation", "fluent", "validator" }),
        new("generate-validation-rules", "Generuje validační pravidla", "Vygeneruje RuleFor() pravidla na základě atributů", new[] { "validation", "fluent", "rules" }),
        new("generate-di", "Generuje DI registraci", "Vygeneruje AddValidatorsFromAssembly() registraci", new[] { "validation", "fluent", "di" }),
    };

    public DiscoveryMetadata Discovery { get; } = new(
        DisplayName: "FluentValidation",
        Description: "Generuje FluentValidation validátory pro každou business entitu",
        Author: "MetaForge Team",
        Tags: new[] { "validation", "fluent", "validator", "rules" },
        Categories: new[] { "Validation", "Infrastructure" }
    );

    public ForgeBlockPackageDescriptor Descriptor { get; } = new(
        Handle: "validation-fluent",
        Version: "1.0.0",
        DisplayName: "FluentValidation",
        Description: "FluentValidation — validátory pro entity"
    );

    public IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; } = new List<ForgeBlockCatalogEntryDescriptor>
    {
        new("fluent-validator", "FluentValidation validátor pro entitu"),
        new("fluent-validation-rules", "Validační pravidla — RuleFor, When, Unless"),
    };

    public GeneratorTier RequiredTier => GeneratorTier.Infrastructure;

    public void Register(ForgeBlockRegistry registry)
    {
        // Balík je již zaregistrován v registru (voláno z ForgeBlockRegistry.Register).
    }
}
