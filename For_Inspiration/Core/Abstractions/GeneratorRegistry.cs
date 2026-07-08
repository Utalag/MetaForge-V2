using MetaForge.Core.Common;
using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.Core.Abstractions;

/// <summary>
/// Centrální registr pro všechny dostupné generátory kódu.
/// Umožňuje runtime registraci a vyhledávání generátorů podle jazyka.
/// </summary>
public class GeneratorRegistry
{
    private readonly Dictionary<ProgramLanguage, ICodeGenerator> _generators = new();
    private ForgeBlockPackageRegistry? _forgeBlockPackages;

    /// <summary>
    /// Event vyvolaný při registraci nového generátoru.
    /// </summary>
    public event EventHandler<GeneratorRegisteredEventArgs>? GeneratorRegistered;

    /// <summary>
    /// Registruje generátor pro daný jazyk.
    /// Pokud generátor pro jazyk již existuje, bude nahrazen.
    /// </summary>
    /// <param name="generator">Instance generátoru k registraci.</param>
    public void Register(ICodeGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);

        if (generator is IForgeBlockPackageRegistryAware packageRegistryAware)
        {
            if (_forgeBlockPackages == null)
            {
                _forgeBlockPackages = packageRegistryAware.ForgeBlockPackageRegistry;
            }
            else
            {
                _forgeBlockPackages.MergeFrom(packageRegistryAware.ForgeBlockPackageRegistry);
                packageRegistryAware.UseForgeBlockPackageRegistry(_forgeBlockPackages);
            }
        }

        _generators[generator.Language] = generator;
        
