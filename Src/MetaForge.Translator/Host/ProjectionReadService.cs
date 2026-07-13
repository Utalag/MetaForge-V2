using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.Translator.Translation;

namespace MetaForge.Translator.Host;

/// <summary>
/// Vytváří projekci business modelu pro čtení.
/// Používá ReplayEngine pro rekonstrukci a DefaultBusinessTranslator pro překlad.
/// </summary>
public sealed class ProjectionReadService
{
    private readonly ReplayEngine _replayEngine;
    private readonly IBusinessTranslator _translator;

    public ProjectionReadService(ReplayEngine replayEngine, IBusinessTranslator translator)
    {
        _replayEngine = replayEngine;
        _translator = translator;
    }

    /// <summary>

    /// Vytvoří projekci z existujícího dokumentu (bez replay).
    /// </summary>
    public ProjectionView GetProjection(BusinessAuthoringDocument document)
    {
        return BuildProjection(document);
    }

    private ProjectionView BuildProjection(BusinessAuthoringDocument document)
    {
        var view = new ProjectionView
        {
            ProjectName = document.ProjectName,
            Project = new ProjectionView.ProjectInfoView
            {
                Id = document.Project.Id,
                Name = document.Project.Name,
                Description = document.Project.Description,
                Icon = document.Project.Icon,
                Version = document.Project.Version,
            },
        };

        foreach (var entity in document.Entities)
        {
            var entityProj = new ProjectionView.EntityProjection
            {
                Id = entity.Id,
                Name = entity.Name,
            };

            foreach (var attr in entity.Attributes)
            {
                var coreType = _translator.Translate(attr);

                var attrProj = new ProjectionView.AttributeProjection
                {
                    Id = attr.Id,
                    Name = attr.Name,
                    CoreType = coreType,
                    IsRequired = attr.IsRequired,
                    MaxLength = attr.MaxLength,
                    DefaultValue = attr.DefaultValue,
                };

                // Zohledni CoreDetail v projekci
                if (attr.CoreDetail is not null)
                {
                    attrProj.CoreDetail = new ProjectionView.CoreDetailInfoView
                    {
                        Source = attr.CoreDetail.Source,
                        ResolvedPresetId = attr.CoreDetail.ResolvedPresetId,
                        ValueObjectName = attr.CoreDetail.ValueObjectName,
                        IsStrongType = attr.CoreDetail.IsStrongType,
                        LastSyncedAt = attr.CoreDetail.LastSyncedAt,
                        SyncState = attr.CoreDetail.SyncState,
                    };
                }

                entityProj.Attributes.Add(attrProj);
            }

            view.Entities.Add(entityProj);
        }

        return view;
    }

    /// <summary>
    /// Vytvoří expertní projekci s diagnostikou, relacemi a workflow stavy.
    /// Respektuje ProjectionOptions pro volitelné sekce.
    /// </summary>
    public ExpertProjectionView GetExpertProjection(BusinessAuthoringDocument document, ProjectionOptions? options = null)
    {
        options ??= ProjectionOptions.Basic();

        // Entity s atributy
        var entities = new List<ExpertEntityProjection>();
        var syncedCount = 0;
        var pendingCount = 0;

        foreach (var entity in document.Entities)
        {
            var attrs = new List<ExpertAttributeProjection>();
            foreach (var attr in entity.Attributes)
            {
                var coreType = _translator.Translate(attr);
                var isStrongType = attr.CoreDetail?.IsStrongType == true;

                if (attr.CoreDetail?.SyncState == AttributeSyncState.Synced) syncedCount++;
                else pendingCount++;

                attrs.Add(new ExpertAttributeProjection
                {
                    Id = attr.Id,
                    Name = attr.Name,
                    BusinessType = attr.Type,
                    CoreType = isStrongType ? attr.CoreDetail?.ValueObjectName : coreType.ToString(),
                    IsRequired = attr.IsRequired,
                    MaxLength = attr.MaxLength,
                    DefaultValue = attr.DefaultValue,
                    IsStrongType = isStrongType,
                    SyncState = attr.CoreDetail?.SyncState ?? AttributeSyncState.New,
                    Constraints = attr.Metadata?.ContainsKey("constraints") == true
                        ? (attr.Metadata["constraints"] as IEnumerable<string>)?.ToList() ?? []
                        : [],
                });
            }

            var behaviors = options.Expert
                ? entity.Behaviors?.Select(b => new ExpertBehaviorProjection
                {
                    Id = b.Id,
                    Name = b.Name,
                    Returns = b.ReturnType,
                    Inputs = b.Parameters?.Select(p => p.Name).ToList() ?? [],
                    Constraints = [],
                }).ToList() ?? []
                : [];

            entities.Add(new ExpertEntityProjection
            {
                Id = entity.Id,
                Name = entity.Name,
                NoteCount = entity.Notes?.Count ?? 0,
                BehaviorCount = entity.Behaviors?.Count ?? 0,
                Attributes = attrs,
                Behaviors = behaviors,
            });
        }

        // Relace
        var relations = options.Expert && document.Relations is not null
            ? document.Relations.Select(r => new ExpertRelationProjection
            {
                From = r.FromEntityId,
                To = r.ToEntityId,
                Type = r.RelationType,
                Navigation = r.FromNavigationName ?? r.ToNavigationName,
            }).ToList()
            : [];

        // Pending questions
        var pendingQuestions = document.PendingQuestions?.Select(q => new ExpertPendingQuestionProjection
        {
            Id = q.Id,
            Text = q.Question,
            Context = q.ContextEntityId ?? string.Empty,
            IsBlocking = false,
        }).ToList() ?? [];

        // Diagnostika
        var allAttrs = document.Entities.SelectMany(e => e.Attributes).ToList();
        var diagnostics = new ExpertProjectionDiagnostics
        {
            TotalAttributes = allAttrs.Count,
            WithConstraints = allAttrs.Count(a => a.Metadata?.ContainsKey("constraints") == true),
            StrongTypes = allAttrs.Count(a => a.CoreDetail?.IsStrongType == true),
            PresetsUsed = allAttrs.Count(a => a.CoreDetail?.ResolvedPresetId != null),
            UnsyncedAttributes = allAttrs.Count(a => a.CoreDetail?.SyncState != AttributeSyncState.Synced),
        };

        return new ExpertProjectionView
        {
            SchemaVersion = document.SchemaVersion,
            ProjectName = document.ProjectName,
            EntityCount = document.Entities.Count,
            RelationCount = document.Relations?.Count ?? 0,
            OpenQuestionCount = document.PendingQuestions?.Count ?? 0,
            SyncedAttributeCount = syncedCount,
            PendingAttributeCount = pendingCount,
            Entities = entities,
            Relations = relations,
            PendingQuestions = pendingQuestions,
            Diagnostics = diagnostics,
        };
    }
}
