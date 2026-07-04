namespace MetaForge.BusinessModel.Models;

/// <summary>
/// SOURCE OF TRUTH — kompletní stav business modelu.
/// Veškerý stav systému je odvoditelný z tohoto dokumentu.
/// NIKDY nemutovat přímo — vždy přes PatchEngine + CommandLog.
/// </summary>
public sealed record BusinessAuthoringDocument
{
    /// <summary>Aktuální verze schématu.</summary>
    public const string CurrentSchemaVersion = "1.0";

    /// <summary>Název projektu.</summary>
    public string ProjectName { get; init; } = string.Empty;

    /// <summary>Strukturované informace o projektu (rozšířená náhrada za ProjectName).</summary>
    public BusinessProjectInfo Project { get; init; } = new();

    /// <summary>Verze schématu dokumentu.</summary>
    public string SchemaVersion { get; init; } = CurrentSchemaVersion;

    /// <summary>Datum poslední modifikace.</summary>
    public DateTime LastModified { get; init; } = DateTime.UtcNow;

    /// <summary>Business entity.</summary>
    public IReadOnlyList<BusinessEntityNode> Entities { get; init; } = [];

    /// <summary>Relace mezi entitami.</summary>
    public IReadOnlyList<BusinessRelationNode> Relations { get; init; } = [];

    /// <summary>Vlastní typy definované uživatelem.</summary>
    public IReadOnlyList<CustomTypeDefinition> CustomTypes { get; init; } = [];

    /// <summary>Nezodpovězené otázky.</summary>
    public IReadOnlyList<PendingQuestionNode> PendingQuestions { get; init; } = [];

    /// <summary>Workflow definice.</summary>
    public IReadOnlyList<BusinessWorkflowNode> Workflows { get; init; } = [];
}

/// <summary>
/// Vlastní typ definovaný uživatelem — např. "Address", "Money".
/// </summary>
public sealed record CustomTypeDefinition
{
    /// <summary>Unikátní identifikátor.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Název typu.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Základní typ (např. "string", "decimal").</summary>
    public string BaseType { get; init; } = "string";

    /// <summary>Validační pravidla.</summary>
    public IReadOnlyList<string> ValidationRules { get; init; } = [];

    /// <summary>Popis typu.</summary>
    public string? Description { get; init; }
}
