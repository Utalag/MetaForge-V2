using MetaForge.Core.Common;

namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Discovery metadata jednoho capability facet vystaveneho ForgeBlock balickem.
/// </summary>
public enum ForgeBlockCapabilityKind
{
    Utility,
    StandardLibraryWrapper,
    FrameworkIntegration,
    ArchitecturalPattern,
    BuilderAdapter,
    CatalogExtension
}

public sealed record ForgeBlockCapabilityDescriptor
{
    public required string PackageId { get; init; }

    public required string CapabilityId { get; init; }

    public required string DisplayName { get; init; }

    public string Description { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public ForgeBlockCapabilityKind Kind { get; init; } = ForgeBlockCapabilityKind.Utility;

    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<string> SemanticHandles { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<ProgramLanguage> SupportedLanguages { get; init; } = Array.Empty<ProgramLanguage>();

    public IReadOnlyCollection<string> DependsOnPackages { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<ForgeBlockCapabilityReference> DependsOnCapabilities { get; init; } = Array.Empty<ForgeBlockCapabilityReference>();
}

/// <summary>
/// In-memory katalog capability descriptoru registrovanych providerem.
/// </summary>
public sealed class ForgeBlockCapabilityCatalog : IForgeBlockCapabilityCatalog
{
    private readonly Dictionary<string, ForgeBlockCapabilityDescriptor> _capabilities = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<ForgeBlockCapabilityDescriptor> Capabilities => _capabilities.Values
        .OrderBy(capability => capability.PackageId, StringComparer.OrdinalIgnoreCase)
        .ThenBy(capability => capability.CapabilityId, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public void AddCapability(ForgeBlockCapabilityDescriptor capability)
    {
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentException.ThrowIfNullOrWhiteSpace(capability.PackageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(capability.CapabilityId);

        var key = BuildKey(capability.PackageId, capability.CapabilityId);
        if (_capabilities.TryGetValue(key, out var existing))
        {
            if (!EqualityComparer<ForgeBlockCapabilityDescriptor>.Default.Equals(existing, capability))
                throw new InvalidOperationException($"Conflicting ForgeBlock capability '{capability.PackageId}/{capability.CapabilityId}'.");

            return;
        }

        _capabilities[key] = capability;
    }

    public bool TryGetCapability(string packageId, string capabilityId, out ForgeBlockCapabilityDescriptor capability)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(capabilityId);

        return _capabilities.TryGetValue(BuildKey(packageId, capabilityId), out capability!);
    }

    private static string BuildKey(string packageId, string capabilityId) => $"{packageId}/{capabilityId}";
}