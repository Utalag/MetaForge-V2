using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Aktualizuje AttributeSyncState na CoreDetail atributu (immutable).
/// </summary>
public sealed class UpdateSyncStateOp : IPatchOperation
{
    public string CommandType => "UpdateSyncState";

    public string EntityId { get; }
    public string AttributeId { get; }
    public AttributeSyncState NewState { get; }

    public UpdateSyncStateOp(string entityId, string attributeId, AttributeSyncState newState)
    {
        EntityId = entityId;
        AttributeId = attributeId;
        NewState = newState;
    }

    public BusinessAuthoringDocument Apply(BusinessAuthoringDocument document)
    {
        return document with
        {
            Entities = document.Entities
                .Select(e => e.Id == EntityId
                    ? e with
                    {
                        Attributes = e.Attributes
                            .Select(a => a.Id == AttributeId && a.CoreDetail is not null
                                ? a with { CoreDetail = a.CoreDetail with { SyncState = NewState } }
                                : a)
                            .ToList()
                            .AsReadOnly(),
                    }
                    : e)
                .ToList()
                .AsReadOnly(),
        };
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        TargetAttributeId = AttributeId,
        Payload = NewState.ToString(),
    };
}
