using System.Collections.ObjectModel;
using System.Text;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;

namespace MetaForge.Core.DataTypes;

/// <summary>
/// Model datového typu s reaktivním přepočtem syntaxe.
/// </summary>
public class TypeModel : RootElement, ILanguageElement
{
    private DataType _baseType = DataType.String;
    private string _customTypeName = string.Empty;
    private bool _isNullable;
    private bool _isCollection;
    private SemanticCollection _semanticCollection = SemanticCollection.None;
    private DataType _keyType = DataType.String;
    private string _keyCustomTypeName = string.Empty;
    private EntityKind _entityKind = EntityKind.Primitive;
    private string _currentSyntax = string.Empty;

    /// <summary>
    /// Konstruktor - inicializuje syntaxi.
    /// </summary>
    public TypeModel()
    {
        RecalculateSyntax();
    }

    /// <summary>
    /// Základní datový typ.
    /// </summary>
    public DataType BaseType
    {
        get => _baseType;
        set
        {
            if (_baseType != value)
            {
                _baseType = value;
                OnPropertyChanged();
                RecalculateSyntax();
            }
        }
    }

    /// <summary>
    /// Název vlastního typu (pokud BaseType == Custom).
    /// </summary>
    public string CustomTypeName
    {
        get => _customTypeName;
        set
        {
            if (_customTypeName != value)
            {
                _customTypeName = value;
                OnPropertyChanged();
                RecalculateSyntax();
            }
        }
    }

    /// <summary>
    /// Je typ nullable?
    /// </summary>
    public bool IsNullable
    {
        get => _isNullable;
        set
        {
            if (_isNullable != value)
            {
                _isNullable = value;
                OnPropertyChanged();
                RecalculateSyntax();
            }
        }
    }

    /// <summary>
    /// Je typ kolekce?
    /// </summary>
    public bool IsCollection
    {
        get => _isCollection;
        set
        {
            if (_isCollection != value)
            {
                _isCollection = value;
                OnPropertyChanged();
                RecalculateSyntax();
            }
        }
    }

    /// <summary>
    /// Sémantická kolekce (List, Set, Map, atd.).
    /// </summary>
    public SemanticCollection SemanticCollection
    {
        get => _semanticCollection;
        set
        {
            if (_semanticCollection != value)
            {
                _semanticCollection = value;
                OnPropertyChanged();
                RecalculateSyntax();
            }
        }
    }

    /// <summary>
    /// Typ klíče pro Dictionary/Map kolekce.
    /// </summary>
    public DataType KeyType
    {
        get => _keyType;
        set
        {
            if (_keyType != value)
            {
                _keyType = value;
                OnPropertyChanged();
                RecalculateSyntax();
            }
        }
    }

    /// <summary>
    /// Název vlastního typu klíče (pokud KeyType == Custom).
    /// </summary>
    public string KeyCustomTypeName
    {
        get => _keyCustomTypeName;
        set
        {
            if (_keyCustomTypeName != value)
            {
                _keyCustomTypeName = value;
                OnPropertyChanged();
                RecalculateSyntax();
            }
        }
    }

    /// <summary>
    /// Druh entity (pro vlastní typy).
    /// </summary>
    public EntityKind EntityKind
    {
        get => _entityKind;
        set
        {
            if (_entityKind != value)
            {
                _entityKind = value;
                OnPropertyChanged();
                RecalculateSyntax();
            }
        }
    }

