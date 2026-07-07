using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Reprezentuje C# event — typovaná notifikace na bázi delegáta.
/// Např. `public event EventHandler&lt;OrderEventArgs&gt; OrderPlaced;`.
/// </summary>
public sealed class EventElement
{
    /// <summary>Název eventu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Název typu delegáta (např. "EventHandler", "Action&lt;string&gt;").</summary>
    public string DelegateTypeName { get; set; } = string.Empty;

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsStatic { get; set; }

    /// <summary>
    /// Má vlastní add/remove accessory (field-like event nemá)?
    /// Pokud true, generuje se `event T Name { add { } remove { } }`.
    /// </summary>
    public bool HasCustomAccessors { get; set; }

    /// <summary>Atributy na eventu.</summary>
    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 3;

    /// <summary>Vytvoří field-like event (bez vlastních accessorů).</summary>
    public static EventElement Basic(string name, string delegateTypeName) => new()
    {
        Name = name,
        DelegateTypeName = delegateTypeName,
    };

    /// <summary>Nastaví access modifier.</summary>
    public EventElement WithAccess(AccessModifier access)
    {
        AccessModifier = access;
        return this;
    }
}
