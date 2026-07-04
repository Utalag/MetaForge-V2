namespace MetaForge.BusinessModel.Models;

/// <summary>
/// SOURCE OF TRUTH — kompletní stav business modelu.
/// Veškerý stav systému je odvoditelný z tohoto dokumentu.
/// NIKDY nemutovat přímo — vždy přes PatchEngine + CommandLog.
/// </summary>
public sealed class BusinessAuthoringDocument
{
    /// <summary>Název projektu.</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Verze schématu dokumentu.</summary>
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>Datum poslední modifikace.</summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>Business entity.</summary>
    public List<BusinessEntityNode> Entities { get; } = new();

    /// <summary>Relace mezi entitami.</summary>
    public List<BusinessRelationNode> Relations { get; } = new();

    /// <summary>Vlastní typy definované uživatelem.</summary>
    public List<CustomTypeDefinition> CustomTypes { get; } = new();

    /// <summary>Nezodpovězené otázky.</summary>
    public List<PendingQuestionNode> PendingQuestions { get; } = new();
}

/// <summary>
/// Vlastní typ definovaný uživatelem — např. "Address", "Money".
/// </summary>
public sealed class CustomTypeDefinition
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název typu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Základní typ (např. "string", "decimal").</summary>
    public string BaseType { get; set; } = "string";

    /// <summary>Validační pravidla.</summary>
    public List<string> ValidationRules { get; } = new();

    /// <summary>Popis typu.</summary>
    public string? Description { get; set; }
}