    /// <summary>
    /// Aktuální syntaxe pro cílový jazyk.
    /// </summary>
    public string CurrentSyntax
    {
        get => _currentSyntax;
        private set
        {
            if (_currentSyntax != value)
            {
                _currentSyntax = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Přepočítá syntaxi podle cílového jazyka.
    /// </summary>
    public void RecalculateSyntax()
    {
        CurrentSyntax = GetSyntax(TargetLanguage);
    }

    /// <summary>
    /// Získá syntaxi pro daný jazyk.
    /// </summary>
    public string GetSyntax(ProgramLanguage language)
    {
        // Pro nyní jednoduché mapování, později použijeme IDataTypeSyntaxRule
         
        var baseTypeSyntax = GetBaseTypeSyntax(language);

        if (IsCollection)
        {
            return GetCollectionSyntax(language, baseTypeSyntax);
        }

        if (IsNullable)
        {
            return GetNullableSyntax(language, baseTypeSyntax);
        }

        return baseTypeSyntax;
    }

    /// <summary>
    /// Vrátí název ValueObject třídy. Musí být nastaven CustomTypeName.
    /// </summary>
    private string GetValueObjectName() =>
        GetResolvedTypeName(CustomTypeName, "UnknownValueObject");

    private string GetCustomTypeName() =>
        GetResolvedTypeName(CustomTypeName, "UnknownType");

    private string GetCustomKeyTypeName() =>
        GetResolvedTypeName(KeyCustomTypeName, "UnknownType");

    private static string GetResolvedTypeName(string? typeName, string fallbackName) =>
        string.IsNullOrWhiteSpace(typeName) ? fallbackName : typeName;

    private string GetBaseTypeSyntax(ProgramLanguage language)
    {
        // ValueObject mode: vrátí název VO třídy (jazykově-agnostický)
        if (EntityKind == EntityKind.ValueObject)
            return GetValueObjectName();

        if (BaseType == DataType.Custom)
        {
            return GetCustomTypeName();
        }

        // Jednoduché mapování pro základní typy
        return GetPrimitiveTypeSyntax(language, BaseType);
    }

    private static string GetPrimitiveTypeSyntax(ProgramLanguage language, DataType dataType)
    {
        return language switch
        {
            ProgramLanguage.CSharp => dataType switch
            {
                DataType.Int => "int",
                DataType.String => "string",
                DataType.Boolean => "bool",
                DataType.Double => "double",
                DataType.Decimal => "decimal",
                DataType.Void => "void",
                DataType.Object => "object",
                DataType.Guid => "Guid",
                DataType.DateTime => "DateTime",
                DataType.Date => "DateOnly",
                DataType.Time => "TimeOnly",
                DataType.Byte => "byte",
                DataType.Short => "short",
                DataType.Long => "long",
                DataType.Float => "float",
                DataType.Char => "char",
                _ => dataType.ToString().ToLower()
            },
            ProgramLanguage.TypeScript => dataType switch
            {
                DataType.Int => "number",
                DataType.Double => "number",
                DataType.Float => "number",
                DataType.Long => "number",
                DataType.Decimal => "number",
                DataType.String => "string",
                DataType.Boolean => "boolean",
                DataType.Void => "void",
                DataType.Object => "any",
                DataType.Guid => "string",
                DataType.DateTime => "Date",
                DataType.Date => "Date",
                DataType.Time => "Date",
                _ => "any"
            },
            ProgramLanguage.Python => dataType switch
            {
                DataType.Int => "int",
                DataType.Long => "int",
                DataType.String => "str",
                DataType.Boolean => "bool",
                DataType.Double => "float",
                DataType.Float => "float",
                DataType.Decimal => "Decimal",
                DataType.Void => "None",
                DataType.Object => "object",
                DataType.Guid => "uuid.UUID",
                DataType.DateTime => "datetime",
                DataType.Date => "date",
                DataType.Time => "time",
                _ => "Any"
            },
            ProgramLanguage.Java => dataType switch
            {
                DataType.Int => "int",
                DataType.Long => "long",
                DataType.String => "String",
                DataType.Boolean => "boolean",
                DataType.Double => "double",
                DataType.Float => "float",
                DataType.Decimal => "BigDecimal",
                DataType.Void => "void",
                DataType.Object => "Object",
                DataType.Guid => "UUID",
                DataType.DateTime => "LocalDateTime",
                DataType.Date => "LocalDate",
                DataType.Time => "LocalTime",
                _ => "Object"
            },
            ProgramLanguage.Go => dataType switch
            {
                DataType.Int => "int",
                DataType.Long => "int64",
                DataType.String => "string",
                DataType.Boolean => "bool",
                DataType.Double => "float64",
                DataType.Float => "float32",
                DataType.Decimal => "float64",
                DataType.Void => "",
                DataType.Object => "interface{}",
                DataType.Guid => "string",
                DataType.DateTime => "time.Time",
                DataType.Date => "time.Time",
                DataType.Time => "time.Time",
                _ => "interface{}"
            },
            _ => dataType.ToString()
        };
    }

    private string GetKeyTypeSyntax(ProgramLanguage language)
    {
        if (KeyType == DataType.Custom)
            return GetCustomKeyTypeName();

        return GetPrimitiveTypeSyntax(language, KeyType);
    }

    private string GetCollectionSyntax(ProgramLanguage language, string elementType)
    {
        var keyType = GetKeyTypeSyntax(language);

        return language switch
        {
            ProgramLanguage.CSharp => SemanticCollection switch
            {
                SemanticCollection.List => $"List<{elementType}>",
                SemanticCollection.Array => $"{elementType}[]",
                SemanticCollection.Set => $"HashSet<{elementType}>",
                SemanticCollection.Dictionary => $"Dictionary<{keyType}, {elementType}>",
                SemanticCollection.Map => $"Dictionary<{keyType}, {elementType}>",
                SemanticCollection.Queue => $"Queue<{elementType}>",
                SemanticCollection.Stack => $"Stack<{elementType}>",
                SemanticCollection.LinkedList => $"LinkedList<{elementType}>",
                SemanticCollection.ReadOnlyList => $"IReadOnlyList<{elementType}>",
                _ => $"List<{elementType}>"
            },
            ProgramLanguage.TypeScript => SemanticCollection switch
            {
                SemanticCollection.Array => $"{elementType}[]",
                SemanticCollection.Set => $"Set<{elementType}>",
                SemanticCollection.Map or SemanticCollection.Dictionary => $"Map<{keyType}, {elementType}>",
                _ => $"Array<{elementType}>"
            },
            ProgramLanguage.Python => SemanticCollection switch
            {
                SemanticCollection.List => $"list[{elementType}]",
                SemanticCollection.Set => $"set[{elementType}]",
                SemanticCollection.Dictionary or SemanticCollection.Map => $"dict[{keyType}, {elementType}]",
                _ => $"list[{elementType}]"
            },
            ProgramLanguage.Java => SemanticCollection switch
            {
                SemanticCollection.List => $"ArrayList<{elementType}>",
                SemanticCollection.Array => $"{elementType}[]",
                SemanticCollection.Set => $"HashSet<{elementType}>",
                SemanticCollection.Dictionary or SemanticCollection.Map => $"HashMap<{keyType}, {elementType}>",
                _ => $"List<{elementType}>"
            },
            ProgramLanguage.Go => SemanticCollection switch
            {
                SemanticCollection.Array => $"[]{elementType}",
                SemanticCollection.Map or SemanticCollection.Dictionary => $"map[{keyType}]{elementType}",
                _ => $"[]{elementType}"
            },
            _ => $"List<{elementType}>"
        };
    }

    private string GetNullableSyntax(ProgramLanguage language, string baseTypeSyntax)
    {
        return language switch
        {
            ProgramLanguage.CSharp => $"{baseTypeSyntax}?",
            ProgramLanguage.TypeScript => $"{baseTypeSyntax} | null",
            ProgramLanguage.Python => $"Optional[{baseTypeSyntax}]",
            ProgramLanguage.Java => $"Optional<{baseTypeSyntax}>",
            ProgramLanguage.Go => $"*{baseTypeSyntax}",
            _ => baseTypeSyntax
        };
    }

    /// <summary>
    /// Vygeneruje kód typu.
    /// </summary>
    public string GenerateCode()
    {
        return CurrentSyntax;
    }

    public override ProgramLanguage TargetLanguage
    {
        get => base.TargetLanguage;
        set
        {
            if (base.TargetLanguage != value)
            {
                base.TargetLanguage = value;
                RecalculateSyntax();
            }
        }
    }
}
