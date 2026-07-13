using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.Generators.ForgeBlockPackages;

/// <summary>
/// Integruje ForgeBlock capability do generovaného kódu.
/// Prohledá Core elementy, najde matching ForgeBlock capability a přidá
/// vyžadované NuGet balíčky a usingy do GeneratedCodeArtifact.
/// PROP-017: ForgeBlock Packaging.
/// </summary>
public sealed class ForgeBlockPackageIntegrator
{
    private readonly ForgeBlockRegistry _registry;

    public ForgeBlockPackageIntegrator(ForgeBlockRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Obohať vygenerovaný artefakt o balíčky a usingy z ForgeBlocků,
    /// které odpovídají použitým capability v elementu.
    /// </summary>
    public GeneratedCodeArtifact Enrich(GeneratedCodeArtifact artifact, MetaForge.Core.Abstractions.RootElement element)
    {
        var packages = new List<CodePackageDependency>();
        var usings = new List<string>();

        // Prohledáme capabilities v registru, které matchují element
        foreach (var capability in _registry.GetAllCapabilities())
        {
            // Heuristika: capability tagy matchují element nebo property names
            var elementName = element.Name;
            if (MatchesElement(capability, elementName))
            {
                // Přidáme známé NuGet balíčky pro capability
                AddKnownPackage(capability, packages, usings);
            }
        }

        return packages.Count > 0 || usings.Count > 0
            ? artifact with
            {
                RequiredPackages = packages.AsReadOnly(),
                RequiredUsings = usings.AsReadOnly()
            }
            : artifact;
    }

    private static bool MatchesElement(ForgeBlockCapability capability, string elementName)
    {
        return capability.Tags.Any(t =>
            elementName.Contains(t, StringComparison.OrdinalIgnoreCase) ||
            t.Contains(elementName, StringComparison.OrdinalIgnoreCase));
    }

    private static void AddKnownPackage(ForgeBlockCapability capability,
        List<CodePackageDependency> packages, List<string> usings)
    {
        foreach (var tag in capability.Tags)
        {
            switch (tag.ToLowerInvariant())
            {
                case "ef-core" or "orm":
                    packages.Add(new CodePackageDependency("Microsoft.EntityFrameworkCore", "9.0.0"));
                    usings.Add("Microsoft.EntityFrameworkCore");
                    break;
                case "automapper" or "mapping":
                    packages.Add(new CodePackageDependency("AutoMapper", "13.0.0"));
                    usings.Add("AutoMapper");
                    break;
                case "fluent" or "validation" or "validator":
                    packages.Add(new CodePackageDependency("FluentValidation", "11.0.0"));
                    usings.Add("FluentValidation");
                    break;
            }
        }
    }
}
