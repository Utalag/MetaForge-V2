using System.Security;
using System.Text;

namespace MetaForge.Generators.Packaging;

/// <summary>
/// Generuje NuGet package manifest (.props soubor) s PackageReference elementy.
/// </summary>
public sealed class PackageManifestGenerator : IPackageManifestGenerator
{
    private const string ManifestFileName = "MetaForge.Generated.PackageReferences.props";

    public IReadOnlyCollection<GeneratedArtifactFile> GenerateFiles(IReadOnlyCollection<CodePackageDependency> packages)
    {
        var nugetPackages = packages
            .Where(p => string.IsNullOrWhiteSpace(p.PackageManager)
                || string.Equals(p.PackageManager, "NuGet", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.PackageId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (nugetPackages.Length == 0)
            return Array.Empty<GeneratedArtifactFile>();

        var sb = new StringBuilder();
        sb.AppendLine("<Project>");
        sb.AppendLine("  <ItemGroup>");

        foreach (var package in nugetPackages)
        {
            var packageId = SecurityElement.Escape(package.PackageId) ?? package.PackageId;
            var version = SecurityElement.Escape(package.Version) ?? package.Version;

            if (string.IsNullOrWhiteSpace(version))
                sb.AppendLine($"    <PackageReference Include=\"{packageId}\" />");
            else
                sb.AppendLine($"    <PackageReference Include=\"{packageId}\" Version=\"{version}\" />");
        }

        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine("</Project>");

        return new[]
        {
            new GeneratedArtifactFile(
                RelativePath: ManifestFileName,
                Content: sb.ToString().TrimEnd(),
                Kind: GeneratedArtifactFileKind.PackageManifest
            )
        };
    }
}
