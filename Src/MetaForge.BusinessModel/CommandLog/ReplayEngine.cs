using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Autoritativní rekonstrukce stavu — přehraje commandy a vytvoří BusinessAuthoringDocument.
/// </summary>
public sealed class ReplayEngine
{
    /// <summary>
    /// Přehraje všechny commandy a vytvoří aktuální stav dokumentu.
    /// Toto je autoritativní způsob, jak získat stav — ne cache, ne databáze.
    /// </summary>
    public BusinessAuthoringDocument Replay(IReadOnlyList<CommandEnvelope> commands)
    {
        var document = new BusinessAuthoringDocument();

        foreach (var command in commands)
        {
            ApplyCommand(document, command);
        }

        return document;
    }

    /// <summary>
    /// Inkrementální replay — přehraje commandy od startIndex na existující dokument.
    /// </summary>
    public void ReplayFrom(BusinessAuthoringDocument document, IReadOnlyList<CommandEnvelope> commands, int startIndex)
    {
        for (int i = startIndex; i < commands.Count; i++)
        {
            ApplyCommand(document, commands[i]);
        }
    }

    /// <summary>Aplikuje jeden command na dokument.</summary>
    private static void ApplyCommand(BusinessAuthoringDocument document, CommandEnvelope command)
    {
        switch (command.CommandType)
        {
            case "AddEntity":
                ApplyAddEntity(document, command);
                break;
            case "UpdateEntity":
                ApplyUpdateEntity(document, command);
                break;
            case "DeleteEntity":
                ApplyDeleteEntity(document, command);
                break;
            case "AddAttribute":
                ApplyAddAttribute(document, command);
                break;
            case "UpdateAttribute":
                ApplyUpdateAttribute(document, command);
                break;
            case "DeleteAttribute":
                ApplyDeleteAttribute(document, command);
                break;
            case "AddRelation":
                ApplyAddRelation(document, command);
                break;
            // Neznámý command typ = přeskočit (pro budoucí kompatibilitu)
        }

        document.LastModified = command.Timestamp;
    }

    private static void ApplyAddEntity(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = new BusinessEntityNode
        {
            Id = cmd.TargetEntityId ?? Guid.NewGuid().ToString("N")[..8],
            Name = cmd.Payload, // Payload je název entity
        };
        doc.Entities.Add(entity);
    }

    private static void ApplyUpdateEntity(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = doc.Entities.FirstOrDefault(e => e.Id == cmd.TargetEntityId);
        if (entity is not null)
            entity.Name = cmd.Payload;
    }

    private static void ApplyDeleteEntity(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        doc.Entities.RemoveAll(e => e.Id == cmd.TargetEntityId);
        doc.Relations.RemoveAll(r => r.FromEntityId == cmd.TargetEntityId || r.ToEntityId == cmd.TargetEntityId);
    }

    private static void ApplyAddAttribute(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = doc.Entities.FirstOrDefault(e => e.Id == cmd.TargetEntityId);
        if (entity is null) return;

        // Payload formát: "NázevAtributu|Typ|IsRequired"
        var parts = cmd.Payload.Split('|');
        var attr = new BusinessAttributeNode
        {
            Id = cmd.TargetAttributeId ?? Guid.NewGuid().ToString("N")[..8],
            Name = parts.Length > 0 ? parts[0] : "Unnamed",
            Type = parts.Length > 1 ? parts[1] : "string",
            IsRequired = parts.Length > 2 && bool.TryParse(parts[2], out var req) && req,
        };
        entity.Attributes.Add(attr);
    }

    private static void ApplyUpdateAttribute(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = doc.Entities.FirstOrDefault(e => e.Id == cmd.TargetEntityId);
        var attr = entity?.Attributes.FirstOrDefault(a => a.Id == cmd.TargetAttributeId);
        if (attr is null) return;

        var parts = cmd.Payload.Split('|');
        if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0])) attr.Name = parts[0];
        if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1])) attr.Type = parts[1];
        if (parts.Length > 2 && bool.TryParse(parts[2], out var req)) attr.IsRequired = req;
    }

    private static void ApplyDeleteAttribute(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = doc.Entities.FirstOrDefault(e => e.Id == cmd.TargetEntityId);
        entity?.Attributes.RemoveAll(a => a.Id == cmd.TargetAttributeId);
    }

    private static void ApplyAddRelation(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var parts = cmd.Payload.Split('|');
        var relation = new BusinessRelationNode
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            FromEntityId = parts.Length > 0 ? parts[0] : string.Empty,
            ToEntityId = parts.Length > 1 ? parts[1] : string.Empty,
            RelationType = parts.Length > 2 ? parts[2] : "OneToMany",
        };
        doc.Relations.Add(relation);
    }
}
