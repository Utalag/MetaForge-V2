// ---------------------------------------------------------------------------
// MetaForge.Infrastructure — VerificationState
// Verification states and records for element contracts.
// Vrstva: Infrastructure / Verification
//
// PROPOSAL: PROP-057 — ElementContract + VerificationModel
// ---------------------------------------------------------------------------

namespace MetaForge.Infrastructure.Verification;

/// <summary>Stav verifikace elementu.</summary>
public enum VerificationState
{
    Unknown,
    Running,
    Passed,
    Failed,
    Stale,
}

/// <summary>Záznam o verifikaci elementu.</summary>
public sealed record VerificationRecord
{
    /// <summary>ID elementu (PROP-060).</summary>
    public string ElementId { get; init; } = string.Empty;

    /// <summary>Fingerprint v době poslední verifikace.</summary>
    public string Fingerprint { get; init; } = string.Empty;

    /// <summary>Aktuální stav.</summary>
    public VerificationState State { get; init; }

    /// <summary>Kdy byla poslední verifikace.</summary>
    public DateTimeOffset LastVerified { get; init; }

    /// <summary>Diagnostika selhání (pokud Failed).</summary>
    public string? FailureDiagnostics { get; init; }

    /// <summary>Snapshot názvu elementu v době verifikace (debug-only).</summary>
    public string? DisplayNameSnapshot { get; init; }
}
