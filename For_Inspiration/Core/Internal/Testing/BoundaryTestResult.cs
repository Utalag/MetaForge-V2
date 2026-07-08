using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Internal.Testing;

/// <summary>
/// Výsledek boundary testu - BEZ C# KÓDU.
/// Interní komponenta, nikdy neexponuje C# kód uživateli.
/// </summary>
public sealed class BoundaryTestResult
{
    /// <summary>Identifikátor metody.</summary>
    public string MethodId { get; init; } = "";
    
    /// <summary>Verze metody (pro cache invalidaci).</summary>
    public int Version { get; init; }
    
    /// <summary>Detekované hraniční stavy z testů.</summary>
    public IReadOnlyList<DetectedBoundary> Boundaries { get; init; } = Array.Empty<DetectedBoundary>();
    
    /// <summary>Kolik testů prošlo.</summary>
    public int PassedCount { get; init; }
    
    /// <summary>Kolik testů selhalo (→ nové constraints).</summary>
    public int FailedCount { get; init; }
    
    /// <summary>Čas testování.</summary>
    public TimeSpan Duration { get; init; }
    
    /// <summary>Hash výsledků (pro cache invalidaci).</summary>
    public string ResultHash { get; init; } = "";
}

/// <summary>
/// Detekovaný hraniční stav z testů.
/// </summary>
public sealed record DetectedBoundary(
    /// <summary>Podmínka která selhala.</summary>
    string Condition,
    
    /// <summary>Typ výjimky která byla hozena.</summary>
    string ExceptionType,
    
    /// <summary>Popis co se stalo.</summary>
    string Description,
    
    /// <summary>Závažnost.</summary>
    BoundaryTestSeverity Severity
);

/// <summary>
/// Závažnost detekovaného hraničního stavu.
/// </summary>
public enum BoundaryTestSeverity
{
    Info,
    Warning,
    Error,
    Critical
}
