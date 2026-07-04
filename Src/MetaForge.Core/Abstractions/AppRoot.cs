namespace MetaForge.Core.Abstractions;

/// <summary>
/// Vstupní bod dokumentu — obsahuje projekty.
/// Celková cena exportu = suma Coinů všech elementů.
/// </summary>
public sealed class AppRoot
{
    /// <summary>Seznam projektů v solution.</summary>
    public List<ProjectElement> Projects { get; } = new();

    /// <summary>Celková cena exportu v kreditech.</summary>
    public int TotalCoin =>
        Projects.Sum(p => p.RootElements.Sum(e => e.TotalCoin));
}
