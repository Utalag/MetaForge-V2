using MetaForge.BusinessModel;
using MetaForge.Core.Catalog;
using MetaForge.Core.Common;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Dto;

namespace MetaForge.Translator;

/// <summary>
/// Deterministic business model translator.
/// Maps BusinessAuthoringDocument to MetaForgeTransportDto without AI.
/// Uses CatalogManager.ResolveType() for type resolution (Návrh 4).
/// </summary>
public class DefaultBusinessTranslator : IBusinessTranslator
{
    private readonly CatalogManager _catalog;
    private readonly BusinessDocumentValidator _validator = new();

    public DefaultBusinessTranslator(CatalogManager catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    public MetaForgeTransportDto Translate(BusinessAuthoringDocument document, ProgramLanguage language = ProgramLanguage.CSharp)
    {
        ArgumentNullException.ThrowIfNull(document);

        EnsureValidDocument(document);

        var entityLookup = document.Entities.ToDictionary(entity => entity.Id, StringComparer.OrdinalIgnoreCase);
        var classes = new List<TransportClassDto>();

        foreach (var entity in document.Entities)
        {
            var properties = new List<TransportPropertyDto>
            {
                CreatePrimitiveProperty("Id", DataType.Guid, isRequired: true, language)
            };

            foreach (var attribute in entity.Attributes)
            {
                properties.Add(CreateAttributeProperty(attribute, language));
            }

            foreach (var relation in document.Relations)
            {
                if (string.Equals(relation.SourceEntityId, entity.Id, StringComparison.OrdinalIgnoreCase))
                {
                    var sourceProperty = CreateNavigationProperty(relation, entityLookup, isSource: true, language);
                    if (sourceProperty is not null)
                    {
                        properties.Add(sourceProperty);
                    }
                }

                if (string.Equals(relation.TargetEntityId, entity.Id, StringComparison.OrdinalIgnoreCase))
                {
                    var targetProperty = CreateNavigationProperty(relation, entityLookup, isSource: false, language);
                    if (targetProperty is not null)
                    {
                        properties.Add(targetProperty);
                    }
                }
            }

            classes.Add(new TransportClassDto
            {
                Name = entity.Name,
                Namespace = $"{document.Project.Name}.Domain",
                Summary = entity.Summary,
                Icon = entity.Icon,
                PresetId = entity.PresetId,
                Properties = properties,
                Methods = TranslateBehaviors(entity.Behaviors, language),
                TargetLanguage = language,
            });
        }

        return new MetaForgeTransportDto
        {
            Name = document.Project.Name,
            Description = document.Project.Description,
            Icon = document.Project.Icon,
            Version = document.Project.Version,
            TargetLanguage = language,
            SourceFormat = "BusinessAuthoringDocument",
            Classes = classes,
        };
    }

    private void EnsureValidDocument(BusinessAuthoringDocument document)
    {
        var blockingIssues = _validator.Validate(document)
            .Where(issue => string.Equals(issue.Severity, "Error", StringComparison.OrdinalIgnoreCase))
            .Select(issue => string.IsNullOrWhiteSpace(issue.Path)
                ? issue.Message
                : $"{issue.Message} ({issue.Path})")
            .ToArray();

        if (blockingIssues.Length == 0)
        {
            return;
        }

        throw new InvalidOperationException($"Business authoring document is invalid: {string.Join("; ", blockingIssues)}");
    }

    private TransportPropertyDto CreateAttributeProperty(BusinessAttributeNode attribute, ProgramLanguage language)
    {
        var resolution = _catalog.ResolveType(attribute.Type);

        TransportStrongTypeDto strongType;
        if (resolution.IsPrimitive)
        {
            strongType = CreateStrongType(
                resolution.Primitive!.Value,
                isNullable: !attribute.Required,
                language: language);
        }
        else if (resolution.IsStrongType)
        {
            var catalogItem = _catalog.FindById(resolution.CatalogId!);
            strongType = CreateStrongType(
                DataType.String,
                isNullable: !attribute.Required,
                language: language,
                strongTypeName: catalogItem?.DisplayName ?? resolution.CatalogId);
        }
        else if (!string.IsNullOrWhiteSpace(attribute.CustomType))
        {
            strongType = CreateStrongType(
                DataType.Custom,
                isNullable: !attribute.Required,
                language: language,
                customTypeName: attribute.CustomType,
                entityKind: EntityKind.Class);
        }
        else
        {
            strongType = CreateStrongType(
                DataType.String,
                isNullable: !attribute.Required,
                language: language);
        }

        return new TransportPropertyDto
        {
            TargetLanguage = language,
            Name = attribute.Name,
            StrongType = strongType,
            Summary = attribute.Summary,
            DefaultValue = attribute.DefaultValue,
            ComputedExpression = attribute.Computed,
        };
    }

    private static TransportPropertyDto CreatePrimitiveProperty(string name, DataType dataType, bool isRequired, ProgramLanguage language)
    {
        return new TransportPropertyDto
        {
            TargetLanguage = language,
            Name = name,
            StrongType = CreateStrongType(dataType, isNullable: !isRequired, language: language),
        };
    }

    private static TransportStrongTypeDto CreateStrongType(
        DataType baseType,
        bool isNullable,
        ProgramLanguage language,
        string? customTypeName = null,
        EntityKind entityKind = EntityKind.Primitive,
        bool isCollection = false,
        SemanticCollection semanticCollection = SemanticCollection.None,
        string? strongTypeName = null)
    {
        return new TransportStrongTypeDto
        {
            Name = strongTypeName ?? string.Empty,
            TargetLanguage = language,
            UnderlyingType = new TransportTypeModelDto
            {
                BaseType = baseType,
                CustomTypeName = customTypeName ?? string.Empty,
                IsNullable = isNullable,
                IsCollection = isCollection,
                SemanticCollection = semanticCollection,
                EntityKind = entityKind,
                TargetLanguage = language,
            },
        };
    }

    private static TransportPropertyDto? CreateNavigationProperty(
        BusinessRelationNode relation,
        IReadOnlyDictionary<string, BusinessEntityNode> entityLookup,
        bool isSource,
        ProgramLanguage language)
    {
        var relatedEntityId = isSource ? relation.TargetEntityId : relation.SourceEntityId;
        if (!entityLookup.TryGetValue(relatedEntityId, out var relatedEntity))
        {
            return null;
        }

        return relation.Kind switch
        {
            BusinessRelationKind.BelongsTo when isSource => new TransportPropertyDto
            {
                TargetLanguage = language,
                Name = relation.SourceNavigationName ?? $"{relatedEntity.Name}Id",
                StrongType = CreateStrongType(DataType.Guid, isNullable: false, language: language),
            },
            BusinessRelationKind.HasMany when !isSource => new TransportPropertyDto
            {
                TargetLanguage = language,
                Name = relation.TargetNavigationName ?? $"{relatedEntity.Name}s",
                StrongType = CreateStrongType(
                    DataType.Custom,
                    isNullable: false,
                    language: language,
                    customTypeName: relatedEntity.Name,
                    entityKind: EntityKind.Class,
                    isCollection: true,
                    semanticCollection: SemanticCollection.List),
            },
            BusinessRelationKind.HasOne when !isSource => new TransportPropertyDto
            {
                TargetLanguage = language,
                Name = relation.TargetNavigationName ?? relatedEntity.Name,
                StrongType = CreateStrongType(
                    DataType.Custom,
                    isNullable: false,
                    language: language,
                    customTypeName: relatedEntity.Name,
                    entityKind: EntityKind.Class),
            },
            BusinessRelationKind.ManyToMany when isSource => new TransportPropertyDto
            {
                TargetLanguage = language,
                Name = relation.SourceNavigationName ?? $"{relatedEntity.Name}s",
                StrongType = CreateStrongType(
                    DataType.Custom,
                    isNullable: false,
                    language: language,
                    customTypeName: relatedEntity.Name,
                    entityKind: EntityKind.Class,
                    isCollection: true,
                    semanticCollection: SemanticCollection.List),
            },
            BusinessRelationKind.ManyToMany when !isSource => new TransportPropertyDto
            {
                TargetLanguage = language,
                Name = relation.TargetNavigationName ?? $"{relatedEntity.Name}s",
                StrongType = CreateStrongType(
                    DataType.Custom,
                    isNullable: false,
                    language: language,
                    customTypeName: relatedEntity.Name,
                    entityKind: EntityKind.Class,
                    isCollection: true,
                    semanticCollection: SemanticCollection.List),
            },
            _ => null,
        };
    }

    private List<TransportMethodDto> TranslateBehaviors(IReadOnlyList<BusinessBehaviorNode> behaviors, ProgramLanguage language)
    {
        var methods = new List<TransportMethodDto>();

        foreach (var behavior in behaviors)
        {
            methods.Add(CreateBehaviorMethod(behavior, language));
        }

        return methods;
    }

    private TransportMethodDto CreateBehaviorMethod(BusinessBehaviorNode behavior, ProgramLanguage language)
    {
        var parameters = behavior.Inputs
            .Select(input => CreateBehaviorParameter(input, language))
            .ToList();

        var returnType = ResolveReturnType(behavior.Returns, language);
        var documentation = BuildBehaviorDocumentation(behavior);

        return new TransportMethodDto
        {
            Name = behavior.Name,
            ReturnType = returnType,
            Parameters = parameters,
            IsAsync = behavior.Kind == BusinessBehaviorKind.Command,
            Documentation = documentation,
            TargetLanguage = language,
        };
    }

    private TransportParameterDto CreateBehaviorParameter(BusinessBehaviorInputNode input, ProgramLanguage language)
    {
        var resolution = _catalog.ResolveType(input.Type);
        var typeModel = ResolveTypeModel(resolution, input.Type, language);

        return new TransportParameterDto
        {
            Name = input.Name,
            Type = typeModel,
            Description = input.Summary ?? string.Empty,
            TargetLanguage = language,
        };
    }

    private TransportTypeModelDto ResolveReturnType(string? returns, ProgramLanguage language)
    {
        if (string.IsNullOrWhiteSpace(returns) || string.Equals(returns, "void", StringComparison.OrdinalIgnoreCase))
        {
            return new TransportTypeModelDto
            {
                BaseType = DataType.Void,
                TargetLanguage = language,
            };
        }

        var resolution = _catalog.ResolveType(returns);
        return ResolveTypeModel(resolution, returns, language);
    }

    private static TransportTypeModelDto ResolveTypeModel(TypeResolution resolution, string originalTypeName, ProgramLanguage language)
    {
        if (resolution.IsPrimitive)
        {
            return new TransportTypeModelDto
            {
                BaseType = resolution.Primitive!.Value,
                TargetLanguage = language,
            };
        }

        return new TransportTypeModelDto
        {
            BaseType = DataType.Custom,
            CustomTypeName = resolution.IsStrongType
                ? resolution.CatalogId ?? originalTypeName
                : originalTypeName,
            EntityKind = EntityKind.Class,
            TargetLanguage = language,
        };
    }

    private static TransportCommentDto? BuildBehaviorDocumentation(BusinessBehaviorNode behavior)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(behavior.Summary))
            parts.Add(behavior.Summary);

        foreach (var note in behavior.Notes)
        {
            if (!string.IsNullOrWhiteSpace(note.Text))
                parts.Add(note.Text);
        }

        if (parts.Count == 0)
            return null;

        return new TransportCommentDto
        {
            Text = string.Join("\n", parts),
            CommentType = CommentType.Documentation,
        };
    }
}
