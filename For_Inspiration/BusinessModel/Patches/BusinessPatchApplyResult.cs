namespace MetaForge.BusinessModel;

public sealed class BusinessPatchApplyResult
{
    public bool Success { get; init; }

    public BusinessAuthoringDocument Document { get; init; } = new();

    public IReadOnlyList<BusinessValidationIssue> Issues { get; init; } = [];

    public IReadOnlyList<PendingQuestionNode> GeneratedQuestions { get; init; } = [];

    public int AppliedOperationCount { get; init; }
}