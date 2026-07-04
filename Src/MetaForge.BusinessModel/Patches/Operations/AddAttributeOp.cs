using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Přidá atribut k entitě (immutable).
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

    public BusinessAuthoringDocument Apply(BusinessAuthoringDocument document)
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

        return document with
        {
            Entities = document.Entities
                .Select(e => e.Id == EntityId
                    ? e with { Attributes = e.Attributes.Append(attr).ToList().AsReadOnly() }
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
        Payload = $"{AttributeName}|{AttributeType}|{IsRequired}",
    };
}
