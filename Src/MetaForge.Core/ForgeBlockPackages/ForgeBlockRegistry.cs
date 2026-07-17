using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Centrální registr ForgeBlock balíků — thread-safe.
/// Spravuje registraci, duplicitu a dotazování.
/// Automaticky registruje Scriban šablony z balíků implementujících IForgeBlockTemplateProvider.
/// </summary>
public sealed class ForgeBlockRegistry
{
    private readonly ConcurrentDictionary<string, IForgeBlockPackage> _packages = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ForgeBlockTemplate> _templates = new();

    /// <summary>Všechny registrované balíky.</summary>
    public IReadOnlyList<IForgeBlockPackage> Packages => _packages.Values.ToList().AsReadOnly();

    /// <summary>Všechny Scriban šablony registrované ForgeBlocky.</summary>
    public IReadOnlyList<ForgeBlockTemplate> Templates => _templates.AsReadOnly();

    /// <summary>Zaregistruje ForgeBlock balík.</summary>
    /// <exception cref="InvalidOperationException">Pokud handle již existuje.</exception>
    public void Register(IForgeBlockPackage package)
    {
        if (!_packages.TryAdd(package.Handle, package))
            throw new InvalidOperationException(
                $"ForgeBlock s handle '{package.Handle}' je již zaregistrován.");

        package.Register(this);

        // Pokud balík poskytuje Scriban šablony, zaregistruj je
        if (package is IForgeBlockTemplateProvider templateProvider)
        {
            foreach (var template in templateProvider.GetTemplates())
            {
                _templates.Add(template);
            }
        }
    }

    /// <summary>Najde balík podle handle. Vrací null pokud nenajde.</summary>
    public IForgeBlockPackage? GetPackage(string handle) =>
        _packages.TryGetValue(handle, out var package) ? package : null;

    /// <summary>Vyhledá balíky podle tagu.</summary>
    public IReadOnlyList<IForgeBlockPackage> SearchByTag(string tag)
    {
        var snapshot = _packages.Values.ToList();
        return snapshot
            .Where(p => p.Discovery.Tags?.Contains(tag, StringComparer.OrdinalIgnoreCase) == true)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>Vrátí všechny capability napříč všemi balíky.</summary>
    public IReadOnlyList<ForgeBlockCapability> GetAllCapabilities()
    {
        var snapshot = _packages.Values.ToList();
        return snapshot
            .SelectMany(p => p.Capabilities)
            .ToList()
            .AsReadOnly();
    }

    // ========================================================================
    // PROP-054: Deklarativní DI registrace
    // ========================================================================

    /// <summary>
    /// Aplikuje DI registrace ze všech registrovaných ForgeBlocků.
    /// Čte <see cref="DiRegistrationAttribute"/> atributy reflectionem.
    /// </summary>
    public void ApplyToDi(IServiceCollection services)
    {
        foreach (var package in _packages.Values)
        {
            var type = package.GetType();
            var attributes = type.GetCustomAttributes<DiRegistrationAttribute>();

            foreach (var attr in attributes)
            {
                if (attr.FactoryMethod is not null)
                {
                    // Tovární metoda
                    var factoryMethod = type.GetMethod(attr.FactoryMethod, BindingFlags.Public | BindingFlags.Static);
                    if (factoryMethod is not null)
                    {
                        var factoryDelegate = Delegate.CreateDelegate(
                            typeof(Func<IServiceProvider, object>), factoryMethod);
                        RegisterWithLifetime(services, attr.ServiceType, factoryDelegate, attr.Lifetime, attr.ReplaceExisting);
                    }
                }
                else if (attr.ImplementationType is not null)
                {
                    RegisterWithLifetime(services, attr.ServiceType, attr.ImplementationType, attr.Lifetime, attr.ReplaceExisting);
                }
                else
                {
                    RegisterWithLifetime(services, attr.ServiceType, attr.ServiceType, attr.Lifetime, attr.ReplaceExisting);
                }
            }
        }
    }

    private static void RegisterWithLifetime(
        IServiceCollection services,
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime,
        bool replaceExisting)
    {
        if (replaceExisting)
        {
            // Odebrat existující registrace stejného typu
            var existing = services.Where(s => s.ServiceType == serviceType).ToList();
            foreach (var descriptor in existing)
                services.Remove(descriptor);
        }

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton(serviceType, implementationType);
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(serviceType, implementationType);
                break;
            case ServiceLifetime.Transient:
                services.AddTransient(serviceType, implementationType);
                break;
        }
    }

    private static void RegisterWithLifetime(
        IServiceCollection services,
        Type serviceType,
        Delegate factoryDelegate,
        ServiceLifetime lifetime,
        bool replaceExisting)
    {
        if (replaceExisting)
        {
            var existing = services.Where(s => s.ServiceType == serviceType).ToList();
            foreach (var descriptor in existing)
                services.Remove(descriptor);
        }

        var sd = new ServiceDescriptor(serviceType, factoryDelegate, lifetime);
        services.Add(sd);
    }

}
