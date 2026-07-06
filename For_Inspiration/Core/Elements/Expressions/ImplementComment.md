# Implementační plán pro rozšíření Comment systému

## 📋 Přehled

Tento dokument popisuje krok-za-krokem implementaci pokročilého dokumentačního systému pro MetaForge založeného na analýze v `CommentAnalyze.md`.

---

## 🎯 Cíle implementace

1. **Rozšířit `Comment` třídu** o strukturované vlastnosti (parametry, návratové hodnoty, výjimky)
2. **Přidat nové typy komentářů** (Deprecated, Todo, Bug, Note, Warning)
3. **Vytvořit pomocné třídy** (`CommentParam`, `CommentException`, `CommentExample`)
4. **Aktualizovat GenerateCode()** pro všechny jazyky s podporou pokročilých tagů
5. **Vytvořit Scriban šablony** pro dokumentační komentáře
6. **Integrovat s Property a Method** třídami

---

## 📐 Fáze 1: Rozšíření datového modelu

### Krok 1.1: Rozšíření CommentType enum

**Soubor:** `Src/MetaForge.Core/Elements/Expressions/Comment.cs`

```csharp
/// <summary>
/// Typ komentáře s rozšířenou podporou.
/// </summary>
public enum CommentType
{
    // Základní typy
    /// <summary>Jednořádkový komentář (// nebo #).</summary>
    SingleLine,

    /// <summary>Víceřádkový komentář (/* */ nebo """).</summary>
    MultiLine,

    /// <summary>Dokumentační komentář (/// nebo /** */).</summary>
    Documentation,

    /// <summary>Region (pouze C#).</summary>
    Region,

    /// <summary>End region (pouze C#).</summary>
    EndRegion,

    // Rozšířené typy
    /// <summary>Označení zastaralého kódu (@deprecated).</summary>
    Deprecated,

    /// <summary>TODO komentář (k dokončení).</summary>
    Todo,

    /// <summary>Poznámka o známé chybě (BUG).</summary>
    Bug,

    /// <summary>Důležitá poznámka (Note/Remarks).</summary>
    Note,

    /// <summary>Varování (Warning).</summary>
    Warning,

    /// <summary>Příklad použití (Example).</summary>
    Example,

    /// <summary>Odkaz na související prvek (See Also).</summary>
    SeeAlso
}
```

### Krok 1.2: Vytvoření pomocných tříd

**Soubor:** `Src/MetaForge.Core/Elements/Expressions/CommentParam.cs`

```csharp
using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Popis parametru v dokumentačním komentáři.
/// </summary>
public class CommentParam : RootElement
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string? _type;

    /// <summary>
    /// Název parametru.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Popis parametru.
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
    /// Typ parametru (volitelné, pro některé jazyky).
    /// </summary>
    public string? Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                OnPropertyChanged();
            }
        }
    }
}
```

**Soubor:** `Src/MetaForge.Core/Elements/Expressions/CommentException.cs`

```csharp
using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Popis výjimky v dokumentačním komentáři.
/// </summary>
public class CommentException : RootElement
{
    private string _type = string.Empty;
    private string _description = string.Empty;

    /// <summary>
    /// Typ výjimky (např. ArgumentNullException, ValueError).
    /// </summary>
    public string Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Popis kdy je výjimka vyhozena.
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
}
```

**Soubor:** `Src/MetaForge.Core/Elements/Expressions/CommentExample.cs`

```csharp
using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Příklad kódu v dokumentačním komentáři.
/// </summary>
public class CommentExample : RootElement
{
    private string _code = string.Empty;
    private string? _description;
    private string? _expectedOutput;

    /// <summary>
    /// Kód příkladu.
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
    /// Popis příkladu (volitelné).
    /// </summary>
    public string? Description
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
    /// Očekávaný výstup (pro Go/Python doctest).
    /// </summary>
    public string? ExpectedOutput
    {
        get => _expectedOutput;
        set
        {
            if (_expectedOutput != value)
            {
                _expectedOutput = value;
                OnPropertyChanged();
            }
        }
    }
}
```

### Krok 1.3: Rozšíření Comment třídy

**Soubor:** `Src/MetaForge.Core/Elements/Expressions/Comment.cs`

