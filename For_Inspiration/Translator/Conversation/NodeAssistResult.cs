using MetaForge.BusinessModel;

namespace MetaForge.Translator;

/// <summary>
/// Strukturovany vysledek AI node-level asistence — navrhy hodnot a operace.
/// </summary>
public sealed class NodeAssistResult
{
    /// <summary>Navrzeny summary pro node.</summary>
    public string? Summary { get; init; }

    /// <summary>Navrzeny vstupy pro behavior. Null pokud neni behavior.</summary>
    public IReadOnlyList<NodeAssistSuggestedInput>? Inputs { get; init; }

    /// <summary>Navrzeny navratovy typ pro behavior. Null pokud neni behavior.</summary>
    public string? Returns { get; init; }

    /// <summary>Vysvetleni navrhu od AI.</summary>
    public string? Explanation { get; init; }

    /// <summary>Navrzeny patch operace pro explicitni apply.</summary>
    public IReadOnlyList<BusinessPatchOperation> ProposedOperations { get; init; } = [];

    /// <summary>Varovani vznikla pri sanitizaci nebo interpretaci AI navrhu.</summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

/// <summary>
/// Navrzeny vstup pro behavior.
/// </summary>
public sealed class NodeAssistSuggestedInput
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string? Summary { get; init; }
}
