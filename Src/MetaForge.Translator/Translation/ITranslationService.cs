using MetaForge.BusinessModel.Models;
using MetaForge.Translator.Host;

namespace MetaForge.Translator.Translation;

/// <summary>
/// AI-assisted enrichment business atributů.
/// Implementace je v MetaForge.Ai — volitelná, při selhání vrací null.
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// Pokusí se o AI enrichment atributu s kontextem projekce.
    /// Vrací null pokud AI selže nebo není k dispozici (graceful fallback).
    /// </summary>
    Task<EnrichmentResult?> EnrichAsync(
        BusinessAttributeNode attribute,
        ProjectionView context,
        CancellationToken ct = default);
}
