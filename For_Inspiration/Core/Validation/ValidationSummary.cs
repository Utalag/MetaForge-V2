using MetaForge.Core.Common;

namespace MetaForge.Core.Validation;

/// <summary>
/// Výsledek volání Validate() na prvku metamodelu.
/// Obsahuje stav po validaci a seznam všech výsledků.
/// </summary>
public sealed class ValidationSummary
{
    /// <summary>
    /// Název validovaného prvku (pro diagnostiku v kaskádě).
    /// </summary>
    public string ElementName { get; }

    /// <summary>
    /// Stav prvku po validaci.
    /// </summary>
    public MetadataState ResultingState { get; }

    /// <summary>
    /// True pokud validace neobsahuje žádné chyby.
    /// </summary>
    public bool IsValid => ResultingState != MetadataState.Invalid;

    /// <summary>
    /// Všechny výsledky validace.
    /// </summary>
    public IReadOnlyList<ValidationResult> Results { get; }

    internal ValidationSummary(string elementName, MetadataState state, IReadOnlyList<ValidationResult> results)
    {
        ElementName = elementName;
        ResultingState = state;
        Results = results;
    }

    /// <summary>
    /// Vrátí pouze chybové zprávy.
    /// </summary>
    public IEnumerable<string> Errors =>
        Results
            .Where(r => r.Severity == ValidationSeverity.Error)
            .Select(r => r.Message);

    public override string ToString() =>
        IsValid
            ? $"{ElementName}: {ResultingState}"
            : $"{ElementName}: [{string.Join(", ", Errors)}]";
}
