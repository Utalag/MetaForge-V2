using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Aktualizuje název existující entity.
/// </summary>
public sealed class UpdateEntityOp : IPatchOperation
{
    public string CommandType => "UpdateEntity";
    public string EntityId { get; }
    public string NewName { get; }

    public UpdateEntityOp(string entityId, string newName)
    {
        EntityId = entityId;
        NewName = newName;
    }

    public void Apply(BusinessAuthoringDocument document)
    {
        var entity = document.Entities.FirstOrDefault(e => e.Id == EntityId);
        if (entity is not null)
            entity.Name = NewName;
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        Payload = NewName,
    };
}
