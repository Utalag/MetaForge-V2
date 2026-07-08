namespace MetaForge.Core.Catalog;

/// <summary>
/// JSON model pro ForgeBlock preset.
/// Deserializuje se z .block.json souboru.
/// </summary>
public class ForgeBlockPreset
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = "MetaForge";
    public string Icon { get; set; } = "🧱";
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public int CreditCost { get; set; }

    /// <summary>NuGet závislosti.</summary>
    public List<NuGetDependencyPreset> NuGetDependencies { get; set; } = new();

    /// <summary>Konfigurovatelné volby.</summary>
    public Dictionary<string, PresetParameter> Configuration { get; set; } = new();

    /// <summary>Reference na Value Object presety.</summary>
    public List<ValueObjectRefPreset> ValueObjectRefs { get; set; } = new();

    /// <summary>Další generované soubory.</summary>
    public List<AdditionalFilePreset> AdditionalFiles { get; set; } = new();
}

/// <summary>
/// NuGet závislost v JSON presetu.
/// </summary>
public class NuGetDependencyPreset
{
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}

/// <summary>
/// Reference na Value Object preset v ForgeBlock presetu.
/// </summary>
public class ValueObjectRefPreset
{
    public string PresetId { get; set; } = string.Empty;
    public string? Condition { get; set; }
}

/// <summary>
/// Další generovaný soubor v ForgeBlock presetu.
/// </summary>
public class AdditionalFilePreset
{
    public string TemplateId { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string Category { get; set; } = "Domain";
}
