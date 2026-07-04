namespace MetaForge.BusinessModel;

/// <summary>
/// Uživatelsky definovaný datový typ (CustomType).
/// Může být vytvořen ručně nebo inferred z atributu.
/// </summary>
public sealed class CustomTypeDefinition
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string UnderlyingType { get; init; } = "text";

    public IReadOnlyList<string> Constraints { get; init; } = [];

    public string? Summary { get; init; }

    /// <summary>manual | inferred</summary>
    public string Source { get; init; } = "manual";

    public bool IsCollection { get; init; }

    public string? CollectionKind { get; init; }

    public int UsageCount { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; set; }
}
