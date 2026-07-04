namespace MetaForge.BusinessModel;

public sealed class BusinessAuthoringDocument
{
    public string SchemaVersion { get; init; } = "1.0";

    public BusinessProjectInfo Project { get; init; } = new();

    public IReadOnlyList<BusinessEntityNode> Entities { get; init; } = [];

    public IReadOnlyList<BusinessRelationNode> Relations { get; init; } = [];

    public IReadOnlyList<BusinessWorkflowNode> Workflows { get; init; } = [];

    public IReadOnlyList<BusinessNoteNode> Notes { get; init; } = [];

    public IReadOnlyList<PendingQuestionNode> PendingQuestions { get; init; } = [];

    public IReadOnlyList<CustomTypeDefinition> CustomTypes { get; init; } = [];
}