```csharp
using System.Collections.ObjectModel;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Comment (komentář) - rozšířený dokumentační systém.
/// </summary>
public class Comment : RootElement, ILanguageElement
{
    private string _text = string.Empty;
    private CommentType _commentType = CommentType.SingleLine;
    private string? _author;
    private string? _since;
    private string? _version;
    private bool _isDeprecated;
    private string? _deprecationMessage;
    private string? _returns;

    /// <summary>
    /// Text komentáře (hlavní popis).
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Typ komentáře.
    /// </summary>
    public CommentType CommentType
    {
        get => _commentType;
        set
        {
            if (_commentType != value)
            {
                _commentType = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Autor kódu (pro Javadoc, TSDoc).
    /// </summary>
    public string? Author
    {
        get => _author;
        set
        {
            if (_author != value)
            {
                _author = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Verze od které je k dispozici (@since).
    /// </summary>
    public string? Since
    {
        get => _since;
        set
        {
            if (_since != value)
            {
                _since = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Verze kódu (@version pro Javadoc).
    /// </summary>
    public string? Version
    {
        get => _version;
        set
        {
            if (_version != value)
            {
                _version = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Je kód označen jako zastaralý?
    /// </summary>
    public bool IsDeprecated
    {
        get => _isDeprecated;
        set
        {
            if (_isDeprecated != value)
            {
                _isDeprecated = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Zpráva o zastaralosti (@deprecated message).
    /// </summary>
    public string? DeprecationMessage
    {
        get => _deprecationMessage;
        set
        {
            if (_deprecationMessage != value)
            {
                _deprecationMessage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Popis návratové hodnoty (@returns, @return).
    /// </summary>
    public string? Returns
    {
        get => _returns;
        set
        {
            if (_returns != value)
            {
                _returns = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Parametry metody/funkce.
    /// </summary>
    public ObservableCollection<CommentParam> Parameters { get; } = new();

    /// <summary>
    /// Možné výjimky/chyby.
    /// </summary>
    public ObservableCollection<CommentException> Exceptions { get; } = new();

    /// <summary>
    /// Příklady použití kódu.
    /// </summary>
    public ObservableCollection<CommentExample> Examples { get; } = new();

    /// <summary>
    /// Odkazy na související třídy/metody (@see, @seealso).
    /// </summary>
    public ObservableCollection<string> SeeAlso { get; } = new();

    /// <summary>
    /// Poznámky a varování.
    /// </summary>
    public ObservableCollection<string> Remarks { get; } = new();

    /// <summary>
    /// Vygeneruje kód komentáře podle cílového jazyka.
    /// </summary>
    public string GenerateCode()
    {
        return TargetLanguage switch
        {
            ProgramLanguage.CSharp => GenerateCSharpComment(),
            ProgramLanguage.TypeScript => GenerateTypeScriptComment(),
            ProgramLanguage.Python => GeneratePythonComment(),
            ProgramLanguage.Java => GenerateJavaComment(),
            ProgramLanguage.Go => GenerateGoComment(),
            _ => GenerateCSharpComment()
        };
    }

    // Implementace jednotlivých generátorů následuje...
}
```

---

## 📐 Fáze 2: Implementace GenerateCode() metod

### Krok 2.1: C# XML Documentation

