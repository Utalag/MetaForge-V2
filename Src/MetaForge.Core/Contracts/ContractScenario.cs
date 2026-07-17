// ---------------------------------------------------------------------------
// MetaForge.Core — ContractScenario
// Unified scenario definition for entities and methods.
// Vrstva: Core / Contracts
//
// PROPOSAL: PROP-057 — ElementContract + VerificationModel
// ---------------------------------------------------------------------------

namespace MetaForge.Core.Contracts;

/// <summary>
/// Unifikovaný scénář — slouží pro entity (CanonicalExample) i metody (VerificationScenario).
/// Stejná struktura, jiný sémantický kontext.
/// 
/// ⚠️ Klíče v InputsByElementId jsou stabilní ElementId (PROP-060), ne jména.
/// DisplayNameSnapshots jsou debug-only, neautoritativní.
/// </summary>
public sealed record ContractScenario
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }

    /// <summary>
    /// Hodnoty podle stabilního ElementId (PropertyId / ParameterId).
    /// Klíč = ID, ne jméno. Jméno je v DisplayNameSnapshots.
    /// </summary>
    public IReadOnlyDictionary<string, ContractValue> InputsByElementId { get; init; }
        = new Dictionary<string, ContractValue>();

    /// <summary>Debug-only snapshot jmen v čase vytvoření scénáře.</summary>
    public IReadOnlyDictionary<string, string> DisplayNameSnapshots { get; init; }
        = new Dictionary<string, string>();

    /// <summary>Očekávání — jak má scénář dopadnout.</summary>
    public ScenarioExpectation Expectation { get; init; } = new();
}

/// <summary>
/// Očekávání na výsledek scénáře.
/// </summary>
public sealed record ScenarioExpectation
{
    /// <summary>True = scénář by měl projít validací / sandboxem.</summary>
    public bool ShouldSucceed { get; init; } = true;

    /// <summary>Očekávaná výjimka (např. "ArgumentException").</summary>
    public string? ExpectedException { get; init; }

    /// <summary>Očekávaná návratová hodnota.</summary>
    public ContractValue? ExpectedReturnValue { get; init; }

    /// <summary>Očekávané diagnostické kódy (např. "REF001").</summary>
    public IReadOnlyList<string> ExpectedDiagnostics { get; init; } = [];
}
