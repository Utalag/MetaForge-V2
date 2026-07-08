using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Inference.Boundary;

/// <summary>
/// Rozhraní pro doménově-specifické analyzéry hraničních stavů.
/// Každý analyzér se specializuje na určitou doménu (matematika, stringy, kolekce, ...).
/// </summary>
public interface IDomainAnalyzer
{
    /// <summary>
    /// Název domény, kterou analyzér pokrývá.
    /// Používá se pro registraci a vyhledávání.
    /// </summary>
    string DomainName { get; }
    
    /// <summary>
    /// Klíčová slova, podle kterých se detekuje doména z názvu metody/parametrů.
    /// </summary>
    IReadOnlyList<string> Keywords { get; }
    
    /// <summary>
    /// Priority tohoto analyzéru (vyšší = větší šance, že bude vybrán).
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Indikuje, zda je tento analyzér dostupný.
    /// Některé analyzéry mohou vyžadovat externí závislosti (AI, knihovny).
    /// </summary>
    bool IsAvailable { get; }
    
    /// <summary>
    /// Provede boundary analýzu metody.
    /// </summary>
    /// <param name="method">Metoda k analýze.</param>
    /// <returns>Seznam hraničních stavů (MethodConstraint).</returns>
    Task<IReadOnlyList<MethodConstraint>> AnalyzeAsync(Method method);
    
    /// <summary>
    /// Detekuje, zda tato metoda patří do domény tohoto analyzéru.
    /// </summary>
    bool CanHandle(Method method);
}
