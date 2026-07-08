namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Root kontrakt jednoho ForgeBlock balicku.
/// </summary>
public interface IForgeBlockPackage
{
    ForgeBlockPackageDescriptor Descriptor { get; }

    void Register(IForgeBlockRegistry registry);
}

/// <summary>
/// Registry API, pres ktere balicek zapisuje svoje contributory.
/// </summary>
public interface IForgeBlockRegistry
{
    void AddCapabilityProvider(IForgeBlockCapabilityProvider provider);

    void AddGeneratorContributor(IForgeBlockGeneratorContributor contributor);

    void AddCatalogContributor(IForgeBlockCatalogContributor contributor);

    void AddDiscoveryContributor(IForgeBlockDiscoveryContributor contributor);

    void RegisterCapability(CapabilityMetadata metadata);
}