```csharp
private string GenerateCSharpComment()
{
    if (CommentType == CommentType.SingleLine)
        return $"// {Text}";

    if (CommentType == CommentType.MultiLine)
        return $"/* {Text} */";

    if (CommentType == CommentType.Region)
        return $"#region {Text}";

    if (CommentType == CommentType.EndRegion)
        return "#endregion";

    // Documentation comment
    var sb = new StringBuilder();

    // Summary
    sb.AppendLine("/// <summary>");
    sb.AppendLine($"/// {Text}");
    sb.AppendLine("/// </summary>");

    // Deprecated
    if (IsDeprecated)
    {
        sb.AppendLine("/// <remarks>");
        sb.AppendLine($"/// Deprecated: {DeprecationMessage ?? "Use alternative."}");
        sb.AppendLine("/// </remarks>");
    }

    // Parameters
    foreach (var param in Parameters)
    {
        sb.AppendLine($"/// <param name=\"{param.Name}\">{param.Description}</param>");
    }

    // Returns
    if (!string.IsNullOrWhiteSpace(Returns))
    {
        sb.AppendLine($"/// <returns>{Returns}</returns>");
    }

    // Exceptions
    foreach (var ex in Exceptions)
    {
        sb.AppendLine($"/// <exception cref=\"{ex.Type}\">{ex.Description}</exception>");
    }

    // Examples
    foreach (var example in Examples)
    {
        sb.AppendLine("/// <example>");
        if (!string.IsNullOrWhiteSpace(example.Description))
        {
            sb.AppendLine($"/// {example.Description}");
        }
        sb.AppendLine("/// <code>");
        foreach (var line in example.Code.Split('\n'))
        {
            sb.AppendLine($"/// {line}");
        }
        sb.AppendLine("/// </code>");
        sb.AppendLine("/// </example>");
    }

    // See Also
    foreach (var see in SeeAlso)
    {
        sb.AppendLine($"/// <seealso cref=\"{see}\"/>");
    }

    // Remarks
    if (Remarks.Count > 0)
    {
        sb.AppendLine("/// <remarks>");
        foreach (var remark in Remarks)
        {
            sb.AppendLine($"/// {remark}");
        }
        sb.AppendLine("/// </remarks>");
    }

    // Author, Since, Version
    if (!string.IsNullOrWhiteSpace(Author))
    {
        sb.AppendLine($"/// <remarks>Author: {Author}</remarks>");
    }

    if (!string.IsNullOrWhiteSpace(Since))
    {
        sb.AppendLine($"/// <remarks>Since: {Since}</remarks>");
    }

    if (!string.IsNullOrWhiteSpace(Version))
    {
        sb.AppendLine($"/// <remarks>Version: {Version}</remarks>");
    }

    return sb.ToString().TrimEnd();
}
```

### Krok 2.2: TypeScript JSDoc

