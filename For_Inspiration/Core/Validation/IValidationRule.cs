namespace MetaForge.Core.Validation;

/// <summary>
/// Rozhraní pro validační pravidlo.
/// </summary>
public interface IValidationRule
{
    /// <summary>
    /// Kód pravidla.
    /// </summary>
    string RuleCode { get; }

    /// <summary>
    /// Popis pravidla.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Validuje hodnotu asynchronně.
    /// </summary>
    Task<List<ValidationResult>> ValidateAsync(object? value);
}
