using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Statement (příkaz) - reprezentuje kód který provádí akci.
/// </summary>
public class Statement : RootElement, ILanguageElement
{
    private string _code = string.Empty;
    private StatementType _statementType = StatementType.Expression;

    /// <summary>
    /// Kód příkazu.
    /// </summary>
    public string Code
    {
        get => _code;
        set
        {
            if (_code != value)
            {
                _code = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Typ příkazu.
    /// </summary>
    public StatementType StatementType
    {
        get => _statementType;
        set
        {
            if (_statementType != value)
            {
                _statementType = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Popis příkazu.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Vygeneruje kód příkazu.
    /// </summary>
    public string GenerateCode()
    {
        // Většina příkazů končí středníkem
        if (StatementType != StatementType.Block &&
            StatementType != StatementType.If &&
            StatementType != StatementType.For &&
            StatementType != StatementType.While &&
            StatementType != StatementType.Switch &&
            !Code.TrimEnd().EndsWith(";"))
        {
            return Code + ";";
        }

        return Code;
    }
}

/// <summary>
/// Typ příkazu.
/// </summary>
public enum StatementType
{
    /// <summary>
    /// Expression statement (např. x = 5;).
    /// </summary>
    Expression,

    /// <summary>
    /// Return statement (např. return value;).
    /// </summary>
    Return,

    /// <summary>
    /// If statement.
    /// </summary>
    If,

    /// <summary>
    /// For loop.
    /// </summary>
    For,

    /// <summary>
    /// While loop.
    /// </summary>
    While,

    /// <summary>
    /// Do-while loop.
    /// </summary>
    DoWhile,

    /// <summary>
    /// Foreach loop.
    /// </summary>
    Foreach,

    /// <summary>
    /// Switch statement.
    /// </summary>
    Switch,

    /// <summary>
    /// Try-catch-finally.
    /// </summary>
    TryCatch,

    /// <summary>
    /// Throw statement.
    /// </summary>
    Throw,

    /// <summary>
    /// Using statement.
    /// </summary>
    Using,

    /// <summary>
    /// Lock statement.
    /// </summary>
    Lock,

    /// <summary>
    /// Break statement.
    /// </summary>
    Break,

    /// <summary>
    /// Continue statement.
    /// </summary>
    Continue,

    /// <summary>
    /// Block statement ({ ... }).
    /// </summary>
    Block,

    /// <summary>
    /// Vlastní příkaz.
    /// </summary>
    Custom
}
