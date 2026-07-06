# Analýza dokumentačních systémů v programovacích jazycích

## 📋 Přehled

Tento dokument analyzuje dokumentační systémy v 5 podporovaných jazycích MetaForge:
- **C#** - XML Documentation Comments
- **TypeScript** - JSDoc / TSDoc
- **Python** - Docstrings (PEP 257)
- **Java** - Javadoc
- **Go** - Godoc

---

## 🔷 C# - XML Documentation Comments

### Syntaxe
```csharp
/// <summary>
/// Popis třídy nebo členu.
/// </summary>
/// <param name="parametr">Popis parametru.</param>
/// <returns>Popis návratové hodnoty.</returns>
/// <exception cref="Exception">Kdy je vyhozena výjimka.</exception>
/// <remarks>Dodatečné poznámky.</remarks>
/// <example>Příklad použití.</example>
/// <seealso cref="JináTřída"/>
public string Metoda(string parametr) { }
```

### Podporované tagy

| Tag | Účel | Příklad |
|-----|------|---------|
| `<summary>` | Hlavní popis | `/// <summary>Popis.</summary>` |
| `<param>` | Popis parametru | `/// <param name="x">Hodnota X.</param>` |
| `<returns>` | Návratová hodnota | `/// <returns>True pokud úspěch.</returns>` |
| `<exception>` | Možné výjimky | `/// <exception cref="ArgumentNullException">` |
| `<remarks>` | Dodatečné info | `/// <remarks>Poznámka.</remarks>` |
| `<example>` | Ukázka kódu | `/// <example>var x = new X();</example>` |
| `<see>` | Inline odkaz | `/// Viz <see cref="Třída"/>` |
| `<seealso>` | Související | `/// <seealso cref="Třída"/>` |
| `<value>` | Popis property | `/// <value>Hodnota vlastnosti.</value>` |
| `<typeparam>` | Generický typ | `/// <typeparam name="T">Typ.</typeparam>` |
| `<inheritdoc>` | Zdědit dokumentaci | `/// <inheritdoc/>` |
| `<c>` | Inline kód | `/// Použij <c>true</c>` |
| `<code>` | Blok kódu | `/// <code>var x = 1;</code>` |
| `<para>` | Nový odstavec | `/// <para>Nový odstavec.</para>` |
| `<list>` | Seznam | `/// <list type="bullet">...</list>` |

### Speciální konstrukce

```csharp
// Region - seskupení kódu
#region Název sekce
// ... kód ...
#endregion

// Podmíněná kompilace
#if DEBUG
/// <summary>Debug verze.</summary>
#endif

// Pragma - potlačení varování
#pragma warning disable CS1591
public void BezDokumentace() { }
#pragma warning restore CS1591
```

### Generování dokumentace
- **Nástroj:** DocFX, Sandcastle
- **Výstup:** HTML, CHM, Markdown
- **Konfigurace:** `<GenerateDocumentationFile>true</GenerateDocumentationFile>` v .csproj

---

## 🟦 TypeScript - JSDoc / TSDoc

### Syntaxe
```typescript
/**
 * Popis funkce nebo třídy.
 * @param parametr - Popis parametru.
 * @returns Popis návratové hodnoty.
 * @throws {Error} Kdy je vyhozena chyba.
 * @example
 * ```typescript
 * const result = funkce("test");
 * ```
 */
function funkce(parametr: string): string { }
```

### Podporované tagy

| Tag | Účel | Příklad |
|-----|------|---------|
| `@param` | Popis parametru | `@param name - Jméno uživatele.` |
| `@returns` | Návratová hodnota | `@returns Výsledek operace.` |
| `@throws` | Možné chyby | `@throws {TypeError} Neplatný typ.` |
| `@example` | Ukázka kódu | `@example const x = 1;` |
| `@see` | Odkaz | `@see {@link Třída}` |
| `@deprecated` | Zastaralé | `@deprecated Použij novou verzi.` |
| `@since` | Verze | `@since 2.0.0` |
| `@author` | Autor | `@author Jan Novák` |
| `@readonly` | Pouze pro čtení | `@readonly` |
| `@private` | Privátní | `@private` |
| `@public` | Veřejné | `@public` |
| `@protected` | Chráněné | `@protected` |
| `@internal` | Interní | `@internal` |
| `@typeParam` | Generický typ | `@typeParam T - Typ položky.` |
| `@defaultValue` | Výchozí hodnota | `@defaultValue 0` |

### TSDoc specifické
```typescript
/**
 * {@inheritDoc Parent.method}
 * {@link Class#method | Odkaz s textem}
 * {@label CUSTOM_LABEL}
 */
```

### Generování dokumentace
- **Nástroj:** TypeDoc, JSDoc
- **Výstup:** HTML, Markdown, JSON
- **Konfigurace:** `typedoc.json` nebo CLI parametry

---

## 🐍 Python - Docstrings (PEP 257)

### Syntaxe

