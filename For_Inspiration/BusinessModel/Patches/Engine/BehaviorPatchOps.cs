using System.Text.Json;

namespace MetaForge.BusinessModel;

public sealed partial class BusinessPatchEngine
{
    private BusinessAuthoringDocument ApplyAddBehavior(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var name = RequireString(operation, "name", "operations.data.name");
        if (entity.Behaviors.Any(behavior => string.Equals(behavior.Name, name, StringComparison.OrdinalIgnoreCase)))
            return document;

        var behavior = new BusinessBehaviorNode
        {
            Id = operation.BehaviorId ?? GetString(operation, "id") ?? _idAllocator.CreateBehaviorId(name, entity),
            Name = name,
            Kind = GetEnum<BusinessBehaviorKind>(operation, "kind") ?? BusinessBehaviorKind.Query,
            Summary = GetString(operation, "summary"),
            Inputs = ParseBehaviorInputList(operation, "inputs"),
            Returns = GetString(operation, "returns"),
            Notes = ParseNoteList(operation, "notes"),
        };

        var behaviors = entity.Behaviors.ToList();
        InsertAtOrAppend(behaviors, behavior, operation.NewIndex ?? GetInt(operation, "index"));
        return ReplaceEntity(document, entity, CopyEntity(entity, behaviors: behaviors));
    }

    private BusinessAuthoringDocument ApplyUpdateBehavior(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var behavior = RequireBehavior(entity, operation.BehaviorId, "operations.behaviorId");
        var behaviors = entity.Behaviors.ToList();
        var index = behaviors.FindIndex(item => string.Equals(item.Id, behavior.Id, StringComparison.OrdinalIgnoreCase));

        behaviors[index] = new BusinessBehaviorNode
        {
            Id = behavior.Id,
            Name = GetString(operation, "name") ?? behavior.Name,
            Kind = GetEnum<BusinessBehaviorKind>(operation, "kind") ?? behavior.Kind,
            Summary = HasValue(operation, "summary") ? GetString(operation, "summary") : behavior.Summary,
            Inputs = HasValue(operation, "inputs") ? ParseBehaviorInputList(operation, "inputs") : behavior.Inputs,
            Returns = HasValue(operation, "returns") ? GetString(operation, "returns") : behavior.Returns,
            Notes = HasValue(operation, "notes") ? ParseNoteList(operation, "notes") : behavior.Notes,
        };

        return ReplaceEntity(document, entity, CopyEntity(entity, behaviors: behaviors));
    }

    private BusinessAuthoringDocument ApplyDeleteBehavior(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.EntityId) || string.IsNullOrWhiteSpace(operation.BehaviorId))
            return document;

        var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var behaviors = entity.Behaviors.Where(behavior => !string.Equals(behavior.Id, operation.BehaviorId, StringComparison.OrdinalIgnoreCase)).ToList();
        if (behaviors.Count == entity.Behaviors.Count)
            return document;

        var updatedDocument = ReplaceEntity(document, entity, CopyEntity(entity, behaviors: behaviors));
        var questions = updatedDocument.PendingQuestions
            .Select(question => DismissQuestionIfMatches(question, entityId: entity.Id, behaviorId: operation.BehaviorId))
            .ToList();

        return CopyDocument(updatedDocument, pendingQuestions: questions);
    }
}
