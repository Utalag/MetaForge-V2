// ---------------------------------------------------------------------------
// MetaForge.Core — IElementIdResolver
// Abstraction for resolving business element names to Core Guid IDs.
// Vrstva: Core / Abstractions
//
// PROPOSAL: PROP-060 / PROP-055 — enables Core to use ID mapping without referencing Translator
// ---------------------------------------------------------------------------

namespace MetaForge.Core.Abstractions;

/// <summary>
/// Abstrakce pro rozlišení jména elementu (z BusinessModel) na Core Guid.
/// Implementováno v Translator vrstvě pomocí <c>ElementIdMapping</c>.
/// Core vrstva na Translatoru nezávisí — používá toto rozhraní.
/// </summary>
public interface IElementIdResolver
{
    /// <summary>
    /// Vyhledá Core Guid podle jména elementu.
    /// Vrací null, pokud element není namapován.
    /// </summary>
    Guid? Resolve(string elementName);
}
