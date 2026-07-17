// ---------------------------------------------------------------------------
// MetaForge.Core — DiRegistrationAttribute
// Declarative DI registration for ForgeBlock plugins.
// Vrstva: Core / ForgeBlockPackages
//
// PROPOSAL: PROP-054 — ForgeBlock DI Extension Methods (deklarativní přístup)
// ---------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Deklarativní DI registrace pro ForgeBlock.
/// Aplikuje se na třídu ForgeBlocku — ForgeBlockRegistry.ApplyToDi() čte tyto atributy reflectionem.
/// 
/// Příklad:
/// <code>
/// [DiRegistration(ServiceType = typeof(IRepository&lt;&gt;), ImplementationType = typeof(EfRepository&lt;&gt;))]
/// [DiRegistration(ServiceType = typeof(AppDbContext), FactoryMethod = nameof(CreateDbContext))]
/// public sealed class EfCoreForgeBlock : IForgeBlockCapabilityPackage { }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DiRegistrationAttribute : Attribute
{
    /// <summary>Typ služby k registraci (např. typeof(IRepository&lt;&gt;)).</summary>
    public Type ServiceType { get; init; } = null!;

    /// <summary>Implementační typ. Null = použije se ServiceType sám.</summary>
    public Type? ImplementationType { get; init; }

    /// <summary>Název statické tovární metody na ForgeBlock třídě. Použije se místo ImplementationType.</summary>
    public string? FactoryMethod { get; init; }

    /// <summary>Životnost služby. Výchozí: Scoped.</summary>
    public ServiceLifetime Lifetime { get; init; } = ServiceLifetime.Scoped;

    /// <summary>Pokud true, existující registrace stejného typu se přepíší.</summary>
    public bool ReplaceExisting { get; init; }
}
