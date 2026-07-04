using System.Text;
using MetaForge.BusinessModel;
using MetaForge.Core.Catalog;

namespace MetaForge.Translator;

public sealed class ExpertProjectionView
{
    public string SchemaVersion { get; init; } = string.Empty;

    public string ProjectName { get; init; } = string.Empty;

    public string? ProjectDescription { get; init; }

    public string? ProjectIcon { get; init; }

    public int EntityCount { get; init; }

    public int RelationCount { get; init; }

    public int OpenQuestionCount { get; init; }

    public IReadOnlyList<ExpertEntityProjection> Entities { get; init; } = [];

    public IReadOnlyList<ExpertRelationProjection> Relations { get; init; } = [];

    public IReadOnlyList<ExpertPendingQuestionProjection> PendingQuestions { get; init; } = [];

    public ExpertProjectionDiagnostics Diagnostics { get; init; } = new();

    public ExpertReplayProjectionInfo? Replay { get; init; }
}

public sealed class ExpertProjectionDiagnostics
{
    public int ProjectNoteCount { get; init; }

    public int TotalNoteCount { get; init; }

    public int AttributesWithConstraintsCount { get; init; }

    public int AttributesWithComputedExpressionCount { get; init; }

    public int AttributesWithExplicitPresetCount { get; init; }

    public int BehaviorsWithInputsCount { get; init; }

    public int TotalBehaviorInputCount { get; init; }

    public int BehaviorsWithReturnsCount { get; init; }

    public int RelationsWithoutFullNavigationCount { get; init; }

    public int ResolvedQuestionCount { get; init; }

    public int DismissedQuestionCount { get; init; }
}

public sealed class ExpertEntityProjection
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? PresetId { get; init; }

    public int NoteCount { get; init; }

    public IReadOnlyList<ExpertAttributeProjection> Attributes { get; init; } = [];

    public IReadOnlyList<ExpertBehaviorProjection> Behaviors { get; init; } = [];
}

public sealed class ExpertAttributeProjection
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string BusinessType { get; init; } = string.Empty;

    public ExpertTypeResolutionKind ResolutionKind { get; init; }

    public string ResolvedType { get; init; } = string.Empty;

    public string UnderlyingType { get; init; } = string.Empty;

    public string? CatalogId { get; init; }

    public string? CatalogDisplayName { get; init; }

    public IReadOnlyList<string> CatalogTags { get; init; } = [];

    public string? SuggestedPresetId { get; init; }

    public string? SuggestedPresetDisplayName { get; init; }

    public IReadOnlyList<ExpertSuggestedPreset> SuggestedPresets { get; init; } = [];

    public string? StrongTypeCandidateName { get; init; }

    public string? PresetId { get; init; }

    public string? CustomType { get; init; }

    public bool Required { get; init; }

    public IReadOnlyList<string> DeclaredConstraints { get; init; } = [];

    public IReadOnlyList<string> CandidateValidationRules { get; init; } = [];

    public string? Summary { get; init; }

    public string? DefaultValue { get; init; }

    public string? ComputedExpression { get; init; }

    public AttributeSyncState? SyncState { get; init; }

    public BusinessAttributeCoreDetail? CoreDetail { get; init; }
}

public sealed class ExpertBehaviorProjection
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public BusinessBehaviorKind Kind { get; init; }

    public string? Summary { get; init; }

    public string? Returns { get; init; }

    public int InputCount { get; init; }

    public IReadOnlyList<ExpertBehaviorInputProjection> Inputs { get; init; } = [];

    public int NoteCount { get; init; }
}