#### Google Style (doporučený)
```python
def funkce(parametr: str) -> str:
    """Krátký popis funkce.

    Delší popis funkce který může být
    na více řádků.

    Args:
        parametr: Popis parametru.

    Returns:
        Popis návratové hodnoty.

    Raises:
        ValueError: Kdy je vyhozena výjimka.

    Examples:
        >>> funkce("test")
        'result'
    """
    pass
```

#### NumPy Style
```python
def funkce(parametr: str) -> str:
    """
    Krátký popis funkce.

    Parameters
    ----------
    parametr : str
        Popis parametru.

    Returns
    -------
    str
        Popis návratové hodnoty.

    Raises
    ------
    ValueError
        Kdy je vyhozena výjimka.

    Examples
    --------
    >>> funkce("test")
    'result'
    """
    pass
```

#### Sphinx Style (reStructuredText)
```python
def funkce(parametr: str) -> str:
    """Krátký popis funkce.

    :param parametr: Popis parametru.
    :type parametr: str
    :returns: Popis návratové hodnoty.
    :rtype: str
    :raises ValueError: Kdy je vyhozena výjimka.

    .. note::
        Poznámka.

    .. warning::
        Varování.
    """
    pass
```

### Podporované sekce (Google Style)

| Sekce | Účel | Příklad |
|-------|------|---------|
| `Args:` | Parametry | `Args:\n    x: Hodnota.` |
| `Returns:` | Návratová hodnota | `Returns:\n    Výsledek.` |
| `Raises:` | Výjimky | `Raises:\n    ValueError: Chyba.` |
| `Yields:` | Pro generátory | `Yields:\n    Položka.` |
| `Examples:` | Příklady | `Examples:\n    >>> f(1)` |
| `Attributes:` | Atributy třídy | `Attributes:\n    name: Jméno.` |
| `Note:` | Poznámka | `Note:\n    Důležité.` |
| `Warning:` | Varování | `Warning:\n    Pozor.` |
| `Todo:` | K dokončení | `Todo:\n    Dodělat.` |
| `See Also:` | Související | `See Also:\n    jiná_funkce` |

### Speciální konstrukce
```python
# Type hints v docstringu (starší styl)
"""
:type parametr: str
:rtype: int
"""

# Doctest - spustitelné příklady
"""
>>> 1 + 1
2
"""
```

### Generování dokumentace
- **Nástroj:** Sphinx, pdoc, pydoc
- **Výstup:** HTML, PDF, ePub
- **Konfigurace:** `conf.py` pro Sphinx

---

## ☕ Java - Javadoc

### Syntaxe
```java
/**
 * Popis třídy nebo metody.
 *
 * <p>Druhý odstavec s HTML formátováním.</p>
 *
 * @param parametr popis parametru
 * @return popis návratové hodnoty
 * @throws Exception kdy je vyhozena výjimka
 * @see JináTřída
 * @since 1.0
 * @author Jan Novák
 */
public String metoda(String parametr) throws Exception { }
```

### Podporované tagy

| Tag | Účel | Příklad |
|-----|------|---------|
| `@param` | Popis parametru | `@param name jméno uživatele` |
| `@return` | Návratová hodnota | `@return výsledek operace` |
| `@throws` / `@exception` | Možné výjimky | `@throws IOException chyba IO` |
| `@see` | Odkaz | `@see java.lang.String` |
| `@since` | Verze | `@since 1.5` |
| `@author` | Autor | `@author Jan Novák` |
| `@version` | Verze | `@version 2.0` |
| `@deprecated` | Zastaralé | `@deprecated použij novou verzi` |
| `@serial` | Serializace | `@serial popis pole` |
| `@serialField` | Pole serializace | `@serialField name String` |
| `@serialData` | Data serializace | `@serialData popis dat` |
| `@link` | Inline odkaz | `{@link Class#method}` |
| `@linkplain` | Odkaz bez fontu | `{@linkplain Class text}` |
| `@code` | Inline kód | `{@code true}` |
| `@literal` | Literál | `{@literal <tag>}` |
| `@value` | Hodnota konstanty | `{@value #KONSTANTA}` |
| `@inheritDoc` | Zdědit dokumentaci | `{@inheritDoc}` |
| `@docRoot` | Kořen dokumentace | `{@docRoot}/images/logo.png` |

### HTML v Javadoc
```java
/**
 * <p>Odstavec s <b>tučným</b> textem.</p>
 * <ul>
 *   <li>Položka 1</li>
 *   <li>Položka 2</li>
 * </ul>
 * <pre>{@code
 * // Blok kódu
 * var x = new X();
 * }</pre>
 */
```

### Generování dokumentace
- **Nástroj:** javadoc (součást JDK)
- **Výstup:** HTML
- **Konfigurace:** CLI parametry nebo Maven/Gradle plugin

---

## 🔵 Go - Godoc

### Syntaxe
```go
// Package mujpackage poskytuje funkce pro...
// 
// Delší popis package může být na více řádků.
package mujpackage

// Funkce provádí nějakou operaci.
// 
// Parametr x je vstupní hodnota.
// Vrací výsledek operace.
//
// Příklad použití:
//
//	result := Funkce(42)
//	fmt.Println(result)
func Funkce(x int) int {
    return x * 2
}
```

