using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;

namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Sdilena registry ForgeBlock package kontraktu a jejich contribution facetu.
/// </summary>
public sealed class ForgeBlockPackageRegistry : IForgeBlockRegistry
{
    private readonly Lock _syncRoot = new();
    private readonly Dictionary<string, ForgeBlockPackageDescriptor> _packages = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IForgeBlockCapabilityProvider> _capabilityProviders = new();
    private readonly List<IForgeBlockGeneratorContributor> _generatorContributors = new();
    private readonly List<IForgeBlockCatalogContributor> _catalogContributors = new();
    private readonly List<IForgeBlockDiscoveryContributor> _discoveryContributors = new();
    private readonly List<CapabilityMetadata> _capabilityMetadata = new();

    public void Register(IForgeBlockPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentException.ThrowIfNullOrWhiteSpace(package.Descriptor.Id);

        var packageId = package.Descriptor.Id;

        lock (_syncRoot)
        {
            _packages[packageId] = package.Descriptor;
            RemoveContributions(packageId);
        }

        var completed = false;
        try
        {
            package.Register(this);
            completed = true;
        }
        finally
        {
            if (!completed)
            {
                lock (_syncRoot)
                {
                    _packages.Remove(packageId);
                    RemoveContributions(packageId);
                }
            }
        }
    }

    public void AddCapabilityProvider(IForgeBlockCapabilityProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        EnsurePackageIsKnown(provider.PackageId);

        lock (_syncRoot)
        {
            _capabilityProviders.Add(provider);
        }
    }

    public void AddGeneratorContributor(IForgeBlockGeneratorContributor contributor)
    {
        ArgumentNullException.ThrowIfNull(contributor);
        EnsurePackageIsKnown(contributor.PackageId);

        lock (_syncRoot)
        {
            _generatorContributors.Add(contributor);
        }
    }

    public void AddCatalogContributor(IForgeBlockCatalogContributor contributor)
    {
        ArgumentNullException.ThrowIfNull(contributor);
        EnsurePackageIsKnown(contributor.PackageId);

        lock (_syncRoot)
        {
            _catalogContributors.Add(contributor);
        }
    }

    public void AddDiscoveryContributor(IForgeBlockDiscoveryContributor contributor)
    {
        ArgumentNullException.ThrowIfNull(contributor);
        EnsurePackageIsKnown(contributor.PackageId);

        lock (_syncRoot)
        {
            _discoveryContributors.Add(contributor);
        }
    }

    public void RegisterCapability(CapabilityMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrWhiteSpace(metadata.PackageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(metadata.ToolId);
        EnsurePackageIsKnown(metadata.PackageId);

        lock (_syncRoot)
        {
            _capabilityMetadata.Add(metadata);
        }
    }

    public GeneratedCodeArtifact BuildArtifact(ILanguageElement element, ProgramLanguage language)
    {
        ArgumentNullException.ThrowIfNull(element);

        List<IForgeBlockGeneratorContributor> contributors;
        lock (_syncRoot)
        {
            contributors = _generatorContributors.ToList();
        }

        if (contributors.Count == 0)
            return new GeneratedCodeArtifact();

        var context = new ForgeBlockGenerationContext(element, language);

        foreach (var contributor in contributors
                     .Where(contributor => contributor.SupportsLanguage(language)
                                           && contributor.CanContribute(element, language))
                     .OrderBy(contributor => contributor.PackageId, StringComparer.OrdinalIgnoreCase))
        {
            contributor.Contribute(context);
        }

        return context.BuildArtifact();
    }

    public bool IsRegistered(string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        lock (_syncRoot)
        {
            return _packages.ContainsKey(packageId);
        }
    }

    public IReadOnlyCollection<string> GetRegisteredPackageIds()
    {
        lock (_syncRoot)
        {
            return [.. _packages.Keys.OrderBy(packageId => packageId, StringComparer.OrdinalIgnoreCase)];
        }
    }

    public IReadOnlyCollection<ForgeBlockPackageDescriptor> GetPackages()
    {
        lock (_syncRoot)
        {
            return [.. _packages.Values.OrderBy(descriptor => descriptor.Id, StringComparer.OrdinalIgnoreCase)];
        }
    }

    public bool TryGetPackage(string packageId, out ForgeBlockPackageDescriptor package)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        lock (_syncRoot)
        {
            return _packages.TryGetValue(packageId, out package!);
        }
    }

    public IReadOnlyCollection<ForgeBlockCapabilityDescriptor> GetCapabilities()
    {
        List<IForgeBlockCapabilityProvider> providers;
        lock (_syncRoot)
        {
            providers = _capabilityProviders.ToList();
        }

        if (providers.Count == 0)
            return Array.Empty<ForgeBlockCapabilityDescriptor>();

        var catalog = new ForgeBlockCapabilityCatalog();
        foreach (var provider in providers.OrderBy(provider => provider.PackageId, StringComparer.OrdinalIgnoreCase))
        {
            provider.RegisterCapabilities(catalog);
        }

        return catalog.Capabilities;
    }

