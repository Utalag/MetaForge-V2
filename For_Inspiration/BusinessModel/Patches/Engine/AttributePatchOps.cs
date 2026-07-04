using System.Text.Json;

namespace MetaForge.BusinessModel;

public sealed partial class BusinessPatchEngine
{
    private BusinessAuthoringDocument ApplyAddAttribute(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var name = RequireString(operation, "name", "operations.data.name");
        if (entity.Attributes.Any(attribute => string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase)))
            return document;

        var customTypeName = GetString(operation, "customType");
        var customType = customTypeName is not null
            ? document.CustomTypes.FirstOrDefault(ct => string.Equals(ct.Name, customTypeName, StringComparison.OrdinalIgnoreCase))
            : null;

        var explicitConstraints = GetStringList(operation, "constraints");
        var mergedConstraints = MergeConstraints(customType?.Constraints, explicitConstraints);

        var attribute = new BusinessAttributeNode
        {
            Id = operation.AttributeId ?? GetString(operation, "id") ?? _idAllocator.CreateAttributeId(name, entity),
            Name = name,
            Type = GetString(operation, "type") ?? "text",
            CustomType = customTypeName,
            Required = GetBool(operation, "required") ?? false,
            Summary = GetString(operation, "summary"),
            DefaultValue = GetString(operation, "defaultValue"),
            Constraints = mergedConstraints,
            Computed = GetString(operation, "computed"),
            PresetId = GetString(operation, "presetId"),
        };

        var attributes = entity.Attributes.ToList();
        InsertAtOrAppend(attributes, attribute, operation.NewIndex ?? GetInt(operation, "index"));
        var updatedDocument = ReplaceEntity(document, entity, CopyEntity(entity, attributes: attributes));

        // Inkrementovat UsageCount CustomType (blok G)
        if (customType is not null)
            updatedDocument = UpdateCustomTypeUsage(updatedDocument, customType.Name, delta: 1);

        return updatedDocument;
    }

    private BusinessAuthoringDocument ApplyUpdateAttribute(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var attribute = RequireAttribute(entity, operation.AttributeId, "operations.attributeId");
        var attributes = entity.Attributes.ToList();
        var index = attributes.FindIndex(item => string.Equals(item.Id, attribute.Id, StringComparison.OrdinalIgnoreCase));

        var newCustomTypeName = HasValue(operation, "customType") ? GetString(operation, "customType") : attribute.CustomType;
        var newCustomType = newCustomTypeName is not null
            ? document.CustomTypes.FirstOrDefault(ct => string.Equals(ct.Name, newCustomTypeName, StringComparison.OrdinalIgnoreCase))
            : null;

        var explicitConstraints = HasValue(operation, "constraints") ? GetStringList(operation, "constraints") : null;
        var mergedConstraints = explicitConstraints is not null
            ? MergeConstraints(newCustomType?.Constraints, explicitConstraints)
            : (newCustomType is not null && !string.Equals(newCustomTypeName, attribute.CustomType, StringComparison.OrdinalIgnoreCase)
                ? MergeConstraints(newCustomType.Constraints, [])
                : attribute.Constraints);

        attributes[index] = new BusinessAttributeNode
        {
            Id = attribute.Id,
            Name = GetString(operation, "name") ?? attribute.Name,
            Type = GetString(operation, "type") ?? attribute.Type,
            CustomType = newCustomTypeName,
            Required = GetBool(operation, "required") ?? attribute.Required,
            Summary = HasValue(operation, "summary") ? GetString(operation, "summary") : attribute.Summary,
            DefaultValue = HasValue(operation, "defaultValue") ? GetString(operation, "defaultValue") : attribute.DefaultValue,
            Constraints = mergedConstraints,
            Computed = HasValue(operation, "computed") ? GetString(operation, "computed") : attribute.Computed,
            PresetId = HasValue(operation, "presetId") ? GetString(operation, "presetId") : attribute.PresetId,
            CoreDetail = attribute.CoreDetail,
        };

        var updatedDocument = ReplaceEntity(document, entity, CopyEntity(entity, attributes: attributes));

        // Upravit UsageCount CustomType (blok G) — pokud se meni customType
        if (attribute.CustomType is not null && !string.Equals(attribute.CustomType, newCustomTypeName, StringComparison.OrdinalIgnoreCase))
            updatedDocument = UpdateCustomTypeUsage(updatedDocument, attribute.CustomType, delta: -1);

        if (newCustomTypeName is not null && !string.Equals(attribute.CustomType, newCustomTypeName, StringComparison.OrdinalIgnoreCase))
            updatedDocument = UpdateCustomTypeUsage(updatedDocument, newCustomTypeName, delta: 1);

        return updatedDocument;
    }

    private BusinessAuthoringDocument ApplyMoveAttribute(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var sourceEntity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var attribute = RequireAttribute(sourceEntity, operation.AttributeId, "operations.attributeId");
        var targetEntityId = RequireString(operation, "targetEntityId", "operations.data.targetEntityId");
        var targetEntity = RequireEntity(document, targetEntityId, "operations.data.targetEntityId");

        if (string.Equals(sourceEntity.Id, targetEntity.Id, StringComparison.OrdinalIgnoreCase))
            return document;

        var sourceAttributes = sourceEntity.Attributes.Where(item => !string.Equals(item.Id, attribute.Id, StringComparison.OrdinalIgnoreCase)).ToList();
        var targetAttributes = targetEntity.Attributes.ToList();
        InsertAtOrAppend(targetAttributes, attribute, operation.NewIndex ?? GetInt(operation, "index"));

        var updatedDocument = ReplaceEntity(document, sourceEntity, CopyEntity(sourceEntity, attributes: sourceAttributes));
        updatedDocument = ReplaceEntity(updatedDocument, targetEntity, CopyEntity(targetEntity, attributes: targetAttributes));

        var questions = updatedDocument.PendingQuestions
            .Select(question => MoveQuestionAttributeReference(question, attribute.Id, sourceEntity.Id, targetEntity.Id))
            .ToList();

        return CopyDocument(updatedDocument, pendingQuestions: questions);
    }

    private BusinessAuthoringDocument ApplyDeleteAttribute(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.EntityId) || string.IsNullOrWhiteSpace(operation.AttributeId))
            return document;

        var entity = RequireEntity(document, operation.EntityId, "operations.entityId");
        var deletedAttribute = entity.Attributes.FirstOrDefault(attribute => string.Equals(attribute.Id, operation.AttributeId, StringComparison.OrdinalIgnoreCase));
        if (deletedAttribute is null)
            return document;

        var attributes = entity.Attributes.Where(attribute => !string.Equals(attribute.Id, operation.AttributeId, StringComparison.OrdinalIgnoreCase)).ToList();
        var updatedDocument = ReplaceEntity(document, entity, CopyEntity(entity, attributes: attributes));
        var questions = updatedDocument.PendingQuestions
            .Select(question => DismissQuestionIfMatches(question, entityId: entity.Id, attributeId: operation.AttributeId))
            .ToList();

        updatedDocument = CopyDocument(updatedDocument, pendingQuestions: questions);

        // Dekrementovat UsageCount CustomType (blok G)
        if (deletedAttribute.CustomType is not null)
            updatedDocument = UpdateCustomTypeUsage(updatedDocument, deletedAttribute.CustomType, delta: -1);

        return updatedDocument;
    }
}