        GeneratorRegistered?.Invoke(this, new GeneratorRegisteredEventArgs(generator));
    }

    /// <summary>
    /// Odregistruje generátor pro daný jazyk.
    /// </summary>
    /// <param name="language">Jazyk k odregistrování.</param>
    /// <returns>True pokud byl generátor odstraněn, jinak false.</returns>
    public bool Unregister(ProgramLanguage language)
    {
        return _generators.Remove(language);
    }

    /// <summary>
    /// Vygeneruje kód pro daný element v daném jazyce.
    /// </summary>
    /// <param name="element">Element k vygenerování.</param>
    /// <param name="language">Cílový programovací jazyk.</param>
    /// <returns>Vygenerovaný kód.</returns>
    /// <exception cref="InvalidOperationException">Pokud generátor pro daný jazyk není registrován.</exception>
    public string Generate(ILanguageElement element, ProgramLanguage language)
    {
        return GenerateArtifact(element, language).Code;
    }

    /// <summary>
    /// Vygeneruje artefakt pro daný element včetně metadat o importech a balíčcích.
    /// </summary>
    public GeneratedCodeArtifact GenerateArtifact(ILanguageElement element, ProgramLanguage language)
    {
        if (!_generators.TryGetValue(language, out var generator))
        {
            throw new InvalidOperationException(
                $"No generator registered for language '{language}'. " +
                $"Available languages: {string.Join(", ", _generators.Keys)}");
        }

        return MergeArtifacts(generator.GenerateArtifact(element), ForgeBlockPackages.BuildArtifact(element, language));
    }

    /// <summary>
    /// Registruje ForgeBlock package do sdilene package registry.
    /// </summary>
    [Obsolete("Use ForgeBlockPackageRegistry.Register() directly.")]
    public void RegisterForgeBlockPackage(IForgeBlockPackage package)
    {
        ForgeBlockPackages.Register(package);
    }

    /// <summary>
    /// Vypočítá náklady v kreditech za vygenerování elementu v daném jazyce.
    /// </summary>
    /// <param name="element">Element k vygenerování.</param>
    /// <param name="language">Cílový programovací jazyk.</param>
    /// <returns>Počet kreditů.</returns>
    public int CalculateCredits(ILanguageElement element, ProgramLanguage language)
    {
        if (!_generators.TryGetValue(language, out var generator))
        {
            throw new InvalidOperationException($"No generator registered for language '{language}'.");
        }

        // Základní výpočet: 1 kredit za element * násobek jazyka
        return generator.CreditCostPerElement;
    }

    /// <summary>
    /// Vypočítá celkové náklady v kreditech za vygenerování kolekce elementů.
    /// </summary>
    /// <param name="elements">Kolekce elementů k vygenerování.</param>
    /// <param name="language">Cílový programovací jazyk.</param>
    /// <returns>Celkový počet kreditů.</returns>
    public int CalculateTotalCredits(IEnumerable<ILanguageElement> elements, ProgramLanguage language)
    {
        if (!_generators.TryGetValue(language, out var generator))
        {
            throw new InvalidOperationException($"No generator registered for language '{language}'.");
        }

        return elements.Count() * generator.CreditCostPerElement;
    }

    /// <summary>
    /// Zjistí, zda je generátor pro daný jazyk registrován.
    /// </summary>
    /// <param name="language">Jazyk k ověření.</param>
    /// <returns>True pokud je generátor registrován.</returns>
    public bool IsRegistered(ProgramLanguage language)
    {
        return _generators.ContainsKey(language);
    }

    /// <summary>
    /// Vrátí generátor pro daný jazyk, nebo null pokud není registrován.
    /// </summary>
    /// <param name="language">Požadovaný jazyk.</param>
    /// <returns>Instance generátoru nebo null.</returns>
    public ICodeGenerator? GetGenerator(ProgramLanguage language)
    {
        return _generators.GetValueOrDefault(language);
    }

    /// <summary>
    /// Vrati sdilenou registry ForgeBlock package kontraktu a contributoru.
    /// </summary>
    public ForgeBlockPackageRegistry ForgeBlockPackages => _forgeBlockPackages ??= new ForgeBlockPackageRegistry();

    /// <summary>
    /// Vrátí kolekci všech registrovaných jazyků.
    /// </summary>
    public IReadOnlyCollection<ProgramLanguage> RegisteredLanguages => _generators.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Vrátí kolekci všech registrovaných generátorů.
    /// </summary>
    public IReadOnlyCollection<ICodeGenerator> RegisteredGenerators => _generators.Values.ToList().AsReadOnly();

    /// <summary>
    /// Vrati ID vsech registrovanych ForgeBlock balicku.
    /// </summary>
    public IReadOnlyCollection<string> RegisteredForgeBlockPackages => _forgeBlockPackages?.GetRegisteredPackageIds() ?? Array.Empty<string>();

    /// <summary>
    /// Vrati capability discovery metadata vsech registrovanych ForgeBlock balicku.
    /// </summary>
    public IReadOnlyCollection<ForgeBlockCapabilityDescriptor> RegisteredForgeBlockCapabilities => ForgeBlockPackages.GetCapabilities();

    /// <summary>
    /// Vrati katalogove discovery zaznamy registrovanych ForgeBlock balicku.
    /// </summary>
    public IReadOnlyCollection<ForgeBlockCatalogEntryDescriptor> RegisteredForgeBlockCatalogEntries => ForgeBlockPackages.GetCatalogEntries();

    /// <summary>
    /// Vrati capability pro konkretni ForgeBlock balicek.
    /// </summary>
    public IReadOnlyCollection<ForgeBlockCapabilityDescriptor> GetForgeBlockCapabilities(string packageId)
        => ForgeBlockPackages.GetCapabilities(packageId);

    /// <summary>
    /// Vrati katalogove zaznamy konkretniho ForgeBlock balicku.
    /// </summary>
    public IReadOnlyCollection<ForgeBlockCatalogEntryDescriptor> GetForgeBlockCatalogEntries(string packageId)
        => ForgeBlockPackages.GetCatalogEntries(packageId);

    /// <summary>
    /// Vrátí počet registrovaných generátorů.
    /// </summary>
    public int Count => _generators.Count;

    private static GeneratedCodeArtifact MergeArtifacts(GeneratedCodeArtifact primary, GeneratedCodeArtifact secondary)
    {
        var packages = primary.RequiredPackages
            .Concat(secondary.RequiredPackages)
            .Where(package => !string.IsNullOrWhiteSpace(package.PackageId))
            .GroupBy(package => $"{package.PackageManager}|{package.PackageId}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(package => package.Version, StringComparer.OrdinalIgnoreCase)
                .First())
            .OrderBy(package => package.PackageManager, StringComparer.OrdinalIgnoreCase)
            .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new GeneratedCodeArtifact
        {
            Code = string.Join(Environment.NewLine + Environment.NewLine,
                new[] { primary.Code, secondary.Code }.Where(code => !string.IsNullOrWhiteSpace(code))),
            RequiredImports = primary.RequiredImports
                .Concat(secondary.RequiredImports)
                .Where(@using => !string.IsNullOrWhiteSpace(@using))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(@using => @using, StringComparer.Ordinal)
                .ToArray(),
            RequiredPackages = packages,
            AdditionalFiles = MergeFiles(primary.AdditionalFiles.Concat(secondary.AdditionalFiles))
        };
    }

    private static IReadOnlyCollection<GeneratedArtifactFile> MergeFiles(IEnumerable<GeneratedArtifactFile> files)
    {
        var merged = new Dictionary<string, GeneratedArtifactFile>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files.Where(file => !string.IsNullOrWhiteSpace(file.RelativePath)))
        {
            if (merged.TryGetValue(file.RelativePath, out var existing))
            {
                if (!string.Equals(existing.Content, file.Content, StringComparison.Ordinal)
                    || existing.Kind != file.Kind)
                {
                    throw new InvalidOperationException($"Conflicting generated artifact file '{file.RelativePath}'.");
                }

                continue;
            }

            merged[file.RelativePath] = file;
        }

        return merged.Values
            .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

/// <summary>
/// Event args pro událost registrace generátoru.
/// </summary>
public class GeneratorRegisteredEventArgs : EventArgs
{
    public ICodeGenerator Generator { get; }

    public GeneratorRegisteredEventArgs(ICodeGenerator generator)
    {
        Generator = generator;
    }
}
