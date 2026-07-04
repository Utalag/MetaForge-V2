namespace MetaForge.BusinessModel;

public sealed partial class BusinessPatchEngine
{
    private BusinessAuthoringDocument ApplyAddCustomType(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var name = RequireString(operation, "name", "operations.data.name");
        if (document.CustomTypes.Any(ct => string.Equals(ct.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new PatchOperationException("customtype.duplicate", $"CustomType '{name}' jiz existuje.", "operations.data.name");

        var customType = new CustomTypeDefinition
        {
            Id = GetString(operation, "id") ?? _idAllocator.CreateCustomTypeId(name, document),
            Name = name,
            UnderlyingType = GetString(operation, "underlyingType") ?? "text",
            Constraints = ParseStringList(operation, "constraints"),
            Summary = GetString(operation, "summary"),
            Source = GetString(operation, "source") ?? "manual",
            IsCollection = GetBool(operation, "isCollection") ?? false,
            CollectionKind = GetString(operation, "collectionKind"),
            UsageCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var customTypes = document.CustomTypes.ToList();
        customTypes.Add(customType);
        return CopyDocument(document, customTypes: customTypes);
    }

    private BusinessAuthoringDocument ApplyUpdateCustomType(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var id = RequireString(operation, "id", "operations.data.id");
        var customTypes = document.CustomTypes.ToList();
        var index = customTypes.FindIndex(ct => string.Equals(ct.Id, id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            throw new PatchOperationException("customtype.notFound", $"CustomType '{id}' nebyl nalezen.", "operations.data.id");

        var existing = customTypes[index];
        var newName = GetString(operation, "name");
        if (newName is not null && !string.Equals(newName, existing.Name, StringComparison.OrdinalIgnoreCase)
            && document.CustomTypes.Any(ct => string.Equals(ct.Name, newName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new PatchOperationException("customtype.duplicate", $"CustomType '{newName}' jiz existuje.", "operations.data.name");
        }

        customTypes[index] = new CustomTypeDefinition
        {
            Id = existing.Id,
            Name = newName ?? existing.Name,
            UnderlyingType = GetString(operation, "underlyingType") ?? existing.UnderlyingType,
            Constraints = HasValue(operation, "constraints") ? ParseStringList(operation, "constraints") : existing.Constraints,
            Summary = HasValue(operation, "summary") ? GetString(operation, "summary") : existing.Summary,
            Source = existing.Source,
            IsCollection = GetBool(operation, "isCollection") ?? existing.IsCollection,
            CollectionKind = HasValue(operation, "collectionKind") ? GetString(operation, "collectionKind") : existing.CollectionKind,
            UsageCount = existing.UsageCount,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        return CopyDocument(document, customTypes: customTypes);
    }

    private BusinessAuthoringDocument ApplyDeleteCustomType(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var id = RequireString(operation, "id", "operations.data.id");
        var customTypes = document.CustomTypes.ToList();
        var index = customTypes.FindIndex(ct => string.Equals(ct.Id, id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            throw new PatchOperationException("customtype.notFound", $"CustomType '{id}' nebyl nalezen.", "operations.data.id");

        if (customTypes[index].UsageCount > 0)
            throw new PatchOperationException("customtype.inUse", $"CustomType '{customTypes[index].Name}' nelze smazat, protoze se pouziva ({customTypes[index].UsageCount}x).", "operations.data.id");

        customTypes.RemoveAt(index);
        return CopyDocument(document, customTypes: customTypes);
    }

    private BusinessAuthoringDocument ApplyIncrementCustomTypeUsage(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var name = RequireString(operation, "name", "operations.data.name");
        return UpdateCustomTypeUsage(document, name, delta: 1);
    }

    private BusinessAuthoringDocument ApplyDecrementCustomTypeUsage(BusinessAuthoringDocument document, BusinessPatchOperation operation)
    {
        var name = RequireString(operation, "name", "operations.data.name");
        return UpdateCustomTypeUsage(document, name, delta: -1);
    }

    private BusinessAuthoringDocument UpdateCustomTypeUsage(BusinessAuthoringDocument document, string name, int delta)
    {
        var customTypes = document.CustomTypes.ToList();
        var index = customTypes.FindIndex(ct => string.Equals(ct.Name, name, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return document;

        var existing = customTypes[index];
        customTypes[index] = new CustomTypeDefinition
        {
            Id = existing.Id,
            Name = existing.Name,
            UnderlyingType = existing.UnderlyingType,
            Constraints = existing.Constraints,
            Summary = existing.Summary,
            Source = existing.Source,
            IsCollection = existing.IsCollection,
            CollectionKind = existing.CollectionKind,
            UsageCount = Math.Max(0, existing.UsageCount + delta),
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        return CopyDocument(document, customTypes: customTypes);
    }

    private static IReadOnlyList<string> MergeConstraints(IReadOnlyList<string>? customTypeConstraints, IReadOnlyList<string> attributeConstraints)
    {
        var ctConstraints = customTypeConstraints ?? [];
        if (ctConstraints.Count == 0)
            return attributeConstraints;
        if (attributeConstraints.Count == 0)
            return ctConstraints;

        var merged = new List<string>(ctConstraints);
        foreach (var constraint in attributeConstraints)
        {
            if (!merged.Contains(constraint, StringComparer.OrdinalIgnoreCase))
                merged.Add(constraint);
        }
        return merged;
    }
}
