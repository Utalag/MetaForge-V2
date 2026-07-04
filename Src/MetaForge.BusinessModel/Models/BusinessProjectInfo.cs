namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Strukturované informace o projektu.
/// Nahrazuje jednoduchý string ProjectName.
/// </summary>
public sealed record BusinessProjectInfo
{
    /// <summary>Unikátní identifikátor projektu (slug).</summary>
    public string Id { get; init; } = "new-project";

    /// <summary>Čitelný název projektu.</summary>
    public string Name { get; init; } = "NewProject";

    /// <summary>Popis projektu.</summary>
    public string? Description { get; init; }

    /// <summary>Ikona / emoji projektu.</summary>
    public string? Icon { get; init; }

    /// <summary>Verze projektu (inkrementální).</summary>
    public int Version { get; init; } = 1;
}
