using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Inference;

/// <summary>
/// Rozhraní pro constraint inferenci.
/// Používá se jak pro rule-based (vždy dostupný), tak pro AI-based detekci komplexních hraničních stavů.
/// Dříve: rozděleno na IConstraintInferencer (Abstractions) a IAiConstraintInferencer — sjednoceno 2026-03-29.
/// </summary>
public interface IConstraintInferencer
{
    /// <summary>
    /// Zda je inferencer dostupný (AI online, model načtený, ...).
    /// RuleBasedConstraintInferencer vrací vždy true.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Název použitého modelu. Pro rule-based vrací "rule-based".
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Provede inferenci constraintů pro metodu.
    /// </summary>
    /// <param name="method">Metoda k analýze.</param>
    /// <param name="existingConstraints">Již existující constrainty (z jiného inferenceru).</param>
    /// <param name="cancellationToken">Token pro zrušení.</param>
    /// <returns>Seznam constraintů popisujících neplatné kombinace vstupů.</returns>
    Task<IReadOnlyList<MethodConstraint>> InferConstraintsAsync(
        Method method,
        IReadOnlyList<MethodConstraint>? existingConstraints = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Provede detekci komplexních hraničních stavů pro kombinace parametrů.
    /// Není implementováno v RuleBasedConstraintInferencer — vrací prázdný seznam.
    /// </summary>
    /// <param name="method">Metoda k analýze.</param>
    /// <param name="cancellationToken">Token pro zrušení.</param>
    /// <returns>Seznam detekovaných hraničních stavů s popisem.</returns>
    Task<IReadOnlyList<BoundaryCase>> DetectComplexBoundariesAsync(
        Method method,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Otestuje dostupnost AI modelu.
    /// </summary>
    /// <param name="cancellationToken">Token pro zrušení.</param>
    /// <returns>True pokud model odpoví na testovací dotaz. Pro rule-based vždy true.</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Detekovaný hraniční stav z AI analýzy.
/// </summary>
public sealed record BoundaryCase(
    /// <summary>
    /// Název hraničního stavu.
    /// </summary>
    string Name,
    
    /// <summary>
    /// Podmínka která spouští tento stav.
    /// </summary>
    string Condition,
    
    /// <summary>
    /// Popis proč je toto hraniční stav.
    /// </summary>
    string Description,
    
    /// <summary>
    /// Závažnost (Info, Warning, Error, Critical).
    /// </summary>
    BoundarySeverity Severity,
    
    /// <summary>
    /// Očekávaná výjimka.
    /// </summary>
    string? ExpectedException = null,
    
    /// <summary>
    /// Doporučená guard clause.
    /// </summary>
    string? RecommendedGuard = null,
    
    /// <summary>
    /// Parametry které se podílejí na tomto hraničním stavu.
    /// </summary>
    IReadOnlyList<string>? InvolvedParameters = null,
    
    /// <summary>
    /// Důvěra AI v tento odhad (0.0 - 1.0).
    /// </summary>
    float Confidence = 0.8f
);

/// <summary>
/// Závažnost hraničního stavu.
/// </summary>
public enum BoundarySeverity
{
    /// <summary>
    /// FYI — metoda stále funguje korektně.
    /// </summary>
    Info,
    
    /// <summary>
    /// Může způsobit neočekávané chování.
    /// </summary>
    Warning,
    
    /// <summary>
    /// Způsobí výjimku.
    /// </summary>
    Error,
    
    /// <summary>
    /// Bezpečnostní riziko nebo crash.
    /// </summary>
    Critical
}
