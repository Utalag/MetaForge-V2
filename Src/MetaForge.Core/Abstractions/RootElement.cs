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

    /// <summary>Using direktivy potřebné pro tento element.</summary>
    public List<string> Usings { get; } = new();

    /// <summary>Atributy na tomto elementu.</summary>
    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>Cena elementu v kreditech. Výchozí dle typu.</summary>
    public int Coin { get; set; }

    /// <summary>Coin tohoto elementu + children. Přetěžují potomci.</summary>
    public virtual int TotalCoin => Coin;
}
