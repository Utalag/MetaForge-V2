namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Identifikuje, kdo command vydal — uživatel, AI agent, nebo systém.
/// </summary>
public sealed class CommandIssuedBy
{
    /// <summary>Typ aktéra: "user", "ai", "system".</summary>
    public string ActorType { get; init; } = "user";

    /// <summary>Unikátní identifikátor aktéra.</summary>
    public string? ActorId { get; init; }

    /// <summary>Čitelné jméno aktéra (např. "Jan Novák", "Copilot").</summary>
    public string? DisplayName { get; init; }
}
