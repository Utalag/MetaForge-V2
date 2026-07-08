using MetaForge.Core.Common;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Abstractions;

/// <summary>
/// Kontrakt pro prvky metamodelu schopné validace.
/// Validace probíhá ve dvou krocích:
///   1. Validate()   → State: Valid | Invalid
///   2. MarkReady()  → State: Ready  (volá test pipeline po úspěšné Roslyn validaci)
/// Nadřazené elementy kontrolují State == Ready jako gate před vlastní validací.
/// </summary>
public interface IValidatable
{
    /// <summary>
    /// Aktuální stav prvku.
    /// </summary>
    MetadataState State { get; }

    /// <summary>
    /// Spustí validaci invariantů prvku a vrátí souhrn výsledků.
    /// Nastaví State na Valid nebo Invalid.
    /// </summary>
    ValidationSummary Validate();

    /// <summary>
    /// Povýší State z Valid na Ready.
    /// Volá test pipeline po úspěšném vygenerování a Roslyn-validaci testů.
    /// </summary>
    void MarkReady();
}
