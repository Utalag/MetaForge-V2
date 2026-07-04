namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Reprezentuje jednu business entitu — např. "Customer", "Order", "Product".
/// Obsahuje atributy, chování, relace a poznámky.
/// </summary>
public sealed record BusinessEntityNode
{
    /// <summary>Unikátní identifikátor entity.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název entity (např. "Customer").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Atributy entity.</summary>
    public IReadOnlyList<BusinessAttributeNode> Attributes { get; init; } = [];

    /// <summary>Chování entity (metody).</summary>
    public IReadOnlyList<BusinessBehaviorNode> Behaviors { get; init; } = [];

    /// <summary>Relace na jiné entity.</summary>
    public IReadOnlyList<BusinessRelationNode> Relations { get; init; } = [];

    /// <summary>Poznámky k entitě.</summary>
    public IReadOnlyList<BusinessNoteNode> Notes { get; init; } = [];
}