public sealed class ExpertBehaviorInputProjection
{
    public string Name { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public bool Required { get; init; }

    public string? Summary { get; init; }
}

public sealed class ExpertSuggestedPreset
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed class ExpertRelationProjection
{
    public string Id { get; init; } = string.Empty;

    public string SourceEntityName { get; init; } = string.Empty;

    public string TargetEntityName { get; init; } = string.Empty;

    public BusinessRelationKind Kind { get; init; }

    public string? SourceNavigationName { get; init; }

    public string? TargetNavigationName { get; init; }

    public int NoteCount { get; init; }
}

public sealed class ExpertPendingQuestionProjection
{
    public string Id { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;

    public PendingQuestionStatus Status { get; init; }

    public PendingQuestionScope Scope { get; init; }

    public string? RelatedEntityName { get; init; }

    public string? RelatedAttributeName { get; init; }

    public string? RelatedBehaviorName { get; init; }

    public string? RelatedRelationSummary { get; init; }
}

public sealed class ExpertReplayProjectionInfo
{
    public string? StreamId { get; init; }

    public int TotalCommandCount { get; init; }

    public int ReplayedCommandCount { get; init; }

    public int CheckpointCommandCount { get; init; }

    public bool UsedCheckpoint { get; init; }
}

public enum ExpertTypeResolutionKind
{
    Primitive,
    Catalog,
    Custom,
    Fallback,
}

internal sealed class ExpertProjectionBuilder
{
    private readonly CatalogManager _catalogManager;

    public ExpertProjectionBuilder(CatalogManager catalogManager)
    {
        _catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
    }

    public ExpertProjectionView Build(BusinessAuthoringDocument document, BusinessProjectionView? projection = null, ProjectionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        var effectiveOptions = options ?? ProjectionOptions.Expert();

        var entityLookup = document.Entities.ToDictionary(entity => entity.Id, StringComparer.OrdinalIgnoreCase);
        var attributeLookup = document.Entities
            .SelectMany(entity => entity.Attributes.Select(attribute => (entity.Id, Attribute: attribute)))
            .ToDictionary(item => (item.Id, item.Attribute.Id), item => item.Attribute, EqualityComparer<(string, string)>.Default);
        var behaviorLookup = document.Entities
            .SelectMany(entity => entity.Behaviors.Select(behavior => (entity.Id, Behavior: behavior)))
            .ToDictionary(item => (item.Id, item.Behavior.Id), item => item.Behavior, EqualityComparer<(string, string)>.Default);
        var relationLookup = document.Relations.ToDictionary(relation => relation.Id, StringComparer.OrdinalIgnoreCase);
        var presetCache = new Dictionary<string, ValueObjectPreset?>(StringComparer.OrdinalIgnoreCase);
        var entities = document.Entities.Select(entity => BuildEntity(entity, presetCache, effectiveOptions)).ToArray();
        var relations = effectiveOptions.RelationAnalysis
            ? document.Relations.Select(relation => BuildRelation(relation, entityLookup)).ToArray()
            : [];
        var pendingQuestions = document.PendingQuestions
            .Select(question => BuildQuestion(question, entityLookup, attributeLookup, behaviorLookup, relationLookup))
            .ToArray();
        var attributes = entities.SelectMany(entity => entity.Attributes).ToArray();
        var behaviors = entities.SelectMany(entity => entity.Behaviors).ToArray();

        return new ExpertProjectionView
        {
            SchemaVersion = document.SchemaVersion,
            ProjectName = document.Project.Name,
            ProjectDescription = document.Project.Description,
            ProjectIcon = document.Project.Icon,
            EntityCount = document.Entities.Count,
            RelationCount = document.Relations.Count,
            OpenQuestionCount = document.PendingQuestions.Count(question => question.Status == PendingQuestionStatus.Open),
            Entities = entities,
            Relations = relations,
            PendingQuestions = pendingQuestions,
            Diagnostics = effectiveOptions.Diagnostics
                ? new ExpertProjectionDiagnostics
                {
                    ProjectNoteCount = document.Notes.Count,
                    TotalNoteCount = document.Notes.Count + entities.Sum(entity => entity.NoteCount) + behaviors.Sum(behavior => behavior.NoteCount) + relations.Sum(relation => relation.NoteCount),
                    AttributesWithConstraintsCount = attributes.Count(attribute => attribute.DeclaredConstraints.Count > 0),
                    AttributesWithComputedExpressionCount = attributes.Count(attribute => !string.IsNullOrWhiteSpace(attribute.ComputedExpression)),
                    AttributesWithExplicitPresetCount = attributes.Count(attribute => !string.IsNullOrWhiteSpace(attribute.PresetId)),
                    BehaviorsWithInputsCount = behaviors.Count(behavior => behavior.InputCount > 0),
                    TotalBehaviorInputCount = behaviors.Sum(behavior => behavior.InputCount),
                    BehaviorsWithReturnsCount = behaviors.Count(behavior => !string.IsNullOrWhiteSpace(behavior.Returns)),
                    RelationsWithoutFullNavigationCount = relations.Count(relation => string.IsNullOrWhiteSpace(relation.SourceNavigationName) || string.IsNullOrWhiteSpace(relation.TargetNavigationName)),
                    ResolvedQuestionCount = pendingQuestions.Count(question => question.Status == PendingQuestionStatus.Resolved),
                    DismissedQuestionCount = pendingQuestions.Count(question => question.Status == PendingQuestionStatus.Dismissed),
                }
                : new ExpertProjectionDiagnostics(),
            Replay = projection is null
                ? null
                : new ExpertReplayProjectionInfo
                {
                    StreamId = projection.StreamId,
                    TotalCommandCount = projection.TotalCommandCount,
                    ReplayedCommandCount = projection.ReplayedCommandCount,
                    CheckpointCommandCount = projection.CheckpointCommandCount,
                    UsedCheckpoint = projection.UsedCheckpoint,
                },
        };
    }

