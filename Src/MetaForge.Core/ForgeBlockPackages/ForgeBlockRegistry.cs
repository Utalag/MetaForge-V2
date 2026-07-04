using System.Collections.Concurrent;

namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Centrální registr ForgeBlock balíků — thread-safe.
/// Spravuje registraci, duplicitu a dotazování.
/// </summary>
public sealed class ForgeBlockRegistry
{
    private readonly ConcurrentDictionary<string, IForgeBlockPackage> _packages = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Všechny registrované balíky.</summary>
    public IReadOnlyList<IForgeBlockPackage> Packages => _packages.Values.ToList().AsReadOnly();

    /// <summary>Zaregistruje ForgeBlock balík.</summary>
    /// <exception cref="InvalidOperationException">Pokud handle již existuje.</exception>
    public void Register(IForgeBlockPackage package)
    {
        if (!_packages.TryAdd(package.Handle, package))
            throw new InvalidOperationException(
                $"ForgeBlock s handle '{package.Handle}' je již zaregistrován.");

        package.Register(this);
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
}
