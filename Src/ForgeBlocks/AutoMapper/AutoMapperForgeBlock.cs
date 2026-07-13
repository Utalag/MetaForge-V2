using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Generators.Monetization;

namespace MetaForge.ForgeBlocks.AutoMapper;

/// <summary>
/// ForgeBlock pro AutoMapper — generuje Profile třídy a mapping konfiguraci.
/// TIER 1+ (Domain).
/// Poskytuje Scriban šablonu: AutoMapperProfile.
/// </summary>
public sealed class AutoMapperForgeBlock : IForgeBlockCapabilityPackage, IForgeBlockTemplateProvider
{
    public string Handle => "mapping-automapper";
    public string Version => "1.0.0";

    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
    {
        new("generate-mapping-profile", "Generuje MappingProfile", "Vygeneruje AutoMapper Profile s mapováním entit na DTO", new[] { "mapping", "automapper", "dto" }),
        new("generate-dto", "Generuje DTO", "Vygeneruje DTO třídy pro každou entitu", new[] { "mapping", "automapper", "dto" }),
        new("generate-mapping-config", "Generuje MappingConfig", "Vygeneruje konfiguraci AutoMapperu pro DI", new[] { "mapping", "automapper", "config" }),
    };

    public DiscoveryMetadata Discovery { get; } = new(
        DisplayName: "AutoMapper",
        Description: "Generuje AutoMapper Profile třídy, DTO a mapping konfiguraci",
        Author: "MetaForge Team",
        Tags: new[] { "mapping", "automapper", "dto", "profile" },
        Categories: new[] { "Mapping", "Domain" }
    );

    public ForgeBlockPackageDescriptor Descriptor { get; } = new(
        Handle: "mapping-automapper",
        Version: "1.0.0",
        DisplayName: "AutoMapper",
        Description: "AutoMapper — Profile, DTO, mapping konfigurace"
    );

    public IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; } = new List<ForgeBlockCatalogEntryDescriptor>
    {
        new("automapper-profile", "AutoMapper Profile — mapování entit na DTO"),
        new("automapper-dto", "DTO třídy — request/response modely"),
    };

    public GeneratorTier RequiredTier => GeneratorTier.Domain;

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
            new("AutoMapperProfile", "AutoMapper", Templates.AutoMapperProfile),
        };
    }

    private static class Templates
    {
        public const string AutoMapperProfile = """
using AutoMapper;

namespace {{ namespace }}.Mappings;

public class {{ class_name }} : Profile
{
    public {{ class_name }}()
    {
{{~ for mapping in mappings }}
        CreateMap<{{ mapping.source }}, {{ mapping.destination }}>()
{{~ if mapping.reverse_map }}
            .ReverseMap();
{{~ else }}
            ;
{{~ end }}
{{~ end }}
    }
}
""";
    }
}
