namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Rozšířený kontrakt pro ForgeBlocky s catalog entries.
/// </summary>
public interface IForgeBlockCapabilityPackage : IForgeBlockPackage
{
    /// <summary>Descriptor balíku.</summary>
    ForgeBlockPackageDescriptor Descriptor { get; }

    /// <summary>Catalog entries — typy/operace které balík přidává do katalogu.</summary>
    IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; }
}
