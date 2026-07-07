namespace MetaForge.FrameworkMetadata.EntityFrameworkCore;

/// <summary>
/// Metadata pro EF Core konfiguraci entity — analogie k `IEntityTypeConfiguration&lt;T&gt;`.
/// </summary>
public sealed record EntityConfiguration
{
    /// <summary>Název entitního typu (např. "Customer").</summary>
    public string EntityTypeName { get; init; } = string.Empty;

    /// <summary>Název databázové tabulky. Null = použije se konvence (název entity).</summary>
    public string? TableName { get; init; }

    /// <summary>Názvy vlastností tvořících primární klíč.</summary>
    public List<string> PrimaryKeyProperties { get; init; } = new();

    /// <summary>Konfigurace jednotlivých vlastností (max délka, required, index, ...).</summary>
    public List<PropertyConfiguration> Properties { get; init; } = new();

    /// <summary>Konfigurace vztahů (navigation properties) k jiným entitám.</summary>
    public List<RelationshipConfiguration> Relationships { get; init; } = new();
}

/// <summary>Konfigurace jedné vlastnosti entity pro EF Core.</summary>
public sealed record PropertyConfiguration
{
    public string PropertyName { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public bool HasIndex { get; init; }
}

/// <summary>Konfigurace vztahu mezi entitami (1:1, 1:N, N:N).</summary>
public sealed record RelationshipConfiguration
{
    public string NavigationPropertyName { get; init; } = string.Empty;
    public string TargetEntityTypeName { get; init; } = string.Empty;
    public RelationshipKind Kind { get; init; } = RelationshipKind.OneToMany;
    public string? ForeignKeyProperty { get; init; }
}

/// <summary>Druh vztahu mezi entitami v EF Core modelu.</summary>
public enum RelationshipKind
{
    OneToOne,
    OneToMany,
    ManyToMany,
}
