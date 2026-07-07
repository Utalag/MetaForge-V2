namespace MetaForge.Core.Abstractions;

/// <summary>
/// Reprezentuje jeden projekt v solution.
/// Obsahuje RootElementy (třídy, interfacy, enumy, struktury).
/// </summary>
public sealed class ProjectElement
{
    /// <summary>Název projektu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Výchozí namespace projektu.</summary>
    public string? DefaultNamespace { get; set; }

    /// <summary>
    /// Cílový framework projektu (např. "net10.0", "net9.0", "netstandard2.1").
    /// Odpovídá `&lt;TargetFramework&gt;` v .csproj.
    /// </summary>
    public string? TargetFramework { get; set; }

    /// <summary>NuGet balíčkové reference projektu.</summary>
    public List<PackageReferenceInfo> PackageReferences { get; } = new();

    /// <summary>Reference na Roslyn analyzery / source generátory.</summary>
    public List<AnalyzerReferenceInfo> AnalyzerReferences { get; } = new();

    /// <summary>Reference na jiné projekty v solution (multi-project vztahy).</summary>
    public List<ProjectReferenceInfo> ProjectReferences { get; } = new();

    /// <summary>Je povoleno nullable reference types (`&lt;Nullable&gt;enable&lt;/Nullable&gt;`)?</summary>
    public bool NullableEnabled { get; set; } = true;

    /// <summary>Je povoleno `&lt;ImplicitUsings&gt;`?</summary>
    public bool ImplicitUsingsEnabled { get; set; } = true;

    /// <summary>Top-level elementy v projektu.</summary>
    public List<RootElement> RootElements { get; } = new();

    /// <summary>Přidá NuGet balíčkovou referenci a vrátí this (fluent).</summary>
    public ProjectElement WithPackageReference(string name, string version)
    {
        PackageReferences.Add(new PackageReferenceInfo(name, version));
        return this;
    }

    /// <summary>Přidá analyzer referenci a vrátí this (fluent).</summary>
    public ProjectElement WithAnalyzerReference(string name, string version)
    {
        AnalyzerReferences.Add(new AnalyzerReferenceInfo(name, version));
        return this;
    }

    /// <summary>Přidá referenci na jiný projekt v solution a vrátí this (fluent).</summary>
    public ProjectElement WithProjectReference(string projectName)
    {
        ProjectReferences.Add(new ProjectReferenceInfo(projectName));
        return this;
    }
}
