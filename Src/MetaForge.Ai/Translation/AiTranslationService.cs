using MetaForge.Ai.Abstractions;
using MetaForge.BusinessModel.Models;
using MetaForge.Translator.Host;
using MetaForge.Translator.Translation;

namespace MetaForge.Ai.Translation;

/// <summary>
/// AI implementace enrichmentu — používá AI pro hlubší analýzu atributů.
/// Při selhání vrací null (graceful fallback na deterministický DefaultBusinessTranslator).
/// </summary>
public sealed class AiTranslationService : ITranslationService
{
    private readonly IAiBackendAdapter _backend;

    public AiTranslationService(IAiBackendAdapter backend)
    {
        _backend = backend;
    }

    /// <summary>
    /// Pokusí se o AI enrichment atributu s kontextem projekce.
    /// Vrací null pokud AI selže nebo není k dispozici.
    /// </summary>
    public async Task<EnrichmentResult?> EnrichAsync(
        BusinessAttributeNode attribute,
        ProjectionView context,
        CancellationToken ct = default)
    {
        try
        {
            var available = await _backend.IsAvailableAsync(ct);
            if (!available) return null;

            // Postav prompt s kontextem
            var entityContext = context.Entities
                .FirstOrDefault(e => e.Attributes.Any(a => a.Id == attribute.Id));

            var entityAttrs = entityContext is not null
                ? string.Join(", ", entityContext.Attributes.Select(a => a.Name))
                : "neznámý kontext";

            var prompt = $$"""
                Analyzuj atribut '{{attribute.Name}}' typu '{{attribute.Type}}' v kontextu entity.
                
                Kontext:
                - Entita obsahuje atributy: {{entityAttrs}}
                
                Vrať POUZE JSON:
                {
                    "suggested_csharp_type": "string",
                    "validation_rules": ["not_empty"],
                    "max_length": 200,
                    "default_value": null
                }
                """;

            var result = await _backend.SendJsonAsync<AiEnrichmentResponse>(prompt, ct);
            if (result is null) return null;

            return new EnrichmentResult(
                AttributeId: attribute.Id,
                SuggestedCSharpType: result.SuggestedCSharpType,
                ValidationRules: result.ValidationRules,
                MaxLength: result.MaxLength,
                DefaultValue: result.DefaultValue
            );
        }
        catch
        {
            return null; // Graceful fallback
        }
    }

    private class AiEnrichmentResponse
    {
        public string? SuggestedCSharpType { get; set; }
        public List<string>? ValidationRules { get; set; }
        public int? MaxLength { get; set; }
        public string? DefaultValue { get; set; }
    }
}
