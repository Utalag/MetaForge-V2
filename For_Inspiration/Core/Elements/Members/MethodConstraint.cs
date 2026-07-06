using MetaForge.Core.Abstractions;
using MetaForge.Core.Validation;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Zdroj constraintu - určuje jak byl constraint odvozen.
/// </summary>
public enum ConstraintSource
{
    /// <summary>Manuálně přidaný.</summary>
    Manual,
    
    /// <summary>Odvozený z parametru metody.</summary>
    FromParameter,
    
    /// <summary>Odvozený z Property/Field typu.</summary>
    FromMemberType,
    
    /// <summary>Odvozený z kombinace členů (AI).</summary>
    FromCombination,
    
    /// <summary>Odvozený AI boundary analyzerem.</summary>
    FromAIBoundaryAnalysis,
    
    /// <summary>Odvozený z Roslyn testů.</summary>
    FromRoslynTest,
    
    /// <summary>Odvozený z RuleBasedConstraintInferencer.</summary>
    FromRuleBased
}

/// <summary>
/// Kontraktní omezení metody (precondition/invariant/postcondition).
/// Generuje guard clause nebo assertion do těla metody.
/// </summary>
public class MethodConstraint : RootElement
{
    private string _invalidCondition = string.Empty;
    private string _description = string.Empty;
    private ConstraintKind _kind = ConstraintKind.Precondition;
    private string _exceptionType = "ArgumentException";
    private string _exceptionMessage = string.Empty;
    private bool _generateCustomException;
    private ConstraintSource _source = ConstraintSource.Manual;
    private bool _isVerified;

    /// <summary>
    /// Podmínka, která je neplatná (pokud je true, constraint selže).
    /// Příklad: "value &lt; 0", "string.IsNullOrEmpty(name)"
    /// </summary>
    public string InvalidCondition
    {
        get => _invalidCondition;
        set
        {
            if (_invalidCondition != value)
            {
                _invalidCondition = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Popis constraintu (pro dokumentaci / chybové zprávy).
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Druh kontraktu (Precondition, Invariant, Postcondition).
    /// </summary>
    public ConstraintKind Kind
    {
        get => _kind;
        set
        {
            if (_kind != value)
            {
                _kind = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Typ výjimky, která se vyhodí při porušení constraintu.
    /// Default: "ArgumentException".
    /// </summary>
    public string ExceptionType
    {
        get => _exceptionType;
        set
        {
            if (_exceptionType != value)
            {
                _exceptionType = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Zpráva výjimky.
    /// </summary>
    public string ExceptionMessage
    {
        get => _exceptionMessage;
        set
        {
            if (_exceptionMessage != value)
            {
                _exceptionMessage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Pokud true, generuje vlastní typ výjimky místo vestavěné.
    /// </summary>
    public bool GenerateCustomException
    {
        get => _generateCustomException;
        set
        {
            if (_generateCustomException != value)
            {
                _generateCustomException = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Zdroj constraintu - jak byl odvozen.
    /// </summary>
    public ConstraintSource Source
    {
        get => _source;
        set
        {
            if (_source != value)
            {
                _source = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Zda byl constraint ověřen (např. Roslyn testy).
    /// </summary>
    public bool IsVerified
    {
        get => _isVerified;
        set
        {
            if (_isVerified != value)
            {
                _isVerified = value;
                OnPropertyChanged();
            }
        }
    }

    public override ValidationSummary Validate()
    {
        ClearValidationResults();

        if (string.IsNullOrWhiteSpace(InvalidCondition))
            AddError("InvalidCondition cannot be empty.", "CONSTRAINT_001");

        if (string.IsNullOrWhiteSpace(ExceptionType))
            AddError("ExceptionType cannot be empty.", "CONSTRAINT_002");

        return FinalizeValidation(nameof(MethodConstraint));
    }
}
