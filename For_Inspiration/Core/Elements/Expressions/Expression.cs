using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Expression (výraz) - reprezentuje kód který vrací hodnotu.
/// </summary>
public class Expression : RootElement, ILanguageElement
{
    private string _code = string.Empty;
    private ExpressionType _expressionType = ExpressionType.Literal;

    /// <summary>
    /// Kód výrazu.
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
    /// Typ výrazu.
    /// </summary>
    public ExpressionType ExpressionType
    {
        get => _expressionType;
        set
        {
            if (_expressionType != value)
            {
                _expressionType = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Popis výrazu.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Vygeneruje kód výrazu.
    /// </summary>
    public string GenerateCode()
    {
        return Code;
    }
}

/// <summary>
/// Typ výrazu.
/// </summary>
public enum ExpressionType
{
    /// <summary>
    /// Literál (např. 42, "hello", true).
    /// </summary>
    Literal,

    /// <summary>
    /// Binární operace (např. a + b, x * y).
    /// </summary>
    BinaryOperation,

    /// <summary>
    /// Unární operace (např. !flag, -value).
    /// </summary>
    UnaryOperation,

    /// <summary>
    /// Volání metody (např. GetValue(), obj.Method()).
    /// </summary>
    MethodCall,

    /// <summary>
    /// Přístup k property (např. obj.Property).
    /// </summary>
    PropertyAccess,

    /// <summary>
    /// Přístup k poli (např. array[0]).
    /// </summary>
    ArrayAccess,

    /// <summary>
    /// Vytvoření objektu (např. new User()).
    /// </summary>
    ObjectCreation,

    /// <summary>
    /// Lambda výraz (např. x => x * 2).
    /// </summary>
    Lambda,

    /// <summary>
    /// Ternární operátor (např. condition ? true : false).
    /// </summary>
    Conditional,

    /// <summary>
    /// Vlastní výraz.
    /// </summary>
    Custom
}
