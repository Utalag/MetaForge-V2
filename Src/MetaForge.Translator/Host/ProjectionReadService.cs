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
    /// Vytvoří aktuální projekci — přehraje commandy a přeloží.
    /// </summary>
    public ProjectionView GetProjection(CommandLogStore logStore)
    {
        var commands = logStore.GetAll();
        var document = _replayEngine.Replay(commands);
        return BuildProjection(document);
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

                entityProj.Attributes.Add(attrProj);
            }

            view.Entities.Add(entityProj);
        }

        return view;
    }
}
