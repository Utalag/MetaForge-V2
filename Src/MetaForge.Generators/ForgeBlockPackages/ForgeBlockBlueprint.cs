namespace MetaForge.Generators.ForgeBlockPackages;

/// <summary>
/// Blueprint pro ForgeBlock — deskriptor capability s metadaty pro katalog a marketplace.
/// PROP-017: ForgeBlock Packaging.
/// </summary>
public sealed record ForgeBlockBlueprint
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<string> SemanticHandles { get; init; } = [];
    public IReadOnlyList<string> SupportedLanguages { get; init; } = ["C#"];
    public IReadOnlyList<BlueprintDependency> Dependencies { get; init; } = [];
    public IReadOnlyList<BlueprintNuGetPackage> NuGetPackages { get; init; } = [];
    public IReadOnlyList<BlueprintUsing> AdditionalUsings { get; init; } = [];
}

/// <summary>
/// Builder pro ForgeBlockBlueprint — fluent API.
/// </summary>
public sealed class BlueprintBuilder
{
    private readonly List<BlueprintDependency> _dependencies = [];
    private readonly List<BlueprintNuGetPackage> _nuGetPackages = [];
    private readonly List<BlueprintUsing> _additionalUsings = [];
    private string _id;
    private string _displayName;
    private string _description = string.Empty;
    private string _category = string.Empty;
    private string _kind = string.Empty;
    private IReadOnlyList<string> _tags = [];
    private IReadOnlyList<string> _semanticHandles = [];

    public BlueprintBuilder(string id, string displayName)
    {
        _id = id;
        _displayName = displayName;
    }

    public BlueprintBuilder WithDescription(string description) { _description = description; return this; }
    public BlueprintBuilder WithCategory(string category) { _category = category; return this; }
    public BlueprintBuilder WithKind(string kind) { _kind = kind; return this; }

    public BlueprintBuilder WithTags(params string[] tags) { _tags = tags; return this; }
    public BlueprintBuilder WithSemanticHandles(params string[] handles) { _semanticHandles = handles; return this; }

    public BlueprintBuilder WithNuGetPackage(string packageId, string version)
    {
        _nuGetPackages.Add(new BlueprintNuGetPackage(packageId, version));
        return this;
    }

    public BlueprintBuilder WithUsing(string usingStatement)
    {
        _additionalUsings.Add(new BlueprintUsing(usingStatement));
        return this;
    }

    public BlueprintBuilder WithDependency(string handle, string version)
    {
        _dependencies.Add(new BlueprintDependency(handle, version));
        return this;
    }

    public ForgeBlockBlueprint Build() => new()
    {
        Id = _id,
        DisplayName = _displayName,
        Description = _description,
        Category = _category,
        Kind = _kind,
        Tags = _tags,
        SemanticHandles = _semanticHandles,
        Dependencies = _dependencies,
        NuGetPackages = _nuGetPackages,
        AdditionalUsings = _additionalUsings,
    };
}

/// <summary>
/// Závislost na jiném ForgeBlocku.
/// </summary>
public sealed record BlueprintDependency(string Handle, string Version);

/// <summary>
/// NuGet balíček vyžadovaný ForgeBlockem.
/// </summary>
public sealed record BlueprintNuGetPackage(string PackageId, string Version);

/// <summary>
/// Using statement přidaný do generovaného kódu.
/// </summary>
public sealed record BlueprintUsing(string Namespace);