```csharp
private string GenerateTypeScriptComment()
{
    if (CommentType == CommentType.SingleLine)
        return $"// {Text}";

    if (CommentType == CommentType.MultiLine)
        return $"/* {Text} */";

    // Documentation comment (JSDoc/TSDoc)
    var sb = new StringBuilder();
    sb.AppendLine("/**");
    sb.AppendLine($" * {Text}");

    // Deprecated
    if (IsDeprecated)
    {
        var msg = DeprecationMessage ?? "Use alternative.";
        sb.AppendLine($" * @deprecated {msg}");
    }

    // Parameters
    foreach (var param in Parameters)
    {
        var typeInfo = string.IsNullOrWhiteSpace(param.Type) 
            ? "" 
            : $"{{{param.Type}}} ";
        sb.AppendLine($" * @param {typeInfo}{param.Name} - {param.Description}");
    }

    // Returns
    if (!string.IsNullOrWhiteSpace(Returns))
    {
        sb.AppendLine($" * @returns {Returns}");
    }

    // Exceptions (throws)
    foreach (var ex in Exceptions)
    {
        sb.AppendLine($" * @throws {{{ex.Type}}} {ex.Description}");
    }

    // Examples
    foreach (var example in Examples)
    {
        sb.AppendLine(" * @example");
        if (!string.IsNullOrWhiteSpace(example.Description))
        {
            sb.AppendLine($" * {example.Description}");
        }
        sb.AppendLine(" * ```typescript");
        foreach (var line in example.Code.Split('\n'))
        {
            sb.AppendLine($" * {line}");
        }
        sb.AppendLine(" * ```");
    }

    // See Also
    foreach (var see in SeeAlso)
    {
        sb.AppendLine($" * @see {{@link {see}}}");
    }

    // Author, Since
    if (!string.IsNullOrWhiteSpace(Author))
    {
        sb.AppendLine($" * @author {Author}");
    }

    if (!string.IsNullOrWhiteSpace(Since))
    {
        sb.AppendLine($" * @since {Since}");
    }

    sb.AppendLine(" */");

    return sb.ToString().TrimEnd();
}
```

### Krok 2.3: Python Docstrings (Google Style)

```csharp
private string GeneratePythonComment()
{
    if (CommentType == CommentType.SingleLine)
        return $"# {Text}";

    if (CommentType == CommentType.MultiLine || CommentType == CommentType.Documentation)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"\"\"\"{Text}");

        // Deprecated
        if (IsDeprecated)
        {
            sb.AppendLine();
            var msg = DeprecationMessage ?? "Use alternative.";
            sb.AppendLine($"Deprecated: {msg}");
        }

        // Parameters
        if (Parameters.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Args:");
            foreach (var param in Parameters)
            {
                var typeInfo = string.IsNullOrWhiteSpace(param.Type) 
                    ? "" 
                    : $" ({param.Type})";
                sb.AppendLine($"    {param.Name}{typeInfo}: {param.Description}");
            }
        }

        // Returns
        if (!string.IsNullOrWhiteSpace(Returns))
        {
            sb.AppendLine();
            sb.AppendLine("Returns:");
            sb.AppendLine($"    {Returns}");
        }

        // Exceptions (Raises)
        if (Exceptions.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Raises:");
            foreach (var ex in Exceptions)
            {
                sb.AppendLine($"    {ex.Type}: {ex.Description}");
            }
        }

        // Examples
        if (Examples.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Examples:");
            foreach (var example in Examples)
            {
                if (!string.IsNullOrWhiteSpace(example.Description))
                {
                    sb.AppendLine($"    {example.Description}");
                }
                foreach (var line in example.Code.Split('\n'))
                {
                    sb.AppendLine($"    >>> {line}");
                }
                if (!string.IsNullOrWhiteSpace(example.ExpectedOutput))
                {
                    sb.AppendLine($"    {example.ExpectedOutput}");
                }
            }
        }

        // Remarks (Note)
        if (Remarks.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Note:");
            foreach (var remark in Remarks)
            {
                sb.AppendLine($"    {remark}");
            }
        }

        sb.Append("\"\"\"");
        return sb.ToString();
    }

    return $"# {Text}";
}
```

### Krok 2.4: Java Javadoc

```csharp
private string GenerateJavaComment()
{
    if (CommentType == CommentType.SingleLine)
        return $"// {Text}";

    if (CommentType == CommentType.MultiLine)
        return $"/* {Text} */";

    // Javadoc comment
    var sb = new StringBuilder();
    sb.AppendLine("/**");
    sb.AppendLine($" * {Text}");

    // Deprecated
    if (IsDeprecated)
    {
        var msg = DeprecationMessage ?? "use alternative";
        sb.AppendLine($" * @deprecated {msg}");
    }

    // Parameters
    foreach (var param in Parameters)
    {
        sb.AppendLine($" * @param {param.Name} {param.Description}");
    }

    // Returns
    if (!string.IsNullOrWhiteSpace(Returns))
    {
        sb.AppendLine($" * @return {Returns}");
    }

    // Exceptions
    foreach (var ex in Exceptions)
    {
        sb.AppendLine($" * @throws {ex.Type} {ex.Description}");
    }

    // See Also
    foreach (var see in SeeAlso)
    {
        sb.AppendLine($" * @see {see}");
    }

    // Author, Version, Since
    if (!string.IsNullOrWhiteSpace(Author))
    {
        sb.AppendLine($" * @author {Author}");
    }

    if (!string.IsNullOrWhiteSpace(Version))
    {
        sb.AppendLine($" * @version {Version}");
    }

    if (!string.IsNullOrWhiteSpace(Since))
    {
        sb.AppendLine($" * @since {Since}");
    }

    // Examples (jako HTML pre tag)
    foreach (var example in Examples)
    {
        sb.AppendLine(" * <pre>{@code");
        foreach (var line in example.Code.Split('\n'))
        {
            sb.AppendLine($" * {line}");
        }
        sb.AppendLine(" * }</pre>");
    }

    sb.AppendLine(" */");

    return sb.ToString().TrimEnd();
}
```

### Krok 2.5: Go Godoc

```csharp
private string GenerateGoComment()
{
    if (CommentType == CommentType.SingleLine || CommentType == CommentType.Documentation)
    {
        var sb = new StringBuilder();
        
        // Hlavní popis (musí začínat jménem funkce/typu)
        sb.AppendLine($"// {Text}");

        // Deprecated
        if (IsDeprecated)
        {
            var msg = DeprecationMessage ?? "Use alternative.";
            sb.AppendLine("//");
            sb.AppendLine($"// Deprecated: {msg}");
        }

        // Parametry a návratové hodnoty popisujeme v textu
        if (Parameters.Count > 0 || !string.IsNullOrWhiteSpace(Returns))
        {
            sb.AppendLine("//");
            foreach (var param in Parameters)
            {
                sb.AppendLine($"// Parameter {param.Name}: {param.Description}");
            }
            if (!string.IsNullOrWhiteSpace(Returns))
            {
                sb.AppendLine($"// Returns: {Returns}");
            }
        }

        // Examples (s odsazením)
        if (Examples.Count > 0)
        {
            sb.AppendLine("//");
            sb.AppendLine("// Example:");
            sb.AppendLine("//");
            foreach (var example in Examples)
            {
                foreach (var line in example.Code.Split('\n'))
                {
                    sb.AppendLine($"//\t{line}");
                }
                if (!string.IsNullOrWhiteSpace(example.ExpectedOutput))
                {
                    sb.AppendLine($"// Output: {example.ExpectedOutput}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    if (CommentType == CommentType.MultiLine)
        return $"/* {Text} */";

    return $"// {Text}";
}
```

---

## 📐 Fáze 3: Integrace s Property a Method

### Krok 3.1: Rozšíření Property.cs

```csharp
// V Property.cs přidat:

/// <summary>
/// Dokumentační komentář property s parametry.
/// </summary>
public Comment? Documentation
{
    get => _documentation;
    set
    {
        if (_documentation != value)
        {
            _documentation = value;
            OnPropertyChanged();
        }
    }
}

// V GenerateCode():
if (Documentation != null)
{
    Documentation.TargetLanguage = TargetLanguage;
    Documentation.CommentType = CommentType.Documentation;
    
    // Pro property může dokumentace obsahovat @value tag
    if (string.IsNullOrWhiteSpace(Documentation.Returns) && HasGetter)
    {
        Documentation.Returns = $"Hodnota vlastnosti {Name}.";
    }
    
    documentationCode = Documentation.GenerateCode();
}
```

### Krok 3.2: Rozšíření Method.cs

```csharp
// V Method.cs přidat:

/// <summary>
/// Dokumentační komentář metody s parametry a návratovou hodnotou.
/// </summary>
public Comment? Documentation
{
    get => _documentation;
    set
    {
        if (_documentation != value)
        {
            _documentation = value;
            OnPropertyChanged();
        }
    }
}

// V GenerateCode():
if (Documentation != null)
{
    Documentation.TargetLanguage = TargetLanguage;
    Documentation.CommentType = CommentType.Documentation;
    
    // Automaticky přidej parametry z Method.Parameters
    Documentation.Parameters.Clear();
    foreach (var param in Parameters)
    {
        Documentation.Parameters.Add(new CommentParam
        {
            Name = param.Name,
            Description = $"Parametr {param.Name}",
            Type = param.Type.CurrentSyntax
        });
    }
    
    // Automaticky přidej návratovou hodnotu
    if (ReturnType.BaseType != DataType.Void && 
        string.IsNullOrWhiteSpace(Documentation.Returns))
    {
        Documentation.Returns = $"Návratová hodnota typu {ReturnType.CurrentSyntax}.";
    }
    
    documentationCode = Documentation.GenerateCode();
}
```

---

## 📐 Fáze 4: Scriban šablony (volitelné)

### Krok 4.1: Vytvoření Comment šablon

**Soubor:** `Templates/CSharp/CommentDocumentation.scriban`

```scriban
{{~ ## C# XML Documentation Comment ~}}
/// <summary>
/// {{ text }}
/// </summary>
{{- if is_deprecated }}
/// <remarks>
/// Deprecated: {{ deprecation_message }}
/// </remarks>
{{- end }}
{{- for param in parameters }}
/// <param name="{{ param.name }}">{{ param.description }}</param>
{{- end }}
{{- if returns }}
/// <returns>{{ returns }}</returns>
{{- end }}
{{- for exception in exceptions }}
/// <exception cref="{{ exception.type }}">{{ exception.description }}</exception>
{{- end }}
{{- for example in examples }}
/// <example>
{{- if example.description }}
/// {{ example.description }}
{{- end }}
/// <code>
{{- for line in example.code_lines }}
/// {{ line }}
{{- end }}
/// </code>
/// </example>
{{- end }}
{{- for see in see_also }}
/// <seealso cref="{{ see }}"/>
{{- end }}
```

**Soubor:** `Templates/TypeScript/CommentDocumentation.scriban`

```scriban
{{~ ## TypeScript JSDoc Comment ~}}
/**
 * {{ text }}
{{- if is_deprecated }}
 * @deprecated {{ deprecation_message }}
{{- end }}
{{- for param in parameters }}
 * @param {{ if param.type }}{{{ param.type }}} {{ end }}{{ param.name }} - {{ param.description }}
{{- end }}
{{- if returns }}
 * @returns {{ returns }}
{{- end }}
{{- for exception in exceptions }}
 * @throws {{{ exception.type }}} {{ exception.description }}
{{- end }}
{{- for example in examples }}
 * @example
{{- if example.description }}
 * {{ example.description }}
{{- end }}
 * ```typescript
{{- for line in example.code_lines }}
 * {{ line }}
{{- end }}
 * ```
{{- end }}
{{- if author }}
 * @author {{ author }}
{{- end }}
{{- if since }}
 * @since {{ since }}
{{- end }}
 */
```

---

## 📐 Fáze 5: Testování

### Krok 5.1: Unit testy pro Comment

**Soubor:** `Tests/MetaForge.Core.Tests/CommentTests.cs`

```csharp
using MetaForge.Core.Common;
using MetaForge.Core.Elements.Expressions;
using Xunit;

namespace MetaForge.Core.Tests;

public class CommentTests
{
    [Fact]
    public void Comment_CSharpDocumentation_WithParameters()
    {
        var comment = new Comment
        {
            Text = "Vypočítá součet dvou čísel.",
            CommentType = CommentType.Documentation,
            TargetLanguage = ProgramLanguage.CSharp,
            Returns = "Součet a + b."
        };

        comment.Parameters.Add(new CommentParam
        {
            Name = "a",
            Description = "První číslo."
        });

        comment.Parameters.Add(new CommentParam
        {
            Name = "b",
            Description = "Druhé číslo."
        });

        var code = comment.GenerateCode();

        Assert.Contains("/// <summary>", code);
        Assert.Contains("/// Vypočítá součet dvou čísel.", code);
        Assert.Contains("/// <param name=\"a\">První číslo.</param>", code);
        Assert.Contains("/// <param name=\"b\">Druhé číslo.</param>", code);
        Assert.Contains("/// <returns>Součet a + b.</returns>", code);
    }

    [Fact]
    public void Comment_TypeScriptDocumentation_WithExamples()
    {
        var comment = new Comment
        {
            Text = "Vytvoří nový uživatel.",
            CommentType = CommentType.Documentation,
            TargetLanguage = ProgramLanguage.TypeScript
        };

        comment.Examples.Add(new CommentExample
        {
            Description = "Základní použití:",
            Code = "const user = new User('John', 'Doe');\nconsole.log(user.fullName);"
        });

        var code = comment.GenerateCode();

        Assert.Contains("/**", code);
        Assert.Contains(" * Vytvoří nový uživatel.", code);
        Assert.Contains(" * @example", code);
        Assert.Contains(" * ```typescript", code);
    }

    [Fact]
    public void Comment_PythonDocstring_GoogleStyle()
    {
        var comment = new Comment
        {
            Text = "Zpracuje data a vrátí výsledek.",
            CommentType = CommentType.Documentation,
            TargetLanguage = ProgramLanguage.Python,
            Returns = "Zpracovaná data jako slovník."
        };

        comment.Parameters.Add(new CommentParam
        {
            Name = "data",
            Type = "dict",
            Description = "Vstupní data ke zpracování."
        });

        comment.Exceptions.Add(new CommentException
        {
            Type = "ValueError",
            Description = "Pokud jsou data neplatná."
        });

        var code = comment.GenerateCode();

        Assert.Contains("\"\"\"Zpracuje data a vrátí výsledek.", code);
        Assert.Contains("Args:", code);
        Assert.Contains("    data (dict): Vstupní data ke zpracování.", code);
        Assert.Contains("Returns:", code);
        Assert.Contains("Raises:", code);
        Assert.Contains("    ValueError: Pokud jsou data neplatná.", code);
    }

    [Fact]
    public void Comment_Deprecated_AllLanguages()
    {
        var comment = new Comment
        {
            Text = "Zastaralá metoda.",
            CommentType = CommentType.Documentation,
            IsDeprecated = true,
            DeprecationMessage = "Použijte NewMethod() místo toho."
        };

        // C#
        comment.TargetLanguage = ProgramLanguage.CSharp;
        var csharpCode = comment.GenerateCode();
        Assert.Contains("Deprecated: Použijte NewMethod() místo toho.", csharpCode);

        // TypeScript
        comment.TargetLanguage = ProgramLanguage.TypeScript;
        var tsCode = comment.GenerateCode();
        Assert.Contains("@deprecated Použijte NewMethod() místo toho.", tsCode);

        // Java
        comment.TargetLanguage = ProgramLanguage.Java;
        var javaCode = comment.GenerateCode();
        Assert.Contains("@deprecated Použijte NewMethod() místo toho.", javaCode);
    }
}
```

---

## 📐 Fáze 6: Příklady použití

### Příklad 1: Dokumentace metody v C#

```csharp
var method = new Method
{
    Name = "CalculateSum",
    ReturnType = new TypeModel { BaseType = DataType.Int },
    TargetLanguage = ProgramLanguage.CSharp
};

method.Parameters.Add(new Parameter
{
    Name = "a",
    Type = new TypeModel { BaseType = DataType.Int }
});

method.Parameters.Add(new Parameter
{
    Name = "b",
    Type = new TypeModel { BaseType = DataType.Int }
});

method.Documentation = new Comment
{
    Text = "Vypočítá součet dvou čísel.",
    CommentType = CommentType.Documentation,
    Returns = "Součet a + b.",
    TargetLanguage = ProgramLanguage.CSharp
};

method.Documentation.Parameters.Add(new CommentParam
{
    Name = "a",
    Description = "První číslo."
});

method.Documentation.Parameters.Add(new CommentParam
{
    Name = "b",
    Description = "Druhé číslo."
});

method.Documentation.Examples.Add(new CommentExample
{
    Description = "Základní použití:",
    Code = "var result = CalculateSum(5, 3);\nConsole.WriteLine(result); // Output: 8"
});

var code = method.GenerateCode();

// Výstup:
// /// <summary>
// /// Vypočítá součet dvou čísel.
// /// </summary>
// /// <param name="a">První číslo.</param>
// /// <param name="b">Druhé číslo.</param>
// /// <returns>Součet a + b.</returns>
// /// <example>
// /// Základní použití:
// /// <code>
// /// var result = CalculateSum(5, 3);
// /// Console.WriteLine(result); // Output: 8
// /// </code>
// /// </example>
// public int CalculateSum(int a, int b)
// {
// }
```

### Příklad 2: TypeScript s JSDoc

```csharp
var method = new Method
{
    Name = "fetchUserData",
    ReturnType = new TypeModel { BaseType = DataType.String }, // Promise<UserData>
    IsAsync = true,
    TargetLanguage = ProgramLanguage.TypeScript
};

method.Parameters.Add(new Parameter
{
    Name = "userId",
    Type = new TypeModel { BaseType = DataType.String }
});

method.Documentation = new Comment
{
    Text = "Načte data uživatele ze serveru.",
    CommentType = CommentType.Documentation,
    Returns = "Promise s daty uživatele.",
    TargetLanguage = ProgramLanguage.TypeScript
};

method.Documentation.Parameters.Add(new CommentParam
{
    Name = "userId",
    Type = "string",
    Description = "ID uživatele k načtení."
});

method.Documentation.Exceptions.Add(new CommentException
{
    Type = "Error",
    Description = "Pokud uživatel nebyl nalezen."
});

method.Documentation.Examples.Add(new CommentExample
{
    Code = "const data = await fetchUserData('user-123');\nconsole.log(data.name);"
});

// Výstup:
// /**
//  * Načte data uživatele ze serveru.
//  * @param {string} userId - ID uživatele k načtení.
//  * @returns Promise s daty uživatele.
//  * @throws {Error} Pokud uživatel nebyl nalezen.
//  * @example
//  * ```typescript
//  * const data = await fetchUserData('user-123');
//  * console.log(data.name);
//  * ```
//  */
```

### Příklad 3: Python Docstring

```csharp
var method = new Method
{
    Name = "process_data",
    ReturnType = new TypeModel { BaseType = DataType.String }, // dict
    TargetLanguage = ProgramLanguage.Python
};

method.Documentation = new Comment
{
    Text = "Zpracuje vstupní data a vrátí výsledek.",
    CommentType = CommentType.Documentation,
    Returns = "dict: Zpracovaná data.",
    TargetLanguage = ProgramLanguage.Python
};

method.Documentation.Parameters.Add(new CommentParam
{
    Name = "input_data",
    Type = "list",
    Description = "Vstupní data ke zpracování."
});

method.Documentation.Exceptions.Add(new CommentException
{
    Type = "ValueError",
    Description = "Pokud jsou data prázdná."
});

method.Documentation.Examples.Add(new CommentExample
{
    Code = "result = process_data([1, 2, 3])\nprint(result)",
    ExpectedOutput = "{'sum': 6, 'count': 3}"
});

// Výstup:
// """Zpracuje vstupní data a vrátí výsledek.
//
// Args:
//     input_data (list): Vstupní data ke zpracování.
//
// Returns:
//     dict: Zpracovaná data.
//
// Raises:
//     ValueError: Pokud jsou data prázdná.
//
// Examples:
//     >>> result = process_data([1, 2, 3])
//     >>> print(result)
//     {'sum': 6, 'count': 3}
// """
```

---

## 🎯 Časový harmonogram implementace

| Fáze | Činnost | Odhadovaný čas | Priority |
|------|---------|----------------|----------|
| **1** | Rozšíření CommentType enum | 30 min | ⭐⭐⭐ |
| **2** | Vytvoření CommentParam, CommentException, CommentExample | 1 hodina | ⭐⭐⭐ |
| **3** | Rozšíření Comment třídy o nové vlastnosti | 1 hodina | ⭐⭐⭐ |
| **4** | Implementace GenerateCSharpComment() | 2 hodiny | ⭐⭐⭐ |
| **5** | Implementace GenerateTypeScriptComment() | 1.5 hodiny | ⭐⭐ |
| **6** | Implementace GeneratePythonComment() | 1.5 hodiny | ⭐⭐ |
| **7** | Implementace GenerateJavaComment() | 1 hodina | ⭐⭐ |
| **8** | Implementace GenerateGoComment() | 1 hodina | ⭐⭐ |
| **9** | Integrace s Property.cs | 30 min | ⭐⭐⭐ |
| **10** | Integrace s Method.cs | 1 hodina | ⭐⭐⭐ |
| **11** | Vytvoření Scriban šablon (volitelné) | 2 hodiny | ⭐ |
| **12** | Unit testy | 3 hodiny | ⭐⭐⭐ |
| **13** | Dokumentace a příklady | 1 hodina | ⭐⭐ |
| **14** | Build a integrace | 1 hodina | ⭐⭐⭐ |

**Celkem:** ~17.5 hodin

---

## ✅ Kontrolní seznam

- [ ] Rozšířen `CommentType` enum o nové typy
- [ ] Vytvořeny třídy `CommentParam`, `CommentException`, `CommentExample`
- [ ] Rozšířena `Comment` třída o nové vlastnosti
- [ ] Implementovány všechny `Generate*Comment()` metody
- [ ] Integrováno s `Property.cs`
- [ ] Integrováno s `Method.cs`
- [ ] Vytvořeny Scriban šablony (volitelné)
- [ ] Napsány unit testy (min. 10 testů)
- [ ] Build úspěšný
- [ ] Všechny testy prošly
- [ ] Vytvořeny příklady použití
- [ ] Aktualizována dokumentace

---

## 📚 Reference

- [CommentAnalyze.md](./CommentAnalyze.md) - Analýza dokumentačních systémů
- [C# XML Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
- [TSDoc Specification](https://tsdoc.org/)
- [PEP 257](https://peps.python.org/pep-0257/)

---

**Vytvořeno:** 2025  
**Verze:** 1.0  
**Autor:** MetaForge-Ant
