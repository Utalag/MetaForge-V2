// ---------------------------------------------------------------------------
// MetaForge.Infrastructure — IVerificationStateStore
// Persistent store for verification results.
// Vrstva: Infrastructure / Verification
//
// PROPOSAL: PROP-057 — ElementContract + VerificationModel
// ---------------------------------------------------------------------------

namespace MetaForge.Infrastructure.Verification;

/// <summary>
/// Persistent storage for verification results of element contracts.
/// Pattern: checkpoint caching (podobně jako CheckpointProjectionCache v Infrastructure).
/// Implementace: InMemoryVerificationStateStore (pro testy), FileBasedVerificationStateStore (produkce).
/// </summary>
public interface IVerificationStateStore
{
    /// <summary>Získá stav verifikace pro element. Vrací null pokud není.</summary>
    Task<VerificationRecord?> GetAsync(string elementId, CancellationToken ct = default);

    /// <summary>Uloží výsledek verifikace.</summary>
    Task SetAsync(VerificationRecord record, CancellationToken ct = default);

    /// <summary>Zneplatní cached výsledek (např. po změně elementu).</summary>
    Task InvalidateAsync(string elementId, string reason, CancellationToken ct = default);
}

/// <summary>
/// In-memory implementace pro testy a prototyping.
/// </summary>
public sealed class InMemoryVerificationStateStore : IVerificationStateStore
{
    private readonly Dictionary<string, VerificationRecord> _store = new();

    public Task<VerificationRecord?> GetAsync(string elementId, CancellationToken ct = default)
    {
        _store.TryGetValue(elementId, out var record);
        return Task.FromResult(record);
    }

    public Task SetAsync(VerificationRecord record, CancellationToken ct = default)
    {
        _store[record.ElementId] = record;
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(string elementId, string reason, CancellationToken ct = default)
    {
        if (_store.TryGetValue(elementId, out var existing))
        {
            _store[elementId] = existing with
            {
                State = VerificationState.Stale,
                FailureDiagnostics = reason,
                LastVerified = DateTimeOffset.UtcNow,
            };
        }
        return Task.CompletedTask;
    }
}
