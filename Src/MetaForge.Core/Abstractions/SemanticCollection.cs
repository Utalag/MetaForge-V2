namespace MetaForge.Core.Abstractions;

/// <summary>
/// Kolekce child elementů s notifikací o změnách.
/// Používá se pro Properties, Methods, Members — kdekoliv kde child elementy ovlivňují TotalCoin.
/// </summary>
public class SemanticCollection<T> : List<T>
{
    /// <summary>Událost vyvolaná při jakékoliv změně kolekce.</summary>
    public event Action? Changed;

    /// <summary>Přidá prvek a vyvolá Changed.</summary>
    public new void Add(T item)
    {
        base.Add(item);
        Changed?.Invoke();
    }

    /// <summary>Odebere prvek a vyvolá Changed.</summary>
    public new void Remove(T item)
    {
        base.Remove(item);
        Changed?.Invoke();
    }

    /// <summary>Vyčistí kolekci a vyvolá Changed.</summary>
    public new void Clear()
    {
        base.Clear();
        Changed?.Invoke();
    }
}
