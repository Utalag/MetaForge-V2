using System.Text.Json;

namespace MetaForge.BusinessModel;

public sealed partial class BusinessPatchEngine
{
    private BusinessAuthoringDocument ApplyAddNote(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var note = new BusinessNoteNode
        {
            Id = GetString(operation, "id") ?? _idAllocator.CreateNoteId(),
            Text = RequireString(operation, "text", "operations.data.text"),
        };

        if (!string.IsNullOrWhiteSpace(operation.RelationId))
        {
            var relation = RequireRelation(document, operation.RelationId, "operations.relationId");
            var notes = relation.Notes.ToList();
            notes.Add(note);
            return ReplaceRelation(document, relation, CopyRelation(relation, notes: notes));
        }

        if (!string.IsNullOrWhiteSpace(operation.BehaviorId))
        {
            var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
            var behavior = RequireBehavior(entity, operation.BehaviorId, "operations.behaviorId");
            var behaviors = entity.Behaviors.ToList();
            var index = behaviors.FindIndex(item => string.Equals(item.Id, behavior.Id, StringComparison.OrdinalIgnoreCase));
            var notes = behavior.Notes.ToList();
            notes.Add(note);
            behaviors[index] = CopyBehavior(behavior, notes: notes);
            return ReplaceEntity(document, entity, CopyEntity(entity, behaviors: behaviors));
        }

        if (!string.IsNullOrWhiteSpace(operation.EntityId))
        {
            var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
            var notes = entity.Notes.ToList();
            notes.Add(note);
            return ReplaceEntity(document, entity, CopyEntity(entity, notes: notes));
        }

        var rootNotes = document.Notes.ToList();
        rootNotes.Add(note);
        return CopyDocument(document, notes: rootNotes);
    }

