using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Smaže entitu a všechny její relace.
/// </summary>
public sealed class DeleteEntityOp : IPatchOperation
{
    public string CommandType => "DeleteEntity";
    public string EntityId { get; }

    public DeleteEntityOp(string entityId)
    {
        EntityId = entityId;
    }

    public void Apply(BusinessAuthoringDocument document)
    {
        document.Entities.RemoveAll(e => e.Id == EntityId);
        document.Relations.RemoveAll(r => r.FromEntityId == EntityId || r.ToEntityId == EntityId);
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        Payload = EntityId,
    };
}
