using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.BusinessModel.Patches.Operations;

namespace MetaForge.Translator.Translation;

/// <summary>
/// Zapisuje enrichment data zpět do business modelu přes CoreDetail.
/// Používá SetCoreDetailOp — nikdy nemutuje atribut přímo.
/// </summary>
public sealed class WriteBackService
{
    private readonly PatchEngine _patchEngine;

    public WriteBackService(PatchEngine patchEngine)
    {
        _patchEngine = patchEngine;
    }

    /// <summary>
    /// Aplikuje enrichment na atribut v dokumentu — vytvoří CoreDetail.
    /// Vrací nový dokument (immutable pattern).
    /// </summary>
    public BusinessAuthoringDocument ApplyEnrichment(BusinessAuthoringDocument document, string entityId, EnrichmentResult enrichment)
    {
        var entity = document.Entities.FirstOrDefault(e => e.Id == entityId);
        var attr = entity?.Attributes.FirstOrDefault(a => a.Id == enrichment.AttributeId);
        if (attr is null) return document;

        // Vytvoř CoreDetail z enrichment dat
        var coreDetail = new BusinessAttributeCoreDetail
        {
            Source = CoreInfoSource.Generated,
            ValueObjectName = enrichment.SuggestedCSharpType,
            LastSyncedAt = DateTimeOffset.UtcNow,
            SyncState = AttributeSyncState.Synced,
        };

        // Použij SetCoreDetailOp pro zápis
        var coreOp = new SetCoreDetailOp(entityId, enrichment.AttributeId, coreDetail);
        var newDocument = _patchEngine.Apply(document, coreOp);

        // Pokud enrichment obsahuje i změny atributu (MaxLength, DefaultValue), použij UpdateAttributeOp
        if (enrichment.MaxLength.HasValue || enrichment.DefaultValue is not null)
        {
            var attrOp = new UpdateAttributeOp(
                entityId: entityId,
                attributeId: enrichment.AttributeId,
                newName: null,
                newType: enrichment.SuggestedCSharpType,
                isRequired: null
            );
            newDocument = _patchEngine.Apply(newDocument, attrOp);
        }

        return newDocument;
    }
}
