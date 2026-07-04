namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Centrální registr ForgeBlock balíků.
/// Spravuje registraci, duplicitu a dotazování.
/// </summary>
public sealed class ForgeBlockRegistry
{
    private readonly Dictionary<string, IForgeBlockPackage> _packages = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Všechny registrované balíky.</summary>
    public IReadOnlyList<IForgeBlockPackage> Packages => _packages.Values.ToList().AsReadOnly();

    /// <summary>Zaregistruje ForgeBlock balík.</summary>
    /// <exception cref="InvalidOperationException">Pokud handle již existuje.</exception>
    public void Register(IForgeBlockPackage package)
    {
        if (_packages.ContainsKey(package.Handle))
            throw new InvalidOperationException(
                $"ForgeBlock s handle '{package.Handle}' je již zaregistrován.");

        _packages[package.Handle] = package;
        package.Register(this);
    }

    /// <summary>Najde balík podle handle. Vrací null pokud nenajde.</summary>
    public IForgeBlockPackage? GetPackage(string handle) =>
        _packages.TryGetValue(handle, out var package) ? package : null;

    /// <summary>Vyhledá balíky podle tagu.</summary>
    public IReadOnlyList<IForgeBlockPackage> SearchByTag(string tag) =>
        _packages.Values
            .Where(p => p.Discovery.Tags?.Contains(tag, StringComparer.OrdinalIgnoreCase) == true)
            .ToList()
            .AsReadOnly();

    /// <summary>Vrátí všechny capability napříč všemi balíky.</summary>
    public IReadOnlyList<ForgeBlockCapability> GetAllCapabilities() =>
        _packages.Values
            .SelectMany(p => p.Capabilities)
            .ToList()
            .AsReadOnly();
}
