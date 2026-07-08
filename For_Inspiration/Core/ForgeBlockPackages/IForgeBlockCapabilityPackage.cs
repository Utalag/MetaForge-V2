namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Kontrakt pro balicek, ktery registruje neutralni capability metadata (tool-level operace).
/// Implementace vola registry.RegisterCapability() pro kazdy tool.
/// </summary>
public interface IForgeBlockCapabilityPackage
{
    string PackageId { get; }

    void RegisterCapabilities(IForgeBlockRegistry registry);
}
