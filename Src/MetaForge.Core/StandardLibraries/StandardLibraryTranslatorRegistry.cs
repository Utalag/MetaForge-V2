namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Výchozí implementace registru překladačů.
/// </summary>
public sealed class StandardLibraryTranslatorRegistry : IStandardLibraryTranslatorRegistry
{
    private readonly Dictionary<string, IStandardLibraryTranslator> _translators = new();

    public void Register(IStandardLibraryTranslator translator)
    {
        _translators[translator.OperationId] = translator;
    }

    public IStandardLibraryTranslator? Resolve(string operationId) =>
        _translators.TryGetValue(operationId, out var t) ? t : null;

    public IReadOnlyList<IStandardLibraryTranslator> GetAll() =>
        _translators.Values.ToList().AsReadOnly();
}
