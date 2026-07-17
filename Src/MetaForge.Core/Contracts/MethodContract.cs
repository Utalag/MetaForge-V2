// ---------------------------------------------------------------------------
// MetaForge.Core — MethodContract
// Semantic contract for methods (MethodElement).
// Vrstva: Core / Contracts
//
// PROPOSAL: PROP-057 — ElementContract + VerificationModel
// ---------------------------------------------------------------------------

namespace MetaForge.Core.Contracts;

/// <summary>
/// Kontrakt pro metodu (MethodElement).
/// ⚠️ NEObsahuje validace parametrů! Parametry dědí validaci ze svých typů (StrongType).
/// Nese jen cross-parameter invarianty, output očekávání, side effects, scénářové hinty.
/// </summary>
public sealed record MethodContract : ElementContract
{
    /// <summary>Očekávání na výstup metody.</summary>
    public OutputExpectation? OutputContract { get; init; }

    /// <summary>Side efekty — co metoda dělá vedle svého výstupu.</summary>
    public IReadOnlyList<SideEffectHint> SideEffects { get; init; } = [];

    /// <summary>Scénářové hinty pro sandbox (PROP-058).</summary>
    public IReadOnlyList<ScenarioHint> ScenarioHints { get; init; } = [];
}

/// <summary>
/// Očekávání na výstup metody — postcondition.
/// </summary>
public record OutputExpectation
{
    public string? Description { get; init; }
}

/// <summary>Druh side efektu metody.</summary>
public enum SideEffectKind
{
    None,
    DatabaseWrite,
    FileWrite,
    NetworkCall,
    StateMutation,
}

/// <summary>Hint o side efektu metody.</summary>
public record SideEffectHint
{
    public SideEffectKind Kind { get; init; }
    public string? Description { get; init; }
}

/// <summary>Scénářový hint pro sandbox (PROP-058).</summary>
public record ScenarioHint
{
    public string Name { get; init; } = string.Empty;
    public ScenarioKind Kind { get; init; }
    public string Description { get; init; } = string.Empty;
}

/// <summary>Druh scénáře.</summary>
public enum ScenarioKind
{
    HappyPath,
    Boundary,
    ErrorCase,
    EdgeCase,
}
