using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Aktualizuje název existující entity (immutable).
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

    public BusinessAuthoringDocument Apply(BusinessAuthoringDocument document)
    {
        return document with
        {
            Entities = document.Entities
                .Select(e => e.Id == EntityId ? e with { Name = NewName } : e)
                .ToList()
                .AsReadOnly(),
        };
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        Payload = NewName,
    };
}
