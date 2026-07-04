namespace MetaForge.Translator;

/// <summary>
/// Vysledek <see cref="BusinessAuthoringHostFacade.AssistNodeAsync"/> — strukturovany preview bez auto-apply.
/// </summary>
public sealed class NodeAssistProposal
{
    /// <summary>True pokud se podarilo sestavit node-scoped kontext.</summary>
    public bool Success { get; init; }

    /// <summary>Duvod selhani pokud <see cref="Success"/> je false.</summary>
    public string? FailureReason { get; init; }

    /// <summary>Node-scoped kontext pro AI nebo UI preview.</summary>
    public NodeAssistContext? Context { get; init; }

    /// <summary>Vysledek AI asistence. Null pokud AI neni dostupne nebo selhalo.</summary>
    public NodeAssistResult? AiResult { get; init; }

    /// <summary>Uzivatelsky prompt ktery byl soucasti requestu.</summary>
    public string UserPrompt { get; init; } = string.Empty;
}