    private ExpertEntityProjection BuildEntity(BusinessEntityNode entity, IDictionary<string, ValueObjectPreset?> presetCache, ProjectionOptions options)
    {
        return new ExpertEntityProjection
        {
            Id = entity.Id,
            Name = entity.Name,
            PresetId = entity.PresetId,
            NoteCount = entity.Notes.Count,
            Attributes = entity.Attributes.Select(attribute => BuildAttribute(attribute, presetCache, options)).ToArray(),
            Behaviors = entity.Behaviors.Select(behavior => new ExpertBehaviorProjection
            {
                Id = behavior.Id,
                Name = behavior.Name,
                Kind = behavior.Kind,
                Summary = behavior.Summary,
                Returns = behavior.Returns,
                InputCount = behavior.Inputs.Count,
                Inputs = behavior.Inputs.Select(input => new ExpertBehaviorInputProjection
                {
                    Name = input.Name,
                    Type = input.Type,
                    Required = input.Required,
                    Summary = input.Summary,
                }).ToArray(),
                NoteCount = behavior.Notes.Count,
            }).ToArray(),
        };
    }

    private ExpertAttributeProjection BuildAttribute(BusinessAttributeNode attribute, IDictionary<string, ValueObjectPreset?> presetCache, ProjectionOptions options)
    {
        var resolution = options.TypeResolution || options.Suggestions
            ? _catalogManager.ResolveType(attribute.Type)
            : TypeResolution.Unresolved;
        var suggestedPresets = options.Suggestions
            ? _catalogManager.SuggestPresets(attribute.Name, attribute.Type)
            : [];
        var suggestedPreset = suggestedPresets.Count > 0 ? suggestedPresets[0] : null;
        var resolvedCatalogItem = !options.TypeResolution || resolution.CatalogId is null ? null : _catalogManager.FindById(resolution.CatalogId);
        var resolvedPreset = !options.TypeResolution || resolution.CatalogId is null ? null : LoadPreset(resolution.CatalogId, presetCache);
        var suggestedPresetDefinition = !options.Suggestions || suggestedPreset is null ? null : LoadPreset(suggestedPreset.Id, presetCache);
        var ruleSource = options.Suggestions ? (resolvedPreset ?? suggestedPresetDefinition) : null;

        return new ExpertAttributeProjection
        {
            Id = attribute.Id,
            Name = attribute.Name,
            BusinessType = attribute.Type,
            ResolutionKind = ResolveKind(attribute, resolution),
            ResolvedType = ResolveResolvedType(attribute, resolution),
            UnderlyingType = ResolveUnderlyingType(attribute, resolution, resolvedPreset),
            CatalogId = resolution.CatalogId,
            CatalogDisplayName = resolvedCatalogItem?.DisplayName,
            CatalogTags = resolvedCatalogItem?.Tags.ToArray() ?? [],
            SuggestedPresetId = suggestedPreset?.Id,
            SuggestedPresetDisplayName = suggestedPreset?.DisplayName,
            SuggestedPresets = suggestedPresets.Select(s => new ExpertSuggestedPreset
            {
                Id = s.Id,
                DisplayName = s.DisplayName,
                Tags = s.Tags.ToArray(),
            }).ToArray(),
            StrongTypeCandidateName = ResolveStrongTypeCandidateName(attribute, resolvedPreset, suggestedPresetDefinition),
            PresetId = attribute.PresetId,
            CustomType = attribute.CustomType,
            Required = attribute.Required,
            DeclaredConstraints = attribute.Constraints.ToArray(),
            CandidateValidationRules = ruleSource?.Definition.ValidationRules.Select(FormatRule).ToArray() ?? [],
            Summary = attribute.Summary,
            DefaultValue = attribute.DefaultValue,
            ComputedExpression = attribute.Computed,
            SyncState = attribute.CoreDetail?.SyncState,
            CoreDetail = attribute.CoreDetail,
        };
    }

