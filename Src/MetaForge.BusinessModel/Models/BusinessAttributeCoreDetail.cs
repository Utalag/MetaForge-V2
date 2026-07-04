using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Core-konkretizovaná data pro business atribut.
/// Ukládá výstup z Translatoru / AI enrichmentu vedle uživatelského vstupu.
/// </summary>
public sealed record BusinessAttributeCoreDetail
{
    /// <summary>Původ informace — ruční, generovaná, nebo hybridní.</summary>
    public CoreInfoSource Source { get; init; } = CoreInfoSource.Unknown;

    /// <summary>ID rozpoznaného presetu z katalogu (např. "email", "phone").</summary>
    public string? ResolvedPresetId { get; init; }

    /// <summary>Název value objectu, pokud byl rozpoznán (např. "EmailAddress").</summary>
    public string? ValueObjectName { get; init; }

    /// <summary>Zda byl typ rozpoznán jako strong type (value object).</summary>
    public bool IsStrongType { get; init; }

    /// <summary>Čas poslední synchronizace s Core vrstvou.</summary>
    public DateTimeOffset? LastSyncedAt { get; init; }

    /// <summary>
    /// Stav synchronizace mezi business a core vrstvou.
    /// Není serializován — počítá se za běhu.
    /// </summary>
    [JsonIgnore]
    public AttributeSyncState SyncState { get; set; } = AttributeSyncState.New;
}