    private BusinessAuthoringDocument ApplyResolveQuestion(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.QuestionId))
            return document;

        var questions = document.PendingQuestions.ToList();
        var index = questions.FindIndex(question => string.Equals(question.Id, operation.QuestionId, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return document;

        var question = questions[index];
        questions[index] = new PendingQuestionNode
        {
            Id = question.Id,
            Text = HasValue(operation, "text") ? GetString(operation, "text") ?? question.Text : question.Text,
            Status = GetEnum<PendingQuestionStatus>(operation, "status") ?? PendingQuestionStatus.Resolved,
            Scope = question.Scope,
            RelatedEntityId = question.RelatedEntityId,
            RelatedAttributeId = question.RelatedAttributeId,
            RelatedBehaviorId = question.RelatedBehaviorId,
            RelatedRelationId = question.RelatedRelationId,
        };

        return CopyDocument(document, pendingQuestions: questions);
    }

    private BusinessAuthoringDocument ApplyPreset(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var attribute = RequireAttribute(entity, operation.AttributeId, "operations.attributeId");
        var presetId = RequireString(operation, "presetId", "operations.data.presetId");
        var attributes = entity.Attributes.ToList();
        var index = attributes.FindIndex(item => string.Equals(item.Id, attribute.Id, StringComparison.OrdinalIgnoreCase));

        var coreDetail = attribute.CoreDetail ?? new BusinessAttributeCoreDetail();
        attributes[index] = new BusinessAttributeNode
        {
            Id = attribute.Id,
            Name = attribute.Name,
            Type = GetString(operation, "type") ?? attribute.Type,
            CustomType = HasValue(operation, "customType") ? GetString(operation, "customType") : attribute.CustomType,
            Required = GetBool(operation, "required") ?? attribute.Required,
            Summary = HasValue(operation, "summary") ? GetString(operation, "summary") : attribute.Summary,
            DefaultValue = HasValue(operation, "defaultValue") ? GetString(operation, "defaultValue") : attribute.DefaultValue,
            Constraints = HasValue(operation, "constraints") ? GetStringList(operation, "constraints") : attribute.Constraints,
            Computed = HasValue(operation, "computed") ? GetString(operation, "computed") : attribute.Computed,
            PresetId = presetId,
            CoreDetail = new BusinessAttributeCoreDetail
            {
                Source = GetEnum<CoreInfoSource>(operation, "source") ?? CoreInfoSource.Generated,
                ResolvedPresetId = presetId,
                ValueObjectName = GetString(operation, "valueObjectName") ?? coreDetail.ValueObjectName,
                IsStrongType = GetBool(operation, "isStrongType") ?? coreDetail.IsStrongType,
                LastSyncedAt = DateTimeOffset.UtcNow,
            },
        };

        return ReplaceEntity(document, entity, CopyEntity(entity, attributes: attributes));
    }

    private BusinessAuthoringDocument ApplyEnrichAttribute(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var attribute = RequireAttribute(entity, operation.AttributeId, "operations.attributeId");
        var attributes = entity.Attributes.ToList();
        var index = attributes.FindIndex(item => string.Equals(item.Id, attribute.Id, StringComparison.OrdinalIgnoreCase));

        var coreDetail = attribute.CoreDetail ?? new BusinessAttributeCoreDetail();
        attributes[index] = new BusinessAttributeNode
        {
            Id = attribute.Id,
            Name = GetString(operation, "name") ?? attribute.Name,
            Type = GetString(operation, "type") ?? attribute.Type,
            CustomType = HasValue(operation, "customType") ? GetString(operation, "customType") : attribute.CustomType,
            Required = GetBool(operation, "required") ?? attribute.Required,
            Summary = HasValue(operation, "summary") ? GetString(operation, "summary") : attribute.Summary,
            DefaultValue = HasValue(operation, "defaultValue") ? GetString(operation, "defaultValue") : attribute.DefaultValue,
            Constraints = HasValue(operation, "constraints") ? GetStringList(operation, "constraints") : attribute.Constraints,
            Computed = HasValue(operation, "computed") ? GetString(operation, "computed") : attribute.Computed,
            PresetId = GetString(operation, "presetId") ?? attribute.PresetId,
            CoreDetail = new BusinessAttributeCoreDetail
            {
                Source = GetEnum<CoreInfoSource>(operation, "source") ?? CoreInfoSource.Generated,
                ResolvedPresetId = GetString(operation, "resolvedPresetId") ?? coreDetail.ResolvedPresetId,
                ValueObjectName = GetString(operation, "valueObjectName") ?? coreDetail.ValueObjectName,
                IsStrongType = GetBool(operation, "isStrongType") ?? coreDetail.IsStrongType,
                LastSyncedAt = DateTimeOffset.UtcNow,
            },
        };

        return ReplaceEntity(document, entity, CopyEntity(entity, attributes: attributes));
    }

    private BusinessAuthoringDocument ApplyUpdateCoreDetail(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var attribute = RequireAttribute(entity, operation.AttributeId, "operations.attributeId");
        var attributes = entity.Attributes.ToList();
        var index = attributes.FindIndex(item => string.Equals(item.Id, attribute.Id, StringComparison.OrdinalIgnoreCase));

        var coreDetail = attribute.CoreDetail ?? new BusinessAttributeCoreDetail();
        attributes[index] = new BusinessAttributeNode
        {
            Id = attribute.Id,
            Name = attribute.Name,
            Type = attribute.Type,
            CustomType = attribute.CustomType,
            Required = attribute.Required,
            Summary = attribute.Summary,
            DefaultValue = attribute.DefaultValue,
            Constraints = attribute.Constraints,
            Computed = attribute.Computed,
            PresetId = attribute.PresetId,
            CoreDetail = new BusinessAttributeCoreDetail
            {
                Source = GetEnum<CoreInfoSource>(operation, "source") ?? coreDetail.Source,
                ResolvedPresetId = HasValue(operation, "resolvedPresetId") ? GetString(operation, "resolvedPresetId") : coreDetail.ResolvedPresetId,
                ValueObjectName = HasValue(operation, "valueObjectName") ? GetString(operation, "valueObjectName") : coreDetail.ValueObjectName,
                IsStrongType = GetBool(operation, "isStrongType") ?? coreDetail.IsStrongType,
                LastSyncedAt = DateTimeOffset.UtcNow,
            },
        };

        return ReplaceEntity(document, entity, CopyEntity(entity, attributes: attributes));
    }
}
