namespace MetaForge.Core.Abstractions;

/// <summary>
/// Bázová třída pro top-level deklarace (Class, Interface, Enum, Struct).
/// Nese Id, název, usingy, atributy a kreditovou cenu.
/// </summary>
public abstract class RootElement
{
    /// <summary>Unikátní identifikátor elementu.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Název elementu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Druh elementu — implementuje potomek.</summary>
    public abstract string Kind { get; }

    /// <summary>
    /// Namespace elementu. Pro C# top-level declarations; null = bez namespace (global::).
    /// Nepovinné — Translator/Generator doplní defaultní namespace z ProjectElement.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// XML dokumentační komentář (summary) elementu. Volitelný, používá se pro
    /// generování XML docs (<c>&lt;summary&gt;</c>) a jako vstup pro AI prompt engineering.
    /// </summary>
    public string? XmlSummary { get; set; }

    /// <summary>Using direktivy potřebné pro tento element.</summary>
    public List<string> Usings { get; } = new();

    /// <summary>Atributy na tomto elementu.</summary>
    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>
    /// Univerzální key-value anotace (dokumentace, validace, generátorové hinty, AI kontext).
    /// Komplementární k Attributes — C# <c>[Attribute]</c> jde do Attributes, vše ostatní sem.
    /// </summary>
    public MetadataBag Metadata { get; init; } = new();

    /// <summary>Cena elementu v kreditech. Výchozí dle typu.</summary>
    public int Coin { get; set; }

    /// <summary>Coin tohoto elementu + children. Přetěžují potomci.</summary>
    public virtual int TotalCoin => Coin;

    // === Fluent setter metody ===

    /// <summary>Nastaví namespace elementu. Vrací this pro fluent chaining.</summary>
    public RootElement WithNamespace(string? ns)
    {
        Namespace = ns;
        return this;
    }

    /// <summary>Nastaví XML documentation summary. Vrací this pro fluent chaining.</summary>
    public RootElement WithXmlSummary(string? summary)
    {
        XmlSummary = summary;
        return this;
    }
}
