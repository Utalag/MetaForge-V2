namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Chování (metoda) entity — např. "CalculateDiscount", "SendNotification".
/// </summary>
public sealed class BusinessBehaviorNode
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název chování.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Popis chování (co dělá).</summary>
    public string? Description { get; set; }

    /// <summary>Návratový typ (business typ, např. "decimal").</summary>
    public string ReturnType { get; set; } = "void";

    /// <summary>Parametry chování.</summary>
    public List<BusinessParameterNode> Parameters { get; } = new();
}

/// <summary>
/// Parametr chování.
/// </summary>
public sealed class BusinessParameterNode
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public bool IsRequired { get; set; } = true;
    public string? DefaultValue { get; set; }
}
