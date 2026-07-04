using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Aktualizuje atribut entity (immutable).
/// </summary>
public sealed class UpdateAttributeOp : IPatchOperation
{
    public string CommandType => "UpdateAttribute";
    public string EntityId { get; }
    public string AttributeId { get; }
    public string? NewName { get; }
    public string? NewType { get; }
    public bool? IsRequired { get; }

    public UpdateAttributeOp(string entityId, string attributeId, string? newName = null, string? newType = null, bool? isRequired = null)
    {
        EntityId = entityId;
        AttributeId = attributeId;
        NewName = newName;
        NewType = newType;
        IsRequired = isRequired;
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
                                ? a with
                                {
                                    Name = !string.IsNullOrEmpty(NewName) ? NewName : a.Name,
                                    Type = !string.IsNullOrEmpty(NewType) ? NewType : a.Type,
                                    IsRequired = IsRequired ?? a.IsRequired,
                                }
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
        Payload = $"{(NewName ?? "")}|{(NewType ?? "")}|{IsRequired?.ToString() ?? ""}",
    };
}
