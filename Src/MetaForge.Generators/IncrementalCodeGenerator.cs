using System.Security.Cryptography;
using System.Text;
using MetaForge.Core.Abstractions;
using MetaForge.Generators.Monetization;

namespace MetaForge.Generators;

/// <summary>
/// Inkrementální generátor — generuje pouze elementy, jejichž kód se změnil.
/// Používá SHA256 hash pro detekci změn.
/// [IncrementalCodeGenerator je Experimental — cíl je dirty-tracking na úrovni BusinessModel]
/// </summary>
public sealed class IncrementalCodeGenerator
{
    private readonly Dictionary<string, string> _outputCache = new();
    private readonly TieredCodeGenerator _tiered;
    private readonly GeneratorLicense _license;
    private int _entityCount;

    /// <summary>
    /// Vytvoří inkrementální generátor s licencí.
    /// </summary>
    public IncrementalCodeGenerator(GeneratorLicense license)
    {
        _tiered = new TieredCodeGenerator(license);
        _license = license;
    }

    /// <summary>
    /// Vygeneruje kód pro všechny elementy. Přeskočí nezměněné.
    /// </summary>
    /// <param name="elements">Elementy ke generování.</param>
    /// <param name="outputDirectory">Výstupní adresář.</param>
    /// <returns>Seznam vygenerovaných artefaktů (pouze změněné nebo nové).</returns>
    public IReadOnlyList<GeneratedCodeArtifact> GenerateIncremental(
        IEnumerable<RootElement> elements,
        string outputDirectory)
    {
        var results = new List<GeneratedCodeArtifact>();

        foreach (var element in elements)
        {
            // Kontrola limitu entit
            _entityCount++;
            if (_entityCount > GetMaxEntities())
            {
                throw LicenseException.EntityLimitExceeded(GetMaxEntities(), _entityCount);
            }

            var artifact = _tiered.Generate(element);
            var filePath = Path.Combine(outputDirectory, artifact.FileName);
            var newHash = ComputeHash(artifact.SourceCode);

            // Přeskočit pokud se kód nezměnil
            if (_outputCache.TryGetValue(filePath, out var cachedHash) && cachedHash == newHash)
                continue;

            _outputCache[filePath] = newHash;
            results.Add(artifact);
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Vypočítá SHA256 hash zdrojového kódu.
    /// </summary>
    private static string ComputeHash(string sourceCode)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sourceCode));
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Vymaže cache — vynutí regeneraci všech souborů.
    /// </summary>
    public void InvalidateCache()
    {
        _outputCache.Clear();
    }

    /// <summary>
    /// Vrátí maximální počet entit podle licence.
    /// </summary>
    private int GetMaxEntities()
    {
        return _license.MaxEntities;
    }
}
