namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Registr překladačů sémantických operací na standardní knihovnu.
/// </summary>
public interface IStandardLibraryTranslatorRegistry
{
    void Register(IStandardLibraryTranslator translator);
    IStandardLibraryTranslator? Resolve(string operationId);
    IReadOnlyList<IStandardLibraryTranslator> GetAll();
}
