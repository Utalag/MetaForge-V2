using MetaForge.Core.Catalog;

namespace MetaForge.Translator;

/// <summary>
/// Node-scoped kontext pro AI asistenci — odvozeny preview model z <see cref="ProjectionView"/>.
/// Drzi minimum okoli: vybrany node, parent, sibling summary a kompaktni authoring context.
/// </summary>
public sealed class NodeAssistContext
{
    /// <summary>ID rodicovske entity.</summary>
    public string EntityId { get; init; } = string.Empty;

    /// <summary>Nazev rodicovske entity.</summary>
    public string EntityName { get; init; } = string.Empty;

    /// <summary>ID vybraneho node. Null pokud je cilem cela entita.</summary>
    public string? NodeId { get; init; }

    /// <summary>Nazev vybraneho node.</summary>
    public string? NodeName { get; init; }

    /// <summary>Druh vybraneho node.</summary>
    public NodeKind? Kind { get; init; }

    /// <summary>Projekce vybraneho atributu. Null pokud neni attribute.</summary>
    public ExpertAttributeProjection? Attribute { get; init; }

    /// <summary>Projekce vybraneho behavioru. Null pokud neni behavior.</summary>
    public ExpertBehaviorProjection? Behavior { get; init; }

    /// <summary>Projekce cele entity pokud je cilem entita. Null pokud je cilem konkretni node.</summary>
    public ExpertEntityProjection? Entity { get; init; }

    /// <summary>Jmena sibling atributu ve stejne entite.</summary>
    public IReadOnlyList<string> SiblingAttributeNames { get; init; } = [];

    /// <summary>Jmena sibling behavioru ve stejne entite.</summary>
    public IReadOnlyList<string> SiblingBehaviorNames { get; init; } = [];

    /// <summary>Celkovy pocet entit v dokumentu.</summary>
    public int TotalEntityCount { get; init; }

    /// <summary>Celkovy pocet relaci v dokumentu.</summary>
    public int TotalRelationCount { get; init; }

    /// <summary>Kompaktni workflow summary z authoring contextu.</summary>
    public WorkflowSummary Workflow { get; init; } = new();

    /// <summary>Pocet otevrenych otazek v dokumentu.</summary>
    public int OpenQuestionCount { get; init; }

    /// <summary>Texty otevrenych otazek (max. 5 pro kompaktnost).</summary>
    public IReadOnlyList<string> OpenQuestionTexts { get; init; } = [];

    /// <summary>Discovery hints pokud byly requestovany.</summary>
    public DiscoverySummary? DiscoveryHints { get; init; }
}
