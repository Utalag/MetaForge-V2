using MetaForge.Core.Common;

namespace MetaForge.Core.StandardLibraries;

public sealed class StandardLibraryTranslatorRegistry : IStandardLibraryTranslatorRegistry
{
    private static StandardLibraryTranslatorRegistry? _instance;

    private readonly Lock _syncRoot = new();
    private readonly Dictionary<string, IStandardLibraryTranslator> _translators = new(StringComparer.OrdinalIgnoreCase);

    public static StandardLibraryTranslatorRegistry Instance => _instance ??= new StandardLibraryTranslatorRegistry();

    public static void SetGlobalInstance(StandardLibraryTranslatorRegistry registry)
    {
        _instance = registry;
    }

    public void Register(IStandardLibraryTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(translator);

        lock (_syncRoot)
        {
            _translators[translator.LibraryName] = translator;
        }
    }

    public bool TryGetFunctionMappings(string libraryName, ProgramLanguage language, out IReadOnlyDictionary<string, string> mappings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(libraryName);

        if (TryGetTranslator(libraryName, out var translator))
        {
            mappings = translator.GetFunctionMappings(language);
            return true;
        }

        mappings = new Dictionary<string, string>();
        return false;
    }

    public bool TryGetTranslator(string libraryName, out IStandardLibraryTranslator translator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(libraryName);

        lock (_syncRoot)
        {
            if (_translators.TryGetValue(libraryName, out var registeredTranslator))
            {
                translator = registeredTranslator;
                return true;
            }
        }

        translator = null!;
        return false;
    }

    public IReadOnlyCollection<string> GetRegisteredLibraries()
    {
        lock (_syncRoot)
        {
            return [.. _translators.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase)];
        }
    }

    public void Reset()
    {
        lock (_syncRoot)
        {
            _translators.Clear();
        }
    }
}
