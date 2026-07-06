using MetaForge.Cli.Formatting;
using MetaForge.Translator.Host;

namespace MetaForge.Cli.Commands;

/// <summary>
/// CLI command pro přidání entity — samostatná třída s DI.
/// </summary>
public sealed class AddEntityCommand
{
    private readonly BusinessAuthoringHostFacade _facade;

    public AddEntityCommand(BusinessAuthoringHostFacade facade)
    {
        _facade = facade;
    }

    public void Execute(string name)
    {
        var id = _facade.AddEntity(name);
        CliOutputFormatter.Success($"Entita '{name}' vytvořena (ID: {id})");
    }
}

/// <summary>
/// CLI command pro výpis entit.
/// </summary>
public sealed class ListEntitiesCommand
{
    private readonly BusinessAuthoringHostFacade _facade;

    public ListEntitiesCommand(BusinessAuthoringHostFacade facade)
    {
        _facade = facade;
    }

    public void Execute()
    {
        CliOutputFormatter.RenderEntityList(_facade.GetProjection());
    }
}

/// <summary>
/// CLI command pro zobrazení projekce.
/// </summary>
public sealed class ProjectionCommand
{
    private readonly BusinessAuthoringHostFacade _facade;

    public ProjectionCommand(BusinessAuthoringHostFacade facade)
    {
        _facade = facade;
    }

    public void Execute(string? entityName = null)
    {
        var view = _facade.GetProjection();
        if (!string.IsNullOrWhiteSpace(entityName))
            CliOutputFormatter.RenderEntityTable(view, entityName);
        else
        {
            CliOutputFormatter.RenderHeader(view);
            CliOutputFormatter.RenderEntityList(view);
        }
    }
}

/// <summary>
/// CLI command pro přidání atributu.
/// </summary>
public sealed class AddAttributeCommand
{
    private readonly BusinessAuthoringHostFacade _facade;

    public AddAttributeCommand(BusinessAuthoringHostFacade facade)
    {
        _facade = facade;
    }

    public void Execute(string entityId, string name, string type = "string", bool required = false)
    {
        var id = _facade.AddAttribute(entityId, name, type, required);
        CliOutputFormatter.Success($"Atribut '{name}' (typ: {type}) přidán (ID: {id})");
    }
}

/// <summary>
/// CLI command pro smazání entity.
/// </summary>
public sealed class DeleteEntityCommand
{
    private readonly BusinessAuthoringHostFacade _facade;

    public DeleteEntityCommand(BusinessAuthoringHostFacade facade)
    {
        _facade = facade;
    }

    public void Execute(string id)
    {
        _facade.DeleteEntity(id);
        CliOutputFormatter.Success($"Entita '{id}' smazána.");
    }
}
