using System.Text.Json;

namespace MetaForge.BusinessModel;

public sealed partial class BusinessPatchEngine
{
    private BusinessAuthoringDocument ApplySetProject(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var updatedProject = new BusinessProjectInfo
        {
            Id = GetString(operation, "id") ?? document.Project.Id,
            Name = GetString(operation, "name") ?? document.Project.Name,
            Description = HasValue(operation, "description") ? GetString(operation, "description") : document.Project.Description,
            Icon = HasValue(operation, "icon") ? GetString(operation, "icon") : document.Project.Icon,
            Version = GetInt(operation, "version") ?? document.Project.Version,
        };

        return CopyDocument(document, project: updatedProject);
    }

    private BusinessAuthoringDocument ApplyAddEntity(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var name = RequireString(operation, "name", "operations.data.name");
        if (document.Entities.Any(entity => string.Equals(entity.Name, name, StringComparison.OrdinalIgnoreCase)))
            return document;

        var entity = new BusinessEntityNode
        {
            Id = operation.EntityId ?? GetString(operation, "id") ?? _idAllocator.CreateEntityId(name, document),
            Name = name,
            Summary = GetString(operation, "summary"),
            Icon = GetString(operation, "icon"),
            PresetId = GetString(operation, "presetId"),
            Attributes = ParseAttributeList(operation, "attributes"),
            Behaviors = ParseBehaviorList(operation, "behaviors"),
            Notes = ParseNoteList(operation, "notes"),
        };

        var entities = document.Entities.ToList();
        InsertAtOrAppend(entities, entity, operation.NewIndex ?? GetInt(operation, "index"));
        return CopyDocument(document, entities: entities);
    }

    private BusinessAuthoringDocument ApplyUpdateEntity(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var entities = document.Entities.ToList();
        var index = entities.FindIndex(item => string.Equals(item.Id, entity.Id, StringComparison.OrdinalIgnoreCase));

        entities[index] = new BusinessEntityNode
        {
            Id = entity.Id,
            Name = GetString(operation, "name") ?? entity.Name,
            Summary = HasValue(operation, "summary") ? GetString(operation, "summary") : entity.Summary,
            Icon = HasValue(operation, "icon") ? GetString(operation, "icon") : entity.Icon,
            PresetId = HasValue(operation, "presetId") ? GetString(operation, "presetId") : entity.PresetId,
            Attributes = entity.Attributes,
            Behaviors = entity.Behaviors,
            Notes = HasValue(operation, "notes") ? ParseNoteList(operation, "notes") : entity.Notes,
        };

        return CopyDocument(document, entities: entities);
    }

    private BusinessAuthoringDocument ApplyDeleteEntity(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.EntityId))
            return document;

        var deletedEntity = document.Entities.FirstOrDefault(entity => string.Equals(entity.Id, operation.EntityId, StringComparison.OrdinalIgnoreCase));
        if (deletedEntity is null)
            return document;

        var entities = document.Entities.Where(entity => !string.Equals(entity.Id, operation.EntityId, StringComparison.OrdinalIgnoreCase)).ToList();
        var relations = document.Relations
            .Where(relation => !string.Equals(relation.SourceEntityId, operation.EntityId, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(relation.TargetEntityId, operation.EntityId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var questions = document.PendingQuestions
            .Select(question => DismissQuestionIfMatches(question, entityId: operation.EntityId))
            .ToList();

        var updatedDocument = CopyDocument(document, entities: entities, relations: relations, pendingQuestions: questions);

        // Dekrementovat UsageCount pro vsechny CustomType pouzite v atributech entity (blok G)
        foreach (var attribute in deletedEntity.Attributes)
        {
            if (attribute.CustomType is not null)
                updatedDocument = UpdateCustomTypeUsage(updatedDocument, attribute.CustomType, delta: -1);
        }

        return updatedDocument;
    }
}
