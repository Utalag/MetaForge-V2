using System.Text.Json;

namespace MetaForge.BusinessModel;

public sealed partial class BusinessPatchEngine
{
    private readonly BusinessDocumentValidator _validator = new();
    private readonly BusinessIdAllocator _idAllocator = new();

    public BusinessPatchApplyResult Apply(BusinessAuthoringDocument document, IReadOnlyList<BusinessPatchOperation> operations)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(operations);

        if (operations.Count == 0)
        {
            var validationIssues = _validator.Validate(document);
            return new BusinessPatchApplyResult
            {
                Success = validationIssues.All(issue => !IsError(issue)),
                Document = document,
                Issues = validationIssues,
                AppliedOperationCount = 0,
            };
        }

        var originalDocument = document;
        var workingDocument = CloneDocument(document);
        var generatedQuestions = new List<PendingQuestionNode>();
        var appliedOperationCount = 0;

        foreach (var operation in operations)
        {
            try
            {
                workingDocument = ApplyOperation(workingDocument, operation, generatedQuestions);
                appliedOperationCount++;
            }
            catch (PatchOperationException ex)
            {
                return new BusinessPatchApplyResult
                {
                    Success = false,
                    Document = originalDocument,
                    Issues = [new BusinessValidationIssue(ex.Code, ex.Message, "Error", ex.Path)],
                    GeneratedQuestions = generatedQuestions,
                    AppliedOperationCount = 0,
                };
            }
        }

        var issues = _validator.Validate(workingDocument);
        if (issues.Any(IsError))
        {
            return new BusinessPatchApplyResult
            {
                Success = false,
                Document = originalDocument,
                Issues = issues,
                GeneratedQuestions = generatedQuestions,
                AppliedOperationCount = 0,
            };
        }

        return new BusinessPatchApplyResult
        {
            Success = true,
            Document = workingDocument,
            Issues = issues,
            GeneratedQuestions = generatedQuestions,
            AppliedOperationCount = appliedOperationCount,
        };
    }

    private BusinessAuthoringDocument ApplyOperation(
        BusinessAuthoringDocument document,
        BusinessPatchOperation operation,
        ICollection<PendingQuestionNode> generatedQuestions)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(operation);

        return NormalizeOperation(operation.Op) switch
        {
            "set_project" => ApplySetProject(document, operation),
            "add_entity" => ApplyAddEntity(document, operation),
            "update_entity" => ApplyUpdateEntity(document, operation),
            "delete_entity" => ApplyDeleteEntity(document, operation),
            "add_attribute" => ApplyAddAttribute(document, operation),
            "update_attribute" => ApplyUpdateAttribute(document, operation),
            "move_attribute" => ApplyMoveAttribute(document, operation),
            "delete_attribute" => ApplyDeleteAttribute(document, operation),
            "add_behavior" => ApplyAddBehavior(document, operation),
            "update_behavior" => ApplyUpdateBehavior(document, operation),
            "delete_behavior" => ApplyDeleteBehavior(document, operation),
            "add_relation" => ApplyAddRelation(document, operation),
            "update_relation" => ApplyUpdateRelation(document, operation),
            "delete_relation" => ApplyDeleteRelation(document, operation),
            "add_workflow" => ApplyAddWorkflow(document, operation),
            "update_workflow" => ApplyUpdateWorkflow(document, operation),
            "delete_workflow" => ApplyDeleteWorkflow(document, operation),
            "add_workflow_step" => ApplyAddWorkflowStep(document, operation, generatedQuestions),
            "update_workflow_step" => ApplyUpdateWorkflowStep(document, operation, generatedQuestions),
            "move_workflow_step" => ApplyMoveWorkflowStep(document, operation),
            "delete_workflow_step" => ApplyDeleteWorkflowStep(document, operation),
            "remove_workflow_step" => ApplyDeleteWorkflowStep(document, operation),
            "add_workflow_transition" => ApplyAddWorkflowTransition(document, operation),
            "update_workflow_transition" => ApplyUpdateWorkflowTransition(document, operation),
            "delete_workflow_transition" => ApplyDeleteWorkflowTransition(document, operation),
            "bind_workflow_step" => ApplyBindWorkflowStep(document, operation),
            "update_workflow_binding" => ApplyUpdateWorkflowBinding(document, operation),
            "add_note" => ApplyAddNote(document, operation),
            "resolve_question" => ApplyResolveQuestion(document, operation),
            "apply_preset" => ApplyPreset(document, operation),
            "enrich_attribute" => ApplyEnrichAttribute(document, operation),
            "update_coredetail" => ApplyUpdateCoreDetail(document, operation),
            "add_customtype" => ApplyAddCustomType(document, operation),
            "update_customtype" => ApplyUpdateCustomType(document, operation),
            "delete_customtype" => ApplyDeleteCustomType(document, operation),
            "increment_customtype_usage" => ApplyIncrementCustomTypeUsage(document, operation),
            "decrement_customtype_usage" => ApplyDecrementCustomTypeUsage(document, operation),
            _ => throw new PatchOperationException("patch.op.unsupported", $"Patch operace {operation.Op} neni podporovana.", "operations.op"),
        };
    }
}
