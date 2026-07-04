using System.ComponentModel.DataAnnotations;

namespace MetaForge.BusinessModel.Validation;

/// <summary>
/// Validační chyba s cestou ke členu a uživatelsky přívětivou zprávou.
/// </summary>
public sealed record ValidationError(string Message, IEnumerable<string>? MemberNames = null);

/// <summary>
/// Výsledek validace — obsahuje seznam chyb.
/// </summary>
public sealed record ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors { get; init; } = Array.Empty<ValidationError>();
    public IReadOnlyList<ValidationError> Warnings { get; init; } = Array.Empty<ValidationError>();

    public static ValidationResult Success() => new();
    public static ValidationResult Failure(IEnumerable<ValidationError> errors) => new() { Errors = errors.ToList().AsReadOnly() };
}

/// <summary>
/// Centrální validační pipeline — validuje vstupy pomocí Data Annotations a vlastních pravidel.
/// </summary>
public static class ValidationPipeline
{
    /// <summary>
    /// Validuje objekt pomocí Data Annotations.
    /// </summary>
    public static ValidationResult Validate<T>(T input) where T : class
    {
        var errors = new List<ValidationError>();
        var context = new ValidationContext(input);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        try
        {
            Validator.TryValidateObject(input, context, results, validateAllProperties: true);
        }
        catch
        {
            // Některé typy nemusí podporovat DataAnnotations
        }

        errors.AddRange(results.Select(r =>
            new ValidationError(r.ErrorMessage ?? "Neplatná hodnota", r.MemberNames)));

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Validuje, že řetězec není prázdný.
    /// </summary>
    public static ValidationError? ValidateNotEmpty(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new ValidationError($"{fieldName} nesmí být prázdný.", new[] { fieldName });
        return null;
    }

    /// <summary>
    /// Validuje délku řetězce.
    /// </summary>
    public static ValidationError? ValidateMaxLength(string? value, int maxLength, string fieldName)
    {
        if (value is not null && value.Length > maxLength)
            return new ValidationError($"{fieldName} přesahuje maximální délku {maxLength} znaků.", new[] { fieldName });
        return null;
    }
}