    private static ExpertRelationProjection BuildRelation(
        BusinessRelationNode relation,
        IReadOnlyDictionary<string, BusinessEntityNode> entityLookup)
    {
        var sourceName = entityLookup.TryGetValue(relation.SourceEntityId, out var sourceEntity)
            ? sourceEntity.Name
            : relation.SourceEntityId;
        var targetName = entityLookup.TryGetValue(relation.TargetEntityId, out var targetEntity)
            ? targetEntity.Name
            : relation.TargetEntityId;

        return new ExpertRelationProjection
        {
            Id = relation.Id,
            SourceEntityName = sourceName,
            TargetEntityName = targetName,
            Kind = relation.Kind,
            SourceNavigationName = relation.SourceNavigationName,
            TargetNavigationName = relation.TargetNavigationName,
            NoteCount = relation.Notes.Count,
        };
    }

    private static ExpertPendingQuestionProjection BuildQuestion(
        PendingQuestionNode question,
        IReadOnlyDictionary<string, BusinessEntityNode> entityLookup,
        IReadOnlyDictionary<(string, string), BusinessAttributeNode> attributeLookup,
        IReadOnlyDictionary<(string, string), BusinessBehaviorNode> behaviorLookup,
        IReadOnlyDictionary<string, BusinessRelationNode> relationLookup)
    {
        entityLookup.TryGetValue(question.RelatedEntityId ?? string.Empty, out var relatedEntity);

        BusinessAttributeNode? relatedAttribute = null;
        if (relatedEntity is not null && !string.IsNullOrWhiteSpace(question.RelatedAttributeId))
            attributeLookup.TryGetValue((relatedEntity.Id, question.RelatedAttributeId), out relatedAttribute);

        BusinessBehaviorNode? relatedBehavior = null;
        if (relatedEntity is not null && !string.IsNullOrWhiteSpace(question.RelatedBehaviorId))
            behaviorLookup.TryGetValue((relatedEntity.Id, question.RelatedBehaviorId), out relatedBehavior);

        string? relatedRelationSummary = null;
        if (!string.IsNullOrWhiteSpace(question.RelatedRelationId)
            && relationLookup.TryGetValue(question.RelatedRelationId, out var relation))
        {
            var sourceName = entityLookup.TryGetValue(relation.SourceEntityId, out var sourceEntity)
                ? sourceEntity.Name
                : relation.SourceEntityId;
            var targetName = entityLookup.TryGetValue(relation.TargetEntityId, out var targetEntity)
                ? targetEntity.Name
                : relation.TargetEntityId;
            relatedRelationSummary = $"{sourceName} -> {targetName} [{relation.Kind}]";
        }

        return new ExpertPendingQuestionProjection
        {
            Id = question.Id,
            Text = question.Text,
            Status = question.Status,
            Scope = question.Scope,
            RelatedEntityName = relatedEntity?.Name,
            RelatedAttributeName = relatedAttribute?.Name,
            RelatedBehaviorName = relatedBehavior?.Name,
            RelatedRelationSummary = relatedRelationSummary,
        };
    }