### Pravidla Godoc

| Pravidlo | Popis | Příklad |
|----------|-------|---------|
| První věta | Stručný popis (končí tečkou) | `// Funkce vrací součet.` |
| Jméno na začátku | Dokumentace začíná jménem | `// User reprezentuje uživatele.` |
| Prázdný řádek | Odděluje odstavce | `// První.\n//\n// Druhý.` |
| Odsazení | Kód v dokumentaci | `//  kód (2 mezery)` |
| Tab | Také kód | `//\t kód` |

### Speciální sekce
```go
// Deprecated: Použij NováFunkce místo toho.
func StaráFunkce() {}

// BUG(autor): Popis známé chyby.

// TODO(autor): K dokončení.
```

### Příklady (Example functions)
```go
// Speciální funkce pro dokumentaci
func ExampleFunkce() {
    result := Funkce(42)
    fmt.Println(result)
    // Output: 84
}

func ExampleFunkce_suffix() {
    // Varianta příkladu
    // Output: něco
}
```

### Generování dokumentace
- **Nástroj:** godoc, pkgsite (go.dev)
- **Výstup:** HTML, plain text
- **Konfigurace:** Automaticky z komentářů

---

## 📊 Srovnávací tabulka

| Vlastnost | C# | TypeScript | Python | Java | Go |
|-----------|----|-----------:|--------|------|-----|
| **Syntaxe** | `///` XML | `/** */` JSDoc | `"""` Docstring | `/** */` Javadoc | `//` |
| **Formát** | XML | Markdown | Plain/RST | HTML | Plain |
| **Parametry** | `<param>` | `@param` | `Args:` | `@param` | Popis v textu |
| **Návrat** | `<returns>` | `@returns` | `Returns:` | `@return` | Popis v textu |
| **Výjimky** | `<exception>` | `@throws` | `Raises:` | `@throws` | Popis v textu |
| **Příklady** | `<example>` | `@example` | `Examples:` | HTML `<pre>` | Odsazení |
| **Odkazy** | `<see cref="">` | `{@link}` | `:ref:` | `{@link}` | Jméno |
| **Dědičnost** | `<inheritdoc>` | `{@inheritDoc}` | - | `{@inheritDoc}` | - |
| **Generátor** | DocFX | TypeDoc | Sphinx | javadoc | godoc |

---

## 🎯 Doporučení pro MetaForge

### Implementace v Comment.cs

```csharp
public enum CommentType
{
    SingleLine,      // Jednořádkový: // # 
    MultiLine,       // Víceřádkový: /* */ """ 
    Documentation,   // Dokumentační: /// /** """
    Region,          // C# region
    EndRegion,       // C# endregion
    
    // Nové typy pro rozšíření:
    Deprecated,      // @deprecated / Deprecated:
    Todo,            // TODO:
    Bug,             // BUG:
    Note,            // Note: / Poznámka
    Warning          // Warning: / Varování
}
```

### Rozšíření Comment třídy

```csharp
public class Comment : RootElement, ILanguageElement
{
    public string Text { get; set; }
    public CommentType CommentType { get; set; }
    
    // Nové vlastnosti:
    public string? Author { get; set; }
    public string? Since { get; set; }
    public string? Version { get; set; }
    public bool IsDeprecated { get; set; }
    public string? DeprecationMessage { get; set; }
    public List<CommentParam> Parameters { get; } = new();
    public string? Returns { get; set; }
    public List<CommentException> Exceptions { get; } = new();
    public List<string> Examples { get; } = new();
    public List<string> SeeAlso { get; } = new();
}

public class CommentParam
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Type { get; set; }
}

public class CommentException
{
    public string Type { get; set; }
    public string Description { get; set; }
}
```

### Použití v šablonách

```scriban
{{~ ## Dokumentační komentář ~}}
{{- if documentation }}
{{- if target_language == "CSharp" }}
/// <summary>
/// {{ documentation.text }}
/// </summary>
{{- for param in documentation.parameters }}
/// <param name="{{ param.name }}">{{ param.description }}</param>
{{- end }}
{{- if documentation.returns }}
/// <returns>{{ documentation.returns }}</returns>
{{- end }}
{{- end }}
{{- end }}
```

---

## 📚 Reference

- [C# XML Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
- [TSDoc Specification](https://tsdoc.org/)
- [PEP 257 - Docstring Conventions](https://peps.python.org/pep-0257/)
- [Google Python Style Guide](https://google.github.io/styleguide/pyguide.html#38-comments-and-docstrings)
- [Javadoc Guide](https://docs.oracle.com/javase/8/docs/technotes/tools/windows/javadoc.html)
- [Effective Go - Commentary](https://go.dev/doc/effective_go#commentary)

---

**Vytvořeno:** 2025  
**Verze:** 1.0  
**Autor:** MetaForge-Ant
