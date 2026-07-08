using MetaForge.Core.Common;

namespace MetaForge.Core.StandardLibraries;

public interface IStandardLibraryTranslatorRegistry
{
    void Register(IStandardLibraryTranslator translator);
    bool TryGetFunctionMappings(string libraryName, ProgramLanguage language, out IReadOnlyDictionary<string, string> mappings);
    bool TryGetTranslator(string libraryName, out IStandardLibraryTranslator translator);
    IReadOnlyCollection<string> GetRegisteredLibraries();
    void Reset();
}
