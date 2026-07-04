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

    /// <summary>Top-level elementy v projektu.</summary>
    public List<RootElement> RootElements { get; } = new();
}
