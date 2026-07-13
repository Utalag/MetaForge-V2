using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Generators.Monetization;

namespace MetaForge.ForgeBlocks.EntityFrameworkCore;

/// <summary>
/// ForgeBlock pro Entity Framework Core — generuje DbContext, entity konfiguraci,
/// repository vrstvu a migrace. TIER 2+ (Infrastructure).
/// Poskytuje Scriban šablony: DbContext, EntityTypeConfig.
/// </summary>
public sealed class EfCoreForgeBlock : IForgeBlockCapabilityPackage, IForgeBlockTemplateProvider
{
    public string Handle => "orm-ef-core";
    public string Version => "1.0.0";

    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
    {
        new("generate-dbcontext", "Generuje AppDbContext", "Vygeneruje AppDbContext s DbSet<T> pro každou entitu", new[] { "orm", "ef-core", "database" }),
        new("generate-entity-config", "Generuje EntityTypeConfiguration", "Vygeneruje IEntityTypeConfiguration<T> pro každou entitu", new[] { "orm", "ef-core", "configuration" }),
        new("generate-repository", "Generuje Repository", "Vygeneruje repository interface + implementaci pro každou entitu", new[] { "orm", "ef-core", "repository" }),
        new("generate-migration", "Generuje migraci", "Vygeneruje EF Core migraci pro aktuální model", new[] { "orm", "ef-core", "migration" }),
        new("generate-di", "Generuje DI registraci", "Vygeneruje extension metody pro IServiceCollection", new[] { "orm", "ef-core", "di" }),
    };

    public DiscoveryMetadata Discovery { get; } = new(
        DisplayName: "Entity Framework Core",
        Description: "Generuje DbContext, entity konfiguraci, migrace a repository vrstvu pro EF Core",
        Author: "MetaForge Team",
        Tags: new[] { "orm", "ef-core", "database", "sql", "migration", "repository" },
        Categories: new[] { "ORM", "Database", "Infrastructure" }
    );

    public ForgeBlockPackageDescriptor Descriptor { get; } = new(
        Handle: "orm-ef-core",
        Version: "1.0.0",
        DisplayName: "Entity Framework Core",
        Description: "EF Core — DbContext, konfigurace entit, migrace, repository"
    );

    public IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; } = new List<ForgeBlockCatalogEntryDescriptor>
    {
        new("ef-core-config", "Konfigurace EF Core — connection string, provider"),
        new("ef-core-audit", "Audit — CreatedAt, UpdatedAt, IsDeleted"),
        new("ef-core-soft-delete", "Soft delete — IsDeleted flag + query filter"),
    };

    /// <summary>Tier potřebný pro použití tohoto ForgeBlocku.</summary>
    public GeneratorTier RequiredTier => GeneratorTier.Infrastructure;

    public void Register(ForgeBlockRegistry registry)
    {
        // Balík je již zaregistrován v registru (voláno z ForgeBlockRegistry.Register).
        // Capabilities a CatalogEntries jsou dostupné přes IForgeBlockCapabilityPackage.
        // Šablony jsou automaticky zaregistrovány přes IForgeBlockTemplateProvider.
    }

    /// <inheritdoc />
    public IReadOnlyList<ForgeBlockTemplate> GetTemplates()
    {
        return new List<ForgeBlockTemplate>
        {
            new("DbContext", "EfCore", Templates.DbContext),
            new("EntityTypeConfig", "EfCore", Templates.EntityTypeConfig),
        };
    }

    /// <summary>
    /// Scriban šablony embedded přímo v kódu — pro plugin registraci.
    /// Při NuGet distribuci se načítají jako embedded resources.
    /// </summary>
    private static class Templates
    {
        public const string DbContext = """
using Microsoft.EntityFrameworkCore;

namespace {{ namespace }};

/// <summary>
/// Hlavní DbContext pro aplikaci — generováno MetaForge EF Core ForgeBlockem.
/// </summary>
public class {{ class_name }} : DbContext
{
    public {{ class_name }}(DbContextOptions<{{ class_name }}> options) : base(options)
    {
    }

{{~ for entity in entities }}
    public DbSet<{{ entity }}> {{ entity }}s { get; set; }
{{~ end }}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

{{~ for entity in entities }}
        modelBuilder.Entity<{{ entity }}>(entity =>
        {
            entity.ToTable("{{ entity }}s");
        });
{{~ end }}
    }
}
""";

        public const string EntityTypeConfig = """
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {{ namespace }}.Configurations;

public class {{ entity_name }}Configuration : IEntityTypeConfiguration<{{ entity_name }}>
{
    public void Configure(EntityTypeBuilder<{{ entity_name }}> builder)
    {
        builder.ToTable("{{ table_name }}");

{{~ for property in properties }}
{{~ if property.is_key }}
        builder.HasKey(e => e.{{ property.name }});
{{~ end }}
{{~ if property.is_required }}
        builder.Property(e => e.{{ property.name }}).IsRequired();
{{~ end }}
{{~ if property.max_length }}
        builder.Property(e => e.{{ property.name }}).HasMaxLength({{ property.max_length }});
{{~ end }}
{{~ end }}
    }
}
""";
    }
}
