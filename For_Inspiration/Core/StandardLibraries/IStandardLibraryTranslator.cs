using MetaForge.Core.Common;

namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Překladač jedné standardní wrapper knihovny z kanonického MetaForge tvaru
/// na konkrétní knihovní volání cílového jazyka.
/// </summary>
public interface IStandardLibraryTranslator
{
    /// <summary>
    /// Kanonický název wrapper knihovny, např. `math`.
    /// </summary>
    string LibraryName { get; }

    /// <summary>
    /// Vrátí mapování funkcí pro zvolený jazyk.
    /// Klíč je kanonický název funkce, hodnota je jazykové volání knihovny.
    /// </summary>
    IReadOnlyDictionary<string, string> GetFunctionMappings(ProgramLanguage language);

    /// <summary>
    /// Vrátí importy nebo namespace direktivy potřebné pro použití knihovny v cílovém jazyce.
    /// </summary>
    IReadOnlyCollection<string> GetRequiredImports(ProgramLanguage language);

    /// <summary>
    /// Vrátí externí balíčky potřebné pro použití knihovny v cílovém jazyce.
    /// System knihovny vrací prázdnou kolekci.
    /// </summary>
    IReadOnlyCollection<CodePackageDependency> GetRequiredPackages(ProgramLanguage language);
}