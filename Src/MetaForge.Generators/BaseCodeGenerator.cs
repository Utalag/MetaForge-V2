using MetaForge.Core.Abstractions;

namespace MetaForge.Generators;

/// <summary>
/// Abstraktní bázová třída pro všechny generátory kódu.
/// </summary>
public abstract class BaseCodeGenerator
{
    /// <summary>Identifikátor jazyka (např. "csharp").</summary>
    public abstract string LanguageId { get; }

    /// <summary>Přípona souboru (např. ".cs").</summary>
    public abstract string FileExtension { get; }

    /// <summary>Vygeneruje kód pro daný RootElement.</summary>
    public abstract GeneratedCodeArtifact Generate(RootElement element);

    /// <summary>Vygeneruje kód pro více elementů najednou (např. celý namespace).</summary>
    public virtual IReadOnlyList<GeneratedCodeArtifact> GenerateAll(IEnumerable<RootElement> elements)
    {
        var results = new List<GeneratedCodeArtifact>();
        foreach (var element in elements)
        {
            var artifact = Generate(element);
            results.Add(artifact);
        }
        return results.AsReadOnly();
    }
}
