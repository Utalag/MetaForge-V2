// ---------------------------------------------------------------------------
// MetaForge.Translator — ProjectionBuilder
// Builds DocumentProjection from BusinessAuthoringDocument with optional ElementIdMapping.
// Vrstva: Translator / Projections
//
// PROPOSAL: PROP-056 — Projection Unification + JSON Snapshot
// DEPENDS: PROP-060 — ElementIdMapping for CoreId resolution
// ---------------------------------------------------------------------------

using MetaForge.BusinessModel.Models;
using MetaForge.Core.DataTypes;
using MetaForge.Translator.Translation;

namespace MetaForge.Translator.Projections;

public sealed class ProjectionBuilder
{
    private readonly IBusinessTranslator _translator;
    private readonly ElementIdMapping? _idMapping;

    public ProjectionBuilder(IBusinessTranslator translator, ElementIdMapping? idMapping = null)
    {
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
        _idMapping = idMapping;
    }

    public DocumentProjection Build(BusinessAuthoringDocument document, ProjectionFilter filter)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(filter);

        var entities = new List<EntityProjection>();
        int totalAttributes = 0, withConstraints = 0, strongTypes = 0, unsynced = 0;

        foreach (var entity in document.Entities)
        {
            var attrs = new List<AttributeProjection>();
            var entityBehaviors = new List<BehaviorProjection>();

            foreach (var attr in entity.Attributes)
            {
                totalAttributes++;
                var attrProj = BuildAttributeProjection(attr, filter);
                attrs.Add(attrProj);

                if (attr.CoreDetail?.IsStrongType == true)
                    strongTypes++;
                if (attr.CoreDetail?.SyncState == AttributeSyncState.Conflict)
                    unsynced++;
                if (attr.Metadata?.ContainsKey("constraints") == true)
                    withConstraints++;
            }

            foreach (var behavior in entity.Behaviors)
            {
                entityBehaviors.Add(new BehaviorProjection
                {
                    Id = behavior.Id,
                    CoreId = _idMapping?.Resolve(behavior.Id),
                    Name = behavior.Name,
                    ReturnType = behavior.ReturnType,
                    Parameters = behavior.Parameters.Select(p => p.Name).ToList(),
                });
            }

            entities.Add(new EntityProjection
            {
                Id = entity.Id,
                CoreId = _idMapping?.Resolve(entity.Id),
                Name = entity.Name,
                Attributes = attrs,
                Behaviors = entityBehaviors,
                NoteCount = entity.Notes.Count,
            });
        }

        // Relations
        var relations = filter.IncludeRelations
            ? document.Entities
                .SelectMany(e => e.Relations.Select(r => new RelationProjection
                {
                    FromEntityId = r.FromEntityId,
                    ToEntityId = r.ToEntityId,
                    FromEntityCoreId = _idMapping?.Resolve(r.FromEntityId),
                    ToEntityCoreId = _idMapping?.Resolve(r.ToEntityId),
                    RelationType = r.RelationType,
                }))
                .ToList()
            : [];

        var projection = new DocumentProjection
        {
            SchemaVersion = "1.0",
            ProjectName = document.ProjectName,
            Entities = entities,
            Relations = relations,
            Diagnostics = filter.IncludeDiagnostics
                ? new ProjectionDiagnostics
                {
                    TotalAttributes = totalAttributes,
                    WithConstraints = withConstraints,
                    StrongTypes = strongTypes,
                    UnsyncedAttributes = unsynced,
                    BuiltAt = DateTimeOffset.UtcNow,
                }
                : null,
        };

        if (filter.IncludeFlowGraph)
        {
            projection = projection with { FlowGraph = FlowGraphBuilder.Build(document) };
        }

        return projection;
    }

    private AttributeProjection BuildAttributeProjection(BusinessAttributeNode attr, ProjectionFilter filter)
    {
        var proj = new AttributeProjection
        {
            Id = attr.Id,
            CoreId = _idMapping?.Resolve(attr.Id),
            Name = attr.Name,
            BusinessType = attr.Type,
            IsRequired = attr.IsRequired,
            MaxLength = attr.MaxLength,
            DefaultValue = attr.DefaultValue,
        };

        if (filter.AttributeDetail >= AttributeDetailLevel.NameAndType)
        {
            var coreType = _translator.Translate(attr);
            proj = proj with { CoreType = coreType.ToString() };
        }

        if (filter.IncludeCoreDetail && attr.CoreDetail is not null)
        {
            proj = proj with
            {
                CoreDetail = new CoreDetailInfo
                {
                    Source = attr.CoreDetail.Source.ToString(),
                    ResolvedPresetId = attr.CoreDetail.ResolvedPresetId,
                    ValueObjectName = attr.CoreDetail.ValueObjectName,
                    IsStrongType = attr.CoreDetail.IsStrongType,
                    LastSyncedAt = attr.CoreDetail.LastSyncedAt,
                    SyncState = attr.CoreDetail.SyncState,
                },
            };
        }

        if (filter.IncludeSyncState)
            proj = proj with { SyncState = attr.CoreDetail?.SyncState ?? AttributeSyncState.New };

        return proj;
    }
}
