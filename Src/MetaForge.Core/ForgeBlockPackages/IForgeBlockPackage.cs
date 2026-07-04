namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Minimální kontrakt pro ForgeBlock balík.
/// Každý ForgeBlock musí mít Handle, Version, Capabilities a Discovery metadata.
/// </summary>
public interface IForgeBlockPackage
{
    /// <summary>Unikátní identifikátor (např. "math", "string", "validation").</summary>
    string Handle { get; }

    /// <summary>Sémantická verze.</summary>
    string Version { get; }

    /// <summary>Seznam capabilities, které balík poskytuje.</summary>
    IReadOnlyList<ForgeBlockCapability> Capabilities { get; }

    /// <summary>Discovery metadata pro katalog.</summary>
    DiscoveryMetadata Discovery { get; }

    /// <summary>Zaregistruje balík do registru.</summary>
    void Register(ForgeBlockRegistry registry);
}
