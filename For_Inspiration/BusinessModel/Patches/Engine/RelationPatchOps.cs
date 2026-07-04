using System.Text.Json;

namespace MetaForge.BusinessModel;

public sealed partial class BusinessPatchEngine
{
    private BusinessAuthoringDocument ApplyAddRelation(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var sourceEntityId = RequireString(operation, "sourceEntityId", "operations.data.sourceEntityId");
        var targetEntityId = RequireString(operation, "targetEntityId", "operations.data.targetEntityId");
        var kind = GetEnum<BusinessRelationKind>(operation, "kind") ?? BusinessRelationKind.BelongsTo;

        if (document.Relations.Any(relation => string.Equals(relation.SourceEntityId, sourceEntityId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(relation.TargetEntityId, targetEntityId, StringComparison.OrdinalIgnoreCase)
                && relation.Kind == kind))
        {
            return document;
        }

        var seed = new BusinessRelationNode
        {
            Id = operation.RelationId ?? GetString(operation, "id") ?? string.Empty,
            SourceEntityId = sourceEntityId,
            TargetEntityId = targetEntityId,
            Kind = kind,
        };

        var relation = new BusinessRelationNode
        {
            Id = string.IsNullOrWhiteSpace(seed.Id) ? _idAllocator.CreateRelationId(seed, document) : seed.Id,
            SourceEntityId = sourceEntityId,
            TargetEntityId = targetEntityId,
            Kind = kind,
            SourceNavigationName = GetString(operation, "sourceNavigationName"),
            TargetNavigationName = GetString(operation, "targetNavigationName"),
            Notes = ParseNoteList(operation, "notes"),
        };

        var relations = document.Relations.ToList();
        InsertAtOrAppend(relations, relation, operation.NewIndex ?? GetInt(operation, "index"));
        return CopyDocument(document, relations: relations);
    }

    private BusinessAuthoringDocument ApplyUpdateRelation(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var relation = RequireRelation(document, operation.RelationId, "operations.relationId");
        var relations = document.Relations.ToList();
        var index = relations.FindIndex(item => string.Equals(item.Id, relation.Id, StringComparison.OrdinalIgnoreCase));

        relations[index] = new BusinessRelationNode
        {
            Id = relation.Id,
            SourceEntityId = GetString(operation, "sourceEntityId") ?? relation.SourceEntityId,
            TargetEntityId = GetString(operation, "targetEntityId") ?? relation.TargetEntityId,
            Kind = GetEnum<BusinessRelationKind>(operation, "kind") ?? relation.Kind,
            SourceNavigationName = HasValue(operation, "sourceNavigationName") ? GetString(operation, "sourceNavigationName") : relation.SourceNavigationName,
            TargetNavigationName = HasValue(operation, "targetNavigationName") ? GetString(operation, "targetNavigationName") : relation.TargetNavigationName,
            Notes = HasValue(operation, "notes") ? ParseNoteList(operation, "notes") : relation.Notes,
        };

        return CopyDocument(document, relations: relations);
    }

    private BusinessAuthoringDocument ApplyDeleteRelation(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.RelationId))
            return document;

        var relations = document.Relations.Where(relation => !string.Equals(relation.Id, operation.RelationId, StringComparison.OrdinalIgnoreCase)).ToList();
        if (relations.Count == document.Relations.Count)
            return document;

        var questions = document.PendingQuestions
            .Select(question => DismissQuestionIfMatches(question, relationId: operation.RelationId))
            .ToList();

        return CopyDocument(document, relations: relations, pendingQuestions: questions);
    }
}
