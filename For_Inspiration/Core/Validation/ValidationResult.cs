namespace MetaForge.Core.Validation;

/// <summary>
/// Závažnost validačního výsledku.
/// </summary>
public enum ValidationSeverity
{
    Success,
    Info,
    Warning,
    Error
}

/// <summary>
/// Výsledek validace.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Závažnost.
    /// </summary>
    public ValidationSeverity Severity { get; set; }

    /// <summary>
    /// Zpráva.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Kód pravidla.
    /// </summary>
    public string RuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Číslo řádku (volitelné).
    /// </summary>
    public int? Line { get; set; }

    /// <summary>
    /// Číslo sloupce (volitelné).
    /// </summary>
    public int? Column { get; set; }

    /// <summary>
    /// Je validace úspěšná?
    /// </summary>
    public bool IsValid => Severity != ValidationSeverity.Error;

    // Factory metody
    public static ValidationResult Success() => new() { Severity = ValidationSeverity.Success };
    
    public static ValidationResult Info(string message) => new() 
    { 
        Severity = ValidationSeverity.Info, 
        Message = message 
    };
    
    public static ValidationResult Warning(string message) => new() 
    { 
        Severity = ValidationSeverity.Warning, 
        Message = message 
    };
    
    public static ValidationResult Error(string message) => new() 
    { 
        Severity = ValidationSeverity.Error, 
        Message = message 
    };
}
