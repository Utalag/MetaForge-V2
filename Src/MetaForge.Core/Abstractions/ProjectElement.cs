namespace MetaForge.Core.Abstractions;

/// <summary>
/// Reprezentuje jeden projekt v solution.
/// Obsahuje RootElementy (třídy, interfacy, enumy, struktury, delegáty).
/// </summary>
public sealed class ProjectElement
{
    /// <summary>Název projektu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Výchozí namespace projektu.</summary>
    public string? DefaultNamespace { get; set; }

    /// <summary>Target framework (např. "net10.0", "net9.0").</summary>
    public string? TargetFramework { get; set; }

    /// <summary>Root namespace pro generování kódu.</summary>
    public string? RootNamespace { get; set; }

    /// <summary>Zda je nullable enabled (výchozí true pro moderní projekty).</summary>
    public bool NullableEnabled { get; set; } = true;

    /// <summary>Implicitní using direktivy (např. "System", "System.Linq").</summary>
    public List<string> ImplicitUsings { get; init; } = new();

    /// <summary>NuGet package reference.</summary>
    public List<PackageReference> PackageReferences { get; init; } = new();

    /// <summary>Analyzer reference.</summary>
    public List<AnalyzerReference> AnalyzerReferences { get; init; } = new();

    /// <summary>Project-to-project references.</summary>
    public List<ProjectReference> ProjectReferences { get; init; } = new();

    /// <summary>Top-level elementy v projektu.</summary>
    public List<RootElement> RootElements { get; } = new();
}

/// <summary>
/// NuGet package reference.
/// </summary>
public sealed record PackageReference(string Name, string Version);

/// <summary>
/// Analyzer / source generator reference.
/// </summary>
public sealed record AnalyzerReference(string Name, string Path);

/// <summary>
/// Project-to-project reference.
/// </summary>
public sealed record ProjectReference(string Name, string RelativePath);
