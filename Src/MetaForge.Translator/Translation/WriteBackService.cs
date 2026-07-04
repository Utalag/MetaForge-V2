using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.BusinessModel.Patches.Operations;

namespace MetaForge.Translator.Translation;

/// <summary>
/// Zapisuje enrichment data zpět do business modelu.
/// Např. AI zjistí, že "Email" atribut by měl mít MaxLength=254 → zapíše do atributu.
/// </summary>
public sealed class WriteBackService
{
    private readonly PatchEngine _patchEngine;

    public WriteBackService(PatchEngine patchEngine)
    {
        _patchEngine = patchEngine;
    }

    /// <summary>
    /// Aplikuje enrichment na atribut v dokumentu.
    /// </summary>
    public void ApplyEnrichment(BusinessAuthoringDocument document, string entityId, EnrichmentResult enrichment)
    {
        var entity = document.Entities.FirstOrDefault(e => e.Id == entityId);
        var attr = entity?.Attributes.FirstOrDefault(a => a.Id == enrichment.AttributeId);
        if (attr is null) return;

        // Aplikuj enrichment data
        if (enrichment.MaxLength.HasValue)
            attr.MaxLength = enrichment.MaxLength;

        if (enrichment.DefaultValue is not null)
            attr.DefaultValue = enrichment.DefaultValue;
    }

    /// <summary>
    /// Aplikuje enrichment a zaznamená do CommandLog přes PatchEngine.
    /// </summary>
    public void ApplyEnrichmentWithLog(BusinessAuthoringDocument document, string entityId, EnrichmentResult enrichment)
    {
        var entity = document.Entities.FirstOrDefault(e => e.Id == entityId);
        if (entity is null) return;

        var attr = entity.Attributes.FirstOrDefault(a => a.Id == enrichment.AttributeId);
        if (attr is null) return;

        // Vytvoř update operaci
        var op = new UpdateAttributeOp(
            entityId: entityId,
            attributeId: enrichment.AttributeId,
            newName: null, // neměníme název
            newType: enrichment.SuggestedCSharpType,
            isRequired: null
        );

        _patchEngine.Apply(document, op);

        // Aplikuj enrichment dodatečně
        ApplyEnrichment(document, entityId, enrichment);
    }
}
