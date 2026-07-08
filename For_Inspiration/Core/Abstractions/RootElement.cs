using System.ComponentModel;
using System.Runtime.CompilerServices;
using MetaForge.Core.Common;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Abstractions;

/// <summary>
/// Bázová třída pro všechny prvky metamodelu s reaktivní validací.
/// </summary>
public abstract class RootElement : INotifyPropertyChanged, IValidatable
{
    private ProgramLanguage _targetLanguage = ProgramLanguage.CSharp;
    private MetadataState _state = MetadataState.Draft;

    /// <summary>
    /// Unikátní identifikátor prvku.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Datum vytvoření prvku.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Datum poslední úpravy.
    /// </summary>
    public DateTime LastEdit { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Příznak, zda byl prvek uživatelsky upraven (pro merge logiku Translator).
    /// </summary>
    public bool IsCustomized { get; set; }

    /// <summary>
    /// Stav prvku.
    /// </summary>
    public MetadataState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Cílový programovací jazyk.
    /// </summary>
    public virtual ProgramLanguage TargetLanguage
    {
        get => _targetLanguage;
        set
        {
            if (_targetLanguage != value)
            {
                _targetLanguage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Kolekce výsledků validace.
    /// </summary>
    public List<ValidationResult> ValidationResults { get; } = new();

    /// <summary>
    /// Přidá chybu do validačních výsledků.
    /// </summary>
    public void AddError(string message, string ruleCode)
    {
        ValidationResults.Add(new ValidationResult
        {
            Severity = ValidationSeverity.Error,
            Message = message,
            RuleCode = ruleCode
        });

        State = MetadataState.Invalid;
    }

    /// <summary>
    /// Přidá varování do validačních výsledků.
    /// </summary>
    public void AddWarning(string message, string ruleCode)
    {
        ValidationResults.Add(new ValidationResult
        {
            Severity = ValidationSeverity.Warning,
            Message = message,
            RuleCode = ruleCode
        });
    }

    /// <summary>
    /// Přidá informaci do validačních výsledků.
    /// </summary>
    public void AddInfo(string message, string ruleCode)
    {
        ValidationResults.Add(new ValidationResult
        {
            Severity = ValidationSeverity.Info,
            Message = message,
            RuleCode = ruleCode
        });
    }

    /// <summary>
    /// Vymaže všechny validační výsledky.
    /// </summary>
    public void ClearValidationResults()
    {
        ValidationResults.Clear();
    }

    /// <summary>
    /// Zkontroluje, zda prvek obsahuje chyby.
    /// </summary>
    public bool HasErrors()
    {
        return ValidationResults.Any(r => r.Severity == ValidationSeverity.Error);
    }

    /// <summary>
    /// Validuje invarianty prvku. Nastaví State na Valid nebo Invalid.
    /// Potomci přepisují tuto metodu pro konkrétní pravidla.
    /// Kaskádující prvky (Class) kontrolují State dětí před vlastní validací.
    /// </summary>
    public virtual ValidationSummary Validate()
    {
        ClearValidationResults();
        return FinalizeValidation(GetType().Name);
    }

    /// <summary>
    /// Povýší State z Valid na Ready po úspěšné Roslyn validaci vygenerovaných testů.
    /// </summary>
    public void MarkReady()
    {
        if (State == MetadataState.Valid)
            State = MetadataState.Ready;
    }

    /// <summary>
    /// Uzavře validaci: nastaví State podle aktuálních ValidationResults a vrátí souhrn.
    /// Volat vždy jako poslední řádek v override Validate().
    /// </summary>
    protected ValidationSummary FinalizeValidation(string elementName)
    {
        var hasErrors = ValidationResults.Any(r => r.Severity == ValidationSeverity.Error);
        State = hasErrors ? MetadataState.Invalid : MetadataState.Valid;
        return new ValidationSummary(elementName, State, [..ValidationResults]);
    }





    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        LastEdit = DateTime.UtcNow;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
