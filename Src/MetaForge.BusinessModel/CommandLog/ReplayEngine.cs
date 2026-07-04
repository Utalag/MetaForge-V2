using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Autoritativní rekonstrukce stavu — přehraje commandy a vytvoří BusinessAuthoringDocument.
/// Používá immutable pattern — každý krok vrací nový dokument.
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
            document = ApplyCommand(document, command);
        }

        return document;
    }

    /// <summary>
    /// Inkrementální replay — přehraje commandy od startIndex na existující dokument.
    /// Vrací nový dokument — původní zůstává nezměněn.
    /// </summary>
    public BusinessAuthoringDocument ReplayFrom(BusinessAuthoringDocument document, IReadOnlyList<CommandEnvelope> commands, int startIndex)
    {
        var current = document;

        for (int i = startIndex; i < commands.Count; i++)
        {
            current = ApplyCommand(current, commands[i]);
        }

        return current;
    }

    /// <summary>Aplikuje jeden command na dokument a vrátí nový dokument.</summary>
    private static BusinessAuthoringDocument ApplyCommand(BusinessAuthoringDocument document, CommandEnvelope command)
    {
        var result = command.CommandType switch
        {
            "AddEntity" => ApplyAddEntity(document, command),
            "UpdateEntity" => ApplyUpdateEntity(document, command),
            "DeleteEntity" => ApplyDeleteEntity(document, command),
            "AddAttribute" => ApplyAddAttribute(document, command),
            "UpdateAttribute" => ApplyUpdateAttribute(document, command),
            "DeleteAttribute" => ApplyDeleteAttribute(document, command),
            "AddRelation" => ApplyAddRelation(document, command),
            "SetCoreDetail" => ApplySetCoreDetail(document, command),
            "UpdateSyncState" => ApplyUpdateSyncState(document, command),
            _ => document, // Neznámý command typ = přeskočit (pro budoucí kompatibilitu)
        };

        return result with { LastModified = command.Timestamp };
    }

    private static BusinessAuthoringDocument ApplyAddEntity(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = new BusinessEntityNode
        {
            Id = cmd.TargetEntityId ?? Guid.NewGuid().ToString("N")[..8],
            Name = cmd.Payload,
        };

        return doc with
        {
            Entities = doc.Entities.Append(entity).ToList().AsReadOnly(),
        };
    }

    private static BusinessAuthoringDocument ApplyUpdateEntity(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        return doc with
        {
            Entities = doc.Entities
                .Select(e => e.Id == cmd.TargetEntityId ? e with { Name = cmd.Payload } : e)
                .ToList()
                .AsReadOnly(),
        };
    }

    private static BusinessAuthoringDocument ApplyDeleteEntity(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        return doc with
        {
            Entities = doc.Entities
                .Where(e => e.Id != cmd.TargetEntityId)
                .ToList()
                .AsReadOnly(),
            Relations = doc.Relations
                .Where(r => r.FromEntityId != cmd.TargetEntityId && r.ToEntityId != cmd.TargetEntityId)
                .ToList()
                .AsReadOnly(),
        };
    }

    private static BusinessAuthoringDocument ApplyAddAttribute(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var entity = doc.Entities.FirstOrDefault(e => e.Id == cmd.TargetEntityId);
        if (entity is null) return doc;

        var parts = cmd.Payload.Split('|');
        var attr = new BusinessAttributeNode
        {
            Id = cmd.TargetAttributeId ?? Guid.NewGuid().ToString("N")[..8],
            Name = parts.Length > 0 ? parts[0] : "Unnamed",
            Type = parts.Length > 1 ? parts[1] : "string",
            IsRequired = parts.Length > 2 && bool.TryParse(parts[2], out var req) && req,
        };

        return doc with
        {
            Entities = doc.Entities
                .Select(e => e.Id == cmd.TargetEntityId
                    ? e with { Attributes = e.Attributes.Append(attr).ToList().AsReadOnly() }
                    : e)
                .ToList()
                .AsReadOnly(),
        };
    }

    private static BusinessAuthoringDocument ApplyUpdateAttribute(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var parts = cmd.Payload.Split('|');

        return doc with
        {
            Entities = doc.Entities
                .Select(e => e.Id == cmd.TargetEntityId
                    ? e with
                    {
                        Attributes = e.Attributes
                            .Select(a => a.Id == cmd.TargetAttributeId
                                ? a with
                                {
                                    Name = parts.Length > 0 && !string.IsNullOrEmpty(parts[0]) ? parts[0] : a.Name,
                                    Type = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? parts[1] : a.Type,
                                    IsRequired = parts.Length > 2 && bool.TryParse(parts[2], out var req) ? req : a.IsRequired,
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

    private static BusinessAuthoringDocument ApplyDeleteAttribute(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        return doc with
        {
            Entities = doc.Entities
                .Select(e => e.Id == cmd.TargetEntityId
                    ? e with
                    {
                        Attributes = e.Attributes
                            .Where(a => a.Id != cmd.TargetAttributeId)
                            .ToList()
                            .AsReadOnly(),
                    }
                    : e)
                .ToList()
                .AsReadOnly(),
        };
    }

    private static BusinessAuthoringDocument ApplyAddRelation(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        var parts = cmd.Payload.Split('|');
        var relation = new BusinessRelationNode
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            FromEntityId = parts.Length > 0 ? parts[0] : string.Empty,
            ToEntityId = parts.Length > 1 ? parts[1] : string.Empty,
            RelationType = parts.Length > 2 ? parts[2] : "OneToMany",
        };

        return doc with
        {
            Relations = doc.Relations.Append(relation).ToList().AsReadOnly(),
        };
    }

    /// <summary>Aplikuje SetCoreDetail command — nastaví CoreDetail na atributu.</summary>
    private static BusinessAuthoringDocument ApplySetCoreDetail(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        // Payload formát: "Source|ResolvedPresetId|ValueObjectName|IsStrongType|LastSyncedAt"
        var parts = cmd.Payload.Split('|');
        var coreDetail = new BusinessAttributeCoreDetail
        {
            Source = parts.Length > 0 && Enum.TryParse<CoreInfoSource>(parts[0], out var src) ? src : CoreInfoSource.Unknown,
            ResolvedPresetId = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? parts[1] : null,
            ValueObjectName = parts.Length > 2 && !string.IsNullOrEmpty(parts[2]) ? parts[2] : null,
            IsStrongType = parts.Length > 3 && bool.TryParse(parts[3], out var ist) && ist,
            LastSyncedAt = parts.Length > 4 && DateTimeOffset.TryParse(parts[4], out var dt) ? dt : null,
            SyncState = AttributeSyncState.Synced,
        };

        return doc with
        {
            Entities = doc.Entities
                .Select(e => e.Id == cmd.TargetEntityId
                    ? e with
                    {
                        Attributes = e.Attributes
                            .Select(a => a.Id == cmd.TargetAttributeId
                                ? a with { CoreDetail = coreDetail }
                                : a)
                            .ToList()
                            .AsReadOnly(),
                    }
                    : e)
                .ToList()
                .AsReadOnly(),
        };
    }

    /// <summary>Aplikuje UpdateSyncState command — změní SyncState na atributu.</summary>
    private static BusinessAuthoringDocument ApplyUpdateSyncState(BusinessAuthoringDocument doc, CommandEnvelope cmd)
    {
        if (!Enum.TryParse<AttributeSyncState>(cmd.Payload, out var newState))
            return doc;

        return doc with
        {
            Entities = doc.Entities
                .Select(e => e.Id == cmd.TargetEntityId
                    ? e with
                    {
                        Attributes = e.Attributes
                            .Select(a => a.Id == cmd.TargetAttributeId && a.CoreDetail is not null
                                ? a with { CoreDetail = a.CoreDetail with { SyncState = newState } }
                                : a)
                            .ToList()
                            .AsReadOnly(),
                    }
                    : e)
                .ToList()
                .AsReadOnly(),
        };
    }
}
