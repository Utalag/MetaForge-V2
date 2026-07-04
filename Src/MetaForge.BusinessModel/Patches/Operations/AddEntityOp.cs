using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Patches.Operations;

/// <summary>
/// Přidá novou entitu do dokumentu.
/// </summary>
public sealed class AddEntityOp : IPatchOperation
{
    public string CommandType => "AddEntity";

    public string EntityId { get; }
    public string EntityName { get; }

    public AddEntityOp(string entityName)
    {
        EntityId = Guid.NewGuid().ToString("N")[..8];
        EntityName = entityName;
    }

    public void Apply(BusinessAuthoringDocument document)
    {
        var entity = new BusinessEntityNode
        {
            Id = EntityId,
            Name = EntityName,
        };
        document.Entities.Add(entity);
    }

    public CommandEnvelope ToEnvelope() => new()
    {
        CommandType = CommandType,
        TargetEntityId = EntityId,
        Payload = EntityName,
    };
}
