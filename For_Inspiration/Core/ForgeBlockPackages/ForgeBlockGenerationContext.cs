using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;

namespace MetaForge.Core.ForgeBlockPackages;

public interface IForgeBlockGenerationContext
{
    ILanguageElement Element { get; }

    ProgramLanguage TargetLanguage { get; }

    IReadOnlyCollection<string> Imports { get; }

    IReadOnlyCollection<CodePackageDependency> Packages { get; }

    IReadOnlyCollection<GeneratedArtifactFile> AdditionalFiles { get; }

    IReadOnlyDictionary<string, string> Translations { get; }

    void AddImport(string value);

    void AddPackage(CodePackageDependency dependency);

    void AddSupportFile(string relativePath, string content, GeneratedArtifactFileKind kind = GeneratedArtifactFileKind.Supporting);

    void AddTranslation(string semanticId, string renderedCode);
}

/// <summary>
/// Neutralni kontext, do ktereho package contributori zapisuji importy, balicky a support files.
/// </summary>
public sealed class ForgeBlockGenerationContext : IForgeBlockGenerationContext
{
    private readonly HashSet<string> _imports = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CodePackageDependency> _packages = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, GeneratedArtifactFile> _additionalFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _translations = new(StringComparer.OrdinalIgnoreCase);

    public ForgeBlockGenerationContext(ILanguageElement element, ProgramLanguage targetLanguage)
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        TargetLanguage = targetLanguage;
    }

    public ILanguageElement Element { get; }

    public ProgramLanguage TargetLanguage { get; }

    public IReadOnlyCollection<string> Imports => _imports.OrderBy(value => value, StringComparer.Ordinal).ToArray();

    public IReadOnlyCollection<CodePackageDependency> Packages => _packages.Values
        .OrderBy(package => package.PackageManager, StringComparer.OrdinalIgnoreCase)
        .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public IReadOnlyCollection<GeneratedArtifactFile> AdditionalFiles => _additionalFiles.Values
        .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public IReadOnlyDictionary<string, string> Translations => _translations;

    public void AddImport(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            _imports.Add(value);
    }

    public void AddPackage(CodePackageDependency dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        if (string.IsNullOrWhiteSpace(dependency.PackageId))
            return;

        var key = $"{dependency.PackageManager}|{dependency.PackageId}";
        if (!_packages.TryGetValue(key, out var existing)
            || string.Compare(dependency.Version, existing.Version, StringComparison.OrdinalIgnoreCase) > 0)
        {
            _packages[key] = dependency;
        }
    }

    public void AddSupportFile(string relativePath, string content, GeneratedArtifactFileKind kind = GeneratedArtifactFileKind.Supporting)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var candidate = new GeneratedArtifactFile
        {
            RelativePath = relativePath,
            Content = content ?? string.Empty,
            Kind = kind
        };

        if (_additionalFiles.TryGetValue(relativePath, out var existing))
        {
            if (!string.Equals(existing.Content, candidate.Content, StringComparison.Ordinal)
                || existing.Kind != candidate.Kind)
            {
                throw new InvalidOperationException($"Conflicting ForgeBlock support file '{relativePath}'.");
            }

            return;
        }

        _additionalFiles[relativePath] = candidate;
    }

    public void AddTranslation(string semanticId, string renderedCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(semanticId);
        _translations[semanticId] = renderedCode ?? string.Empty;
    }

    public GeneratedCodeArtifact BuildArtifact()
    {
        return new GeneratedCodeArtifact
        {
            RequiredImports = Imports,
            RequiredPackages = Packages,
            AdditionalFiles = AdditionalFiles
        };
    }
}