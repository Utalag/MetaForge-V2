using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Generators.Monetization;

namespace MetaForge.ForgeBlocks.FluentValidation;

/// <summary>
/// ForgeBlock pro FluentValidation — generuje validátory pro každou entitu.
/// TIER 2+ (Infrastructure).
/// Poskytuje Scriban šablonu: FluentValidator.
/// </summary>
public sealed class FluentValidationForgeBlock : IForgeBlockCapabilityPackage, IForgeBlockTemplateProvider
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
        // Šablony jsou automaticky zaregistrovány přes IForgeBlockTemplateProvider.
    }

    /// <inheritdoc />
    public IReadOnlyList<ForgeBlockTemplate> GetTemplates()
    {
        return new List<ForgeBlockTemplate>
        {
            new("FluentValidator", "FluentValidation", Templates.FluentValidator),
        };
    }

    private static class Templates
    {
        public const string FluentValidator = """
using FluentValidation;

namespace {{ namespace }}.Validators;

public class {{ entity_name }}Validator : AbstractValidator<{{ entity_name }}>
{
    public {{ entity_name }}Validator()
    {
{{~ for rule in validation_rules }}
{{~ if rule.type == "not_empty" }}
        RuleFor(x => x.{{ rule.property }}).NotEmpty();
{{~ elsif rule.type == "email" }}
        RuleFor(x => x.{{ rule.property }}).EmailAddress();
{{~ elsif rule.type == "max_length" }}
        RuleFor(x => x.{{ rule.property }}).MaximumLength({{ rule.max }});
{{~ elsif rule.type == "min_length" }}
        RuleFor(x => x.{{ rule.property }}).MinimumLength({{ rule.min }});
{{~ elsif rule.type == "inclusive_between" }}
        RuleFor(x => x.{{ rule.property }}).InclusiveBetween({{ rule.min }}, {{ rule.max }});
{{~ elsif rule.type == "regex" }}
        RuleFor(x => x.{{ rule.property }}).Matches(@"{{ rule.pattern }}");
{{~ else }}
        RuleFor(x => x.{{ rule.property }}).NotNull();
{{~ end }}
{{~ end }}
    }
}
""";
    }
}