    private static ExpertTypeResolutionKind ResolveKind(BusinessAttributeNode attribute, TypeResolution resolution)
    {
        if (resolution.IsPrimitive)
            return ExpertTypeResolutionKind.Primitive;

        if (resolution.IsStrongType)
            return ExpertTypeResolutionKind.Catalog;

        if (!string.IsNullOrWhiteSpace(attribute.CustomType))
            return ExpertTypeResolutionKind.Custom;

        return ExpertTypeResolutionKind.Fallback;
    }

    private static string ResolveResolvedType(BusinessAttributeNode attribute, TypeResolution resolution)
    {
        if (resolution.IsPrimitive)
            return CatalogManager.GetPrimitiveName(resolution.Primitive!.Value) ?? resolution.Primitive.Value.ToString();

        if (resolution.IsStrongType)
            return resolution.CatalogId ?? attribute.Type;

        if (!string.IsNullOrWhiteSpace(attribute.CustomType))
            return attribute.CustomType;

        return "string";
    }

    private static string ResolveUnderlyingType(
        BusinessAttributeNode attribute,
        TypeResolution resolution,
        ValueObjectPreset? resolvedPreset)
    {
        if (resolution.IsPrimitive)
            return CatalogManager.GetPrimitiveName(resolution.Primitive!.Value) ?? resolution.Primitive.Value.ToString();

        if (resolvedPreset is not null)
            return resolvedPreset.Definition.UnderlyingType;

        if (!string.IsNullOrWhiteSpace(attribute.CustomType))
            return attribute.CustomType;

        return "string";
    }

    private static string? ResolveStrongTypeCandidateName(
        BusinessAttributeNode attribute,
        ValueObjectPreset? resolvedPreset,
        ValueObjectPreset? suggestedPreset)
    {
        if (resolvedPreset is not null)
            return resolvedPreset.Definition.Name;

        if (!string.IsNullOrWhiteSpace(attribute.CustomType))
            return attribute.CustomType;

        return suggestedPreset?.Definition.Name;
    }

    private ValueObjectPreset? LoadPreset(string presetId, IDictionary<string, ValueObjectPreset?> presetCache)
    {
        // NOTE: async-over-sync — LoadValueObjectPresetAsync is async but called from
        // synchronous Build pipeline. Resolve by making Build/ExpertProjectionBuilder async.
        if (presetCache.TryGetValue(presetId, out var cached))
            return cached;

        try
        {
            cached = _catalogManager.LoadValueObjectPresetAsync(presetId).GetAwaiter().GetResult();
        }
        catch
        {
            cached = null;
        }

        presetCache[presetId] = cached;
        return cached;
    }

