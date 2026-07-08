using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;

namespace MetaForge.Core.ForgeBlockPackages;

public interface IForgeBlockCapabilityCatalog
{
    void AddCapability(ForgeBlockCapabilityDescriptor capability);
}

public interface IForgeBlockCatalog
{
    void AddEntry(ForgeBlockCatalogEntryDescriptor entry);
}

public interface IForgeBlockCapabilityProvider
{
    string PackageId { get; }

    void RegisterCapabilities(IForgeBlockCapabilityCatalog catalog);
}

public interface IForgeBlockGeneratorContributor
{
    string PackageId { get; }

    bool SupportsLanguage(ProgramLanguage language);

    bool CanContribute(ILanguageElement element, ProgramLanguage language);

    void Contribute(IForgeBlockGenerationContext context);
}

public interface IForgeBlockCatalogContributor
{
    string PackageId { get; }

    void RegisterCatalogEntries(IForgeBlockCatalog catalog);
}

public interface IForgeBlockPackageRegistryAware
{
    ForgeBlockPackageRegistry ForgeBlockPackageRegistry { get; }

    void UseForgeBlockPackageRegistry(ForgeBlockPackageRegistry registry);
}