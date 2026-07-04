namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Relace mezi entitami — např. "Customer → Order" (1:N).
/// </summary>
public sealed class BusinessRelationNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>ID zdrojové entity.</summary>
    public string FromEntityId { get; set; } = string.Empty;

    /// <summary>ID cílové entity.</summary>
    public string ToEntityId { get; set; } = string.Empty;

    /// <summary>Typ relace: "OneToOne", "OneToMany", "ManyToOne", "ManyToMany".</summary>
    public string RelationType { get; set; } = "OneToMany";

    /// <summary>Název navigační property na zdrojové entitě.</summary>
    public string? FromNavigationName { get; set; }

    /// <summary>Název navigační property na cílové entitě.</summary>
    public string? ToNavigationName { get; set; }
}
