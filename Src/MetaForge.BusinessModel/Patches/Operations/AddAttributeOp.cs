using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Přidá atribut k entitě.
/// </summary>
public sealed class AddAttributeOp : IPatchOperation
{
    public string CommandType => "AddAttribute";
    public string EntityId { get; }
    public string AttributeId { get; }
    public string AttributeName { get; }
    public string AttributeType { get; }
    public bool IsRequired { get; }

    public AddAttributeOp(string entityId, string attributeName, string attributeType = "string", bool isRequired = false)
    {
        EntityId = entityId;
        AttributeId = Guid.NewGuid().ToString("N")[..8];
        AttributeName = attributeName;
        AttributeType = attributeType;
        IsRequired = isRequired;
    }

    public void Apply(BusinessAuthoringDocument document)
    {
        var entity = document.Entities.FirstOrDefault(e => e.Id == EntityId);
        if (entity is null)
            throw new InvalidOperationException($"Entita s Id '{EntityId}' neexistuje.");

        var attr = new BusinessAttributeNode
        {
            Id = AttributeId,
            Name = AttributeName,
            Type = AttributeType,
            IsRequired = IsRequired,
        };
        entity.Attributes.Add(attr);
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        TargetAttributeId = AttributeId,
        Payload = $"{AttributeName}|{AttributeType}|{IsRequired}",
    };
}
