using MetaForge.Core.Common;

namespace MetaForge.Core.Abstractions;

/// <summary>
/// Rozhraní pro jazykový prvek.
/// </summary>
public interface ILanguageElement
{
    /// <summary>
    /// Unikátní identifikátor prvku.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Cílový programovací jazyk.
    /// </summary>
    ProgramLanguage TargetLanguage { get; set; }
}
