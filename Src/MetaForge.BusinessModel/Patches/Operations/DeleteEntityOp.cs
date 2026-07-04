using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Smaže entitu a všechny její relace (immutable).
/// </summary>
public sealed class DeleteEntityOp : IPatchOperation
{
    public string CommandType => "DeleteEntity";
    public string EntityId { get; }

    public DeleteEntityOp(string entityId)
    {
        EntityId = entityId;
    }

    public BusinessAuthoringDocument Apply(BusinessAuthoringDocument document)
    {
        return document with
        {
            Entities = document.Entities
                .Where(e => e.Id != EntityId)
                .ToList()
                .AsReadOnly(),
            Relations = document.Relations
                .Where(r => r.FromEntityId != EntityId && r.ToEntityId != EntityId)
                .ToList()
                .AsReadOnly(),
        };
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        Payload = EntityId,
    };
}
