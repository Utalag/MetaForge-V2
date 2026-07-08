using MetaForge.Core.Common;

namespace MetaForge.Core.Abstractions;

/// <summary>
/// Rozhraní pro generátor kódu.
/// Implementováno pluginy pro různé programovací jazyky.
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Cílový programovací jazyk tohoto generátoru.
    /// </summary>
    ProgramLanguage Language { get; }

    /// <summary>
    /// Náklady v kreditech za vygenerování jednoho elementu.
    /// Používá se pro billing: C# = 1, Python = 2, Go = 3, atd.
    /// </summary>
    int CreditCostPerElement { get; }

    /// <summary>
    /// Metadata pluginu (název, verze, autor).
    /// </summary>
    PluginMetadata Metadata { get; }

    /// <summary>
    /// Vygeneruje kód pro daný prvek.
    /// </summary>
    string Generate(ILanguageElement element);

    /// <summary>
    /// Vygeneruje kód pro daný prvek včetně metadat o importech a balíčcích.
    /// </summary>
    GeneratedCodeArtifact GenerateArtifact(ILanguageElement element)
    {
        return new GeneratedCodeArtifact
        {
            Code = Generate(element)
        };
    }
}

/// <summary>
/// Metadata pluginu generátoru.
/// </summary>
public record PluginMetadata
{
    /// <summary>
    /// Název pluginu (např. "MetaForge.CSharp").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Verze pluginu (sémantické verzování).
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Autor pluginu.
    /// </summary>
    public string Author { get; init; } = "MetaForge Team";

    /// <summary>
    /// Popis pluginu.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Cesta k šablonám pluginu.
    /// </summary>
    public string TemplatesPath { get; init; } = string.Empty;
}
