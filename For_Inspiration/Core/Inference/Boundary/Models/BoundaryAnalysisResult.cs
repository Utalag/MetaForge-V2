using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Inference.Boundary.Models;

/// <summary>
/// Výsledek boundary analýzy pro jednu metodu.
/// </summary>
public sealed record BoundaryAnalysisResult(
    /// <summary>Název metody, která byla analyzována.</summary>
    string MethodName,
    
    /// <summary>Seznam odhalených hraničních stavů.</summary>
    IReadOnlyList<MethodConstraint> Constraints,
    
    /// <summary>Detekovaná doména metody (math, string, collection, generic).</summary>
    string DetectedDomain,
    
    /// <summary>Zpráva o analýze (info, varování, chyby).</summary>
    string AnalysisSummary,
    
    /// <summary>Časová náročnost analýzy.</summary>
    TimeSpan AnalysisDuration
);

/// <summary>
/// Závažnost hraničního stavu.
/// </summary>
public enum BoundarySeverity
{
    /// <summary>FYI — metoda stále funguje korektně.</summary>
    Info,
    
    /// <summary>Může způsobit neočekávané chování.</summary>
    Warning,
    
    /// <summary>Způsobí výjimku.</summary>
    Error,
    
    /// <summary>Bezpečnostní riziko nebo crash.</summary>
    Critical
}
