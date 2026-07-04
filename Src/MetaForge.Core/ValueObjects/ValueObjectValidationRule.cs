namespace MetaForge.Core.ValueObjects;

/// <summary>
/// Validační pravidlo pro StrongType.
/// </summary>
public sealed record ValueObjectValidationRule(
    string RuleName,
    string? Parameter = null,
    string? ErrorMessage = null
);
