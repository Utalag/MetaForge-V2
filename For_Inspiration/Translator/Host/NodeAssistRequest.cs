namespace MetaForge.Translator;

/// <summary>
/// Vstupni kontrakt pro <see cref="BusinessAuthoringHostFacade.AssistNodeAsync"/>.
/// </summary>
public sealed class NodeAssistRequest
{
    /// <summary>Adresace ciloveho node.</summary>
    public NodePath NodePath { get; init; } = new();

    /// <summary>Uzivatelsky prompt nebo zamer asistence.</summary>
    public string UserPrompt { get; init; } = string.Empty;

    /// <summary>Zda do kontextu zahrnout discovery hints.</summary>
    public bool IncludeDiscovery { get; init; }
}
