using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Nastaví nebo aktualizuje CoreDetail na atributu (immutable).
/// Používáno WriteBackService v Translatoru pro zápis Core-konkretizovaných dat.
/// </summary>
public sealed class SetCoreDetailOp : IPatchOperation
{
    public string CommandType => "SetCoreDetail";

    public string EntityId { get; }
    public string AttributeId { get; }
    public BusinessAttributeCoreDetail CoreDetail { get; }

    public SetCoreDetailOp(string entityId, string attributeId, BusinessAttributeCoreDetail coreDetail)
    {
        EntityId = entityId;
        AttributeId = attributeId;
        CoreDetail = coreDetail;
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
                            .Select(a => a.Id == AttributeId
                                ? a with { CoreDetail = CoreDetail }
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
        Payload = $"{CoreDetail.Source}|{CoreDetail.ResolvedPresetId ?? ""}|{CoreDetail.ValueObjectName ?? ""}|{CoreDetail.IsStrongType}|{CoreDetail.LastSyncedAt:O}",
    };
}
