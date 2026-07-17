namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Chování (metoda) entity — např. "CalculateDiscount", "SendNotification".
/// </summary>
public sealed record BusinessBehaviorNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název chování.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Popis chování (co dělá).</summary>
    public string? Description { get; init; }

    /// <summary>Návratový typ (business typ, např. "decimal").</summary>
    public string ReturnType { get; init; } = "void";

    /// <summary>Druh chování — dotaz, příkaz, nebo pravidlo.</summary>
    public BusinessBehaviorKind Kind { get; init; } = BusinessBehaviorKind.Command;

    /// <summary>Parametry chování.</summary>
    public IReadOnlyList<BusinessParameterNode> Parameters { get; init; } = [];
}

/// <summary>
/// Parametr chování.
/// </summary>
public sealed record BusinessParameterNode
{
    /// <summary>Unikátní identifikátor parametru (PROP-060).</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "string";
    public bool IsRequired { get; init; } = true;
    public string? DefaultValue { get; init; }
    public string? Summary { get; init; }
}
