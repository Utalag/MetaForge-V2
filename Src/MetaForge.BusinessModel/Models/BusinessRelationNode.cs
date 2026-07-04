namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Relace mezi entitami — např. "Customer → Order" (1:N).
/// </summary>
public sealed record BusinessRelationNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>ID zdrojové entity.</summary>
    public string FromEntityId { get; init; } = string.Empty;

    /// <summary>ID cílové entity.</summary>
    public string ToEntityId { get; init; } = string.Empty;

    /// <summary>Typ relace: "OneToOne", "OneToMany", "ManyToOne", "ManyToMany".</summary>
    public string RelationType { get; init; } = "OneToMany";

    /// <summary>Druh relace — typově bezpečná alternativa k RelationType.</summary>
    public BusinessRelationKind Kind { get; init; } = BusinessRelationKind.HasMany;

    /// <summary>Název navigační property na zdrojové entitě.</summary>
    public string? FromNavigationName { get; init; }

    /// <summary>Název navigační property na cílové entitě.</summary>
    public string? ToNavigationName { get; init; }
}
