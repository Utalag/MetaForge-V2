using MetaForge.Core.Abstractions;

namespace MetaForge.Core.ValueObjects;

/// <summary>
/// Typ validačního pravidla pro Value Object.
/// </summary>
public enum ValidationRuleType
{
    /// <summary>Hodnota nesmí být prázdná/null/default.</summary>
    NotEmpty,

    /// <summary>Hodnota musí být kladná.</summary>
    Positive,

    /// <summary>Hodnota musí být nezáporná.</summary>
    NonNegative,

    /// <summary>Hodnota musí odpovídat regulárnímu výrazu.</summary>
    Regex,

    /// <summary>Minimální hodnota/délka.</summary>
    Min,

    /// <summary>Maximální hodnota/délka.</summary>
    Max,

    /// <summary>Rozsah hodnot.</summary>
    Range,

    /// <summary>Musí začínat prefixem.</summary>
    StartsWith,

    /// <summary>Přesná délka.</summary>
    ExactLength,

    /// <summary>Vlastní validační logika (C# kód).</summary>
    Custom,

    /// <summary>Konkrétní zakázaná hodnota (Parameter = zakázaná hodnota).</summary>
    ForbiddenValue,

    /// <summary>Množina zakázaných hodnot (Parameter = čárkou oddělené hodnoty).</summary>
    ForbiddenValues,

    /// <summary>Množina povolených hodnot (Parameter = čárkou oddělené hodnoty).</summary>
    AllowedValues,

    /// <summary>Hodnota musí být přesně rovna (Parameter = očekávaná hodnota).</summary>
    ExactValue,

    /// <summary>Luhn check (validace čísla karty, IBAN apod.).</summary>
    LuhnCheck,

    /// <summary>Vlastní validační výraz (Parameter = C# expression returning bool).</summary>
    CustomExpression
}

/// <summary>
/// Validační pravidlo pro Value Object.
/// Generuje část Vogen Validate() metody.
/// </summary>
public class ValueObjectValidationRule : RootElement
{
    private ValidationRuleType _ruleType;
    private string _errorMessage = string.Empty;
    private string? _parameter;

    /// <summary>Typ pravidla.</summary>
    public ValidationRuleType RuleType
    {
        get => _ruleType;
        set
        {
            if (_ruleType != value)
            {
                _ruleType = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Chybová zpráva při neúspěšné validaci.</summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Parametr pravidla (regex pattern, min/max hodnota, prefix, custom kód).
    /// </summary>
    public string? Parameter
    {
        get => _parameter;
        set
        {
            if (_parameter != value)
            {
                _parameter = value;
                OnPropertyChanged();
            }
        }
    }
}
