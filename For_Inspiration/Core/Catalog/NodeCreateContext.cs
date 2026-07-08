namespace MetaForge.Core.Catalog;

/// <summary>
/// Druh node pro který se vyhledávají presety.
/// </summary>
public enum NodeKind
{
    Entity,
    Attribute,
    Behavior,
    WorkflowStep,
}

/// <summary>
/// Kontext pro vytváření nového node — vstup pro preset suggestion.
/// </summary>
public sealed class NodeCreateContext
{
    /// <summary>Druh node (entita, atribut, behavior, workflow krok).</summary>
    public NodeKind Kind { get; init; }

    /// <summary>Název node (např. "Email", "ValidateOrder").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Volitelný typ nebo deklarovaný datový typ.</summary>
    public string? Type { get; init; }

    /// <summary>Volitelný popis zadaný uživatelem.</summary>
    public string? Description { get; init; }

    /// <summary>Volitelné tagy přiřazené uživatelem.</summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>ID rodičovské entity, pokud je node uvnitř entity.</summary>
    public string? ParentEntityId { get; init; }
}
