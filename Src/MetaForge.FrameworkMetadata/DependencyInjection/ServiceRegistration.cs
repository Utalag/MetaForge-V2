namespace MetaForge.FrameworkMetadata.DependencyInjection;

/// <summary>
/// Metadata pro registraci služby v DI kontejneru (`IServiceCollection`).
/// Odděleno od Core, protože jde o .NET hostingový detail, ne o business/typový model.
/// </summary>
public sealed record ServiceRegistration
{
    /// <summary>Název typu služby (interface nebo abstraktní třída), např. "IOrderRepository".</summary>
    public string ServiceTypeName { get; init; } = string.Empty;

    /// <summary>Název implementačního typu, např. "OrderRepository".</summary>
    public string ImplementationTypeName { get; init; } = string.Empty;

    /// <summary>Lifetime registrace (Singleton/Scoped/Transient).</summary>
    public ServiceLifetime Lifetime { get; init; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Volitelný název factory metody, pokud se registrace neděje přes prostou
    /// `AddScoped&lt;TService, TImplementation&gt;()`, ale přes `AddScoped(sp => ...)`.
    /// </summary>
    public string? FactoryMethodName { get; init; }
}

/// <summary>Lifetime registrované služby v DI kontejneru.</summary>
public enum ServiceLifetime
{
    Singleton,
    Scoped,
    Transient,
}
