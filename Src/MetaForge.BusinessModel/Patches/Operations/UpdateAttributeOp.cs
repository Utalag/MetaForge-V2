using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Aktualizuje atribut entity.
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

    public void Apply(BusinessAuthoringDocument document)
    {
        var entity = document.Entities.FirstOrDefault(e => e.Id == EntityId);
        var attr = entity?.Attributes.FirstOrDefault(a => a.Id == AttributeId);
        if (attr is null) return;

        if (!string.IsNullOrEmpty(NewName))
            attr.Name = NewName;

        if (!string.IsNullOrEmpty(NewType))
            attr.Type = NewType;

        if (IsRequired.HasValue)
            attr.IsRequired = IsRequired.Value;
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        TargetAttributeId = AttributeId,
        Payload = $"{(NewName ?? "")}|{(NewType ?? "")}|{IsRequired?.ToString() ?? ""}",
    };
}
