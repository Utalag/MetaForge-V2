namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Reprezentuje jednu business entitu — např. "Customer", "Order", "Product".
/// Obsahuje atributy, chování, relace a poznámky.
/// </summary>
public sealed class BusinessEntityNode
{
    /// <summary>Unikátní identifikátor entity.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název entity (např. "Customer").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Atributy entity.</summary>
    public List<BusinessAttributeNode> Attributes { get; } = new();

    /// <summary>Chování entity (metody).</summary>
    public List<BusinessBehaviorNode> Behaviors { get; } = new();

    /// <summary>Relace na jiné entity.</summary>
    public List<BusinessRelationNode> Relations { get; } = new();

    /// <summary>Poznámky k entitě.</summary>
    public List<BusinessNoteNode> Notes { get; } = new();
}