    public IReadOnlyCollection<ForgeBlockCapabilityDescriptor> GetCapabilities(string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        return GetCapabilities()
            .Where(capability => string.Equals(capability.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public bool TryGetCapability(string packageId, string capabilityId, out ForgeBlockCapabilityDescriptor capability)
    {
        var catalog = new ForgeBlockCapabilityCatalog();

        List<IForgeBlockCapabilityProvider> providers;
        lock (_syncRoot)
        {
            providers = _capabilityProviders
                .Where(provider => string.Equals(provider.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        foreach (var provider in providers)
        {
            provider.RegisterCapabilities(catalog);
        }

        return catalog.TryGetCapability(packageId, capabilityId, out capability!);
    }

    public IReadOnlyCollection<ForgeBlockCatalogEntryDescriptor> GetCatalogEntries()
    {
        List<IForgeBlockCatalogContributor> contributors;
        lock (_syncRoot)
        {
            contributors = _catalogContributors.ToList();
        }

        if (contributors.Count == 0)
            return Array.Empty<ForgeBlockCatalogEntryDescriptor>();

        var catalog = new ForgeBlockCatalog();
        foreach (var contributor in contributors.OrderBy(contributor => contributor.PackageId, StringComparer.OrdinalIgnoreCase))
        {
            contributor.RegisterCatalogEntries(catalog);
        }

        return catalog.Entries;
    }

    public IReadOnlyCollection<ForgeBlockCatalogEntryDescriptor> GetCatalogEntries(string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        return GetCatalogEntries()
            .Where(entry => string.Equals(entry.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public IReadOnlyCollection<ForgeBlockDiscoveryItem> GetDiscoveryItems()
    {
        List<IForgeBlockDiscoveryContributor> contributors;
        lock (_syncRoot)
        {
            contributors = _discoveryContributors.ToList();
        }

        if (contributors.Count == 0)
            return Array.Empty<ForgeBlockDiscoveryItem>();

        var catalog = new ForgeBlockDiscoveryCatalog();
        foreach (var contributor in contributors.OrderBy(contributor => contributor.PackageId, StringComparer.OrdinalIgnoreCase))
        {
            contributor.RegisterDiscovery(catalog);
        }

        return catalog.Items;
    }

    public IReadOnlyCollection<ForgeBlockDiscoveryItem> GetDiscoveryItems(string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        return GetDiscoveryItems()
            .Where(item => item.Tags.Any(tag => tag.StartsWith($"pkg:{packageId}", StringComparison.OrdinalIgnoreCase)))
            .ToArray();
    }

    public IReadOnlyCollection<CapabilityMetadata> GetCapabilityMetadata()
    {
        lock (_syncRoot)
        {
            return [.. _capabilityMetadata
                .OrderBy(m => m.PackageId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(m => m.ToolId, StringComparer.OrdinalIgnoreCase)];
        }
    }

    public IReadOnlyCollection<CapabilityMetadata> GetCapabilityMetadata(string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        lock (_syncRoot)
        {
            return [.. _capabilityMetadata
                .Where(m => string.Equals(m.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.ToolId, StringComparer.OrdinalIgnoreCase)];
        }
    }

    public bool TryGetCatalogEntry(string packageId, string entryId, out ForgeBlockCatalogEntryDescriptor entry)
    {
        var catalog = new ForgeBlockCatalog();

        List<IForgeBlockCatalogContributor> contributors;
        lock (_syncRoot)
        {
            contributors = _catalogContributors
                .Where(contributor => string.Equals(contributor.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        foreach (var contributor in contributors)
        {
            contributor.RegisterCatalogEntries(catalog);
        }

        return catalog.TryGetEntry(packageId, entryId, out entry!);
    }

    public void MergeFrom(ForgeBlockPackageRegistry other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (ReferenceEquals(this, other))
            return;

        List<(ForgeBlockPackageDescriptor Descriptor, List<IForgeBlockCapabilityProvider> CapabilityProviders, List<IForgeBlockGeneratorContributor> GeneratorContributors, List<IForgeBlockCatalogContributor> CatalogContributors, List<CapabilityMetadata> CapabilityMeta)> snapshot;

        lock (other._syncRoot)
        {
            snapshot = other._packages.Values
                .OrderBy(descriptor => descriptor.Id, StringComparer.OrdinalIgnoreCase)
                .Select(descriptor => (
                    descriptor,
                    other._capabilityProviders.Where(provider => string.Equals(provider.PackageId, descriptor.Id, StringComparison.OrdinalIgnoreCase)).ToList(),
                    other._generatorContributors.Where(contributor => string.Equals(contributor.PackageId, descriptor.Id, StringComparison.OrdinalIgnoreCase)).ToList(),
                    other._catalogContributors.Where(contributor => string.Equals(contributor.PackageId, descriptor.Id, StringComparison.OrdinalIgnoreCase)).ToList(),
                    other._capabilityMetadata.Where(m => string.Equals(m.PackageId, descriptor.Id, StringComparison.OrdinalIgnoreCase)).ToList()))
                .ToList();
        }

        foreach (var item in snapshot)
        {
            lock (_syncRoot)
            {
                _packages[item.Descriptor.Id] = item.Descriptor;
                RemoveContributions(item.Descriptor.Id);
                _capabilityProviders.AddRange(item.CapabilityProviders);
                _generatorContributors.AddRange(item.GeneratorContributors);
                _catalogContributors.AddRange(item.CatalogContributors);
                _capabilityMetadata.AddRange(item.CapabilityMeta);
            }
        }
    }

    private void EnsurePackageIsKnown(string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        lock (_syncRoot)
        {
            if (_packages.ContainsKey(packageId))
                return;
        }

        throw new InvalidOperationException($"ForgeBlock package '{packageId}' is not registered.");
    }

    private void RemoveContributions(string packageId)
    {
        _capabilityProviders.RemoveAll(provider => string.Equals(provider.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
        _generatorContributors.RemoveAll(contributor => string.Equals(contributor.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
        _catalogContributors.RemoveAll(contributor => string.Equals(contributor.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
        _discoveryContributors.RemoveAll(contributor => string.Equals(contributor.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
        _capabilityMetadata.RemoveAll(m => string.Equals(m.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
    }
}