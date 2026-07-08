using MetaForge.Infrastructure.Caching;
using MetaForge.Infrastructure.Configuration;
using MetaForge.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MetaForge.Infrastructure;

/// <summary>
/// Extension metody pro DI registraci infrastrukturních služeb.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Zaregistruje infrastrukturní služby do DI containeru.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="useJsonPersistence">true = JSONL/JSON persistence, false = InMemory (sandbox).</param>
    public static IServiceCollection AddMetaForgeInfrastructure(
        this IServiceCollection services,
        bool useJsonPersistence = true)
    {
        // Konfigurace — IOptions<T> z appsettings.json
        services.AddOptions<MetaForgeOptions>()
            .BindConfiguration(MetaForgeOptions.SectionName);

        services.AddOptions<StorageOptions>()
            .BindConfiguration($"{MetaForgeOptions.SectionName}:Storage");

        services.AddOptions<AiOptions>()
            .BindConfiguration($"{MetaForgeOptions.SectionName}:Ai");

        // Persistence
        if (useJsonPersistence)
        {
            services.TryAddSingleton<ICommandLogRepository, JsonCommandLogRepository>();
            services.TryAddSingleton<IDocumentRepository, JsonDocumentRepository>();
        }
        else
        {
            services.TryAddSingleton<ICommandLogRepository, InMemoryCommandLogRepository>();
        }

        // Caching
        services.TryAddSingleton<IProjectionCache, CheckpointProjectionCache>();

        return services;
    }
}
