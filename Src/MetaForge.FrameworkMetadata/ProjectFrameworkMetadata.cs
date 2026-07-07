using MetaForge.FrameworkMetadata.AspNetCore;
using MetaForge.FrameworkMetadata.DependencyInjection;
using MetaForge.FrameworkMetadata.EntityFrameworkCore;
using MetaForge.FrameworkMetadata.Hosting;

namespace MetaForge.FrameworkMetadata;

/// <summary>
/// Agreguje veškerá .NET framework/hosting metadata pro jeden projekt
/// (identifikovaný <see cref="ProjectName"/>, odpovídá `MetaForge.Core.Abstractions.ProjectElement.Name`).
/// Tato vrstva stojí nad Core a Core na ní nezávisí — Core zůstává jazykově/frameworkově agnostický
/// pro business a typový model, zatímco tato vrstva nese .NET-specifickou runtime sémantiku.
/// </summary>
public sealed class ProjectFrameworkMetadata
{
    /// <summary>Název projektu, ke kterému se metadata vztahují.</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Registrace služeb v DI kontejneru.</summary>
    public List<ServiceRegistration> ServiceRegistrations { get; } = new();

    /// <summary>ASP.NET Core endpointy (minimal API nebo controller akce).</summary>
    public List<EndpointMetadata> Endpoints { get; } = new();

    /// <summary>Middleware zaregistrovaný v request pipeline.</summary>
    public List<MiddlewareMetadata> Middlewares { get; } = new();

    /// <summary>EF Core konfigurace entit.</summary>
    public List<EntityConfiguration> EntityConfigurations { get; } = new();

    /// <summary>Dlouho běžící služby (IHostedService/BackgroundService).</summary>
    public List<HostedServiceMetadata> HostedServices { get; } = new();

    /// <summary>Konvence pro použití CancellationToken v asynchronních metodách.</summary>
    public List<CancellationTokenConvention> CancellationTokenConventions { get; } = new();
}
