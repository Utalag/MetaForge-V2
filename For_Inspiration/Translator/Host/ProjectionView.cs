using MetaForge.BusinessModel;

namespace MetaForge.Translator;

/// <summary>
/// Unifikovany vysledek projekce — zakladni replay data + volitelne expert sekce.
/// Nahrazuje separatni <see cref="BusinessProjectionView"/> a <see cref="ExpertProjectionView"/>
/// na urovni Translator orchestrace.
/// </summary>
public sealed class ProjectionView
{
    /// <summary>Replay projekce z <see cref="IProjectionQueryService"/>.</summary>
    public BusinessProjectionView Replay { get; init; } = new();

    /// <summary>Expert projekce — null pokud nebyly zadany expert options.</summary>
    public ExpertProjectionView? Expert { get; init; }

    /// <summary>Workflow projekce — null pokud nebyly zadany workflow options.</summary>
    public WorkflowProjectionView? Workflow { get; init; }

    /// <summary>Authoring context projekce — null pokud nebyly zadany context options.</summary>
    public AuthoringContextView? AuthoringContext { get; init; }
}