    private static string FormatRule(ValidationRulePreset rule)
    {
        return string.IsNullOrWhiteSpace(rule.Parameter)
            ? rule.RuleType
            : $"{rule.RuleType}({rule.Parameter})";
    }
}

public static class ExpertProjectionRenderer
{
    public static string Render(ExpertProjectionView projection)
    {
        ArgumentNullException.ThrowIfNull(projection);

        var sb = new StringBuilder();
        sb.AppendLine($"Expert projection for {projection.ProjectName}");
        sb.AppendLine($"Summary: entities {projection.EntityCount} | relations {projection.RelationCount} | open questions {projection.OpenQuestionCount}");

        if (!string.IsNullOrWhiteSpace(projection.ProjectDescription) || !string.IsNullOrWhiteSpace(projection.ProjectIcon))
            sb.AppendLine($"Project meta: description={projection.ProjectDescription ?? "<none>"} | icon={projection.ProjectIcon ?? "<none>"}");

        sb.AppendLine($"Diagnostics: notes {projection.Diagnostics.TotalNoteCount} (project {projection.Diagnostics.ProjectNoteCount}) | constrained attrs {projection.Diagnostics.AttributesWithConstraintsCount} | computed attrs {projection.Diagnostics.AttributesWithComputedExpressionCount} | preset attrs {projection.Diagnostics.AttributesWithExplicitPresetCount} | behaviors with inputs {projection.Diagnostics.BehaviorsWithInputsCount}/{projection.Diagnostics.TotalBehaviorInputCount} | behaviors with returns {projection.Diagnostics.BehaviorsWithReturnsCount} | relations missing nav {projection.Diagnostics.RelationsWithoutFullNavigationCount} | resolved questions {projection.Diagnostics.ResolvedQuestionCount} | dismissed questions {projection.Diagnostics.DismissedQuestionCount}");

        if (projection.Replay is not null)
        {
            sb.AppendLine($"Replay: total {projection.Replay.TotalCommandCount} | replayed {projection.Replay.ReplayedCommandCount} | checkpoint {projection.Replay.CheckpointCommandCount} | used checkpoint {projection.Replay.UsedCheckpoint}");
        }

        sb.AppendLine();
        sb.AppendLine("Entities:");

        foreach (var entity in projection.Entities)
        {
            sb.AppendLine($"- {entity.Name} [attributes {entity.Attributes.Count}, behaviors {entity.Behaviors.Count}, notes {entity.NoteCount}]");

            foreach (var attribute in entity.Attributes)
            {
                sb.AppendLine($"  - {attribute.Name}: type={attribute.BusinessType} | resolution={attribute.ResolutionKind} | resolved={attribute.ResolvedType} | underlying={attribute.UnderlyingType}");

                if (!string.IsNullOrWhiteSpace(attribute.StrongTypeCandidateName))
                    sb.AppendLine($"    strong type candidate: {attribute.StrongTypeCandidateName}");

                if (!string.IsNullOrWhiteSpace(attribute.CatalogId))
                    sb.AppendLine($"    catalog: {attribute.CatalogId} ({attribute.CatalogDisplayName})");

                if (!string.IsNullOrWhiteSpace(attribute.SuggestedPresetId))
                    sb.AppendLine($"    suggested preset: {attribute.SuggestedPresetId} ({attribute.SuggestedPresetDisplayName})");

                if (attribute.DeclaredConstraints.Count > 0)
                    sb.AppendLine($"    declared constraints: {string.Join(", ", attribute.DeclaredConstraints)}");

                if (attribute.CandidateValidationRules.Count > 0)
                    sb.AppendLine($"    candidate validation rules: {string.Join(", ", attribute.CandidateValidationRules)}");
            }

            foreach (var behavior in entity.Behaviors)
            {
                sb.AppendLine($"  - behavior {behavior.Name}: kind={behavior.Kind} | inputs={behavior.InputCount} | returns={behavior.Returns ?? "<none>"} | notes={behavior.NoteCount}");

                foreach (var input in behavior.Inputs)
                {
                    sb.AppendLine($"    input {input.Name}: type={input.Type} | required={input.Required} | summary={input.Summary ?? "<none>"}");
                }
            }
        }

        if (projection.Relations.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Relations:");

            foreach (var relation in projection.Relations)
            {
                sb.AppendLine($"- {relation.SourceEntityName} -> {relation.TargetEntityName} [{relation.Kind}] | sourceNav={relation.SourceNavigationName ?? "<none>"} | targetNav={relation.TargetNavigationName ?? "<none>"} | notes={relation.NoteCount}");
            }
        }

        if (projection.PendingQuestions.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Pending questions:");

            foreach (var question in projection.PendingQuestions)
            {
                var relatedTarget = question.RelatedRelationSummary
                    ?? question.RelatedBehaviorName
                    ?? question.RelatedAttributeName
                    ?? question.RelatedEntityName
                    ?? "<none>";
                sb.AppendLine($"- {question.Status}/{question.Scope}: {question.Text} | target={relatedTarget}");
            }
        }

        return sb.ToString().TrimEnd();
    }
}