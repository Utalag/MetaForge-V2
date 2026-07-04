namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Resolver — pro daný element vrátí seznam potřebných using direktiv.
/// </summary>
public sealed class StandardLibraryRequirementResolver
{
    private readonly IStandardLibraryTranslatorRegistry _registry;

    public StandardLibraryRequirementResolver(IStandardLibraryTranslatorRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>Vyřeší požadavky pro danou operaci.</summary>
    public StandardLibraryRequirements? Resolve(string operationId)
    {
        var translator = _registry.Resolve(operationId);
        return translator?.Translate(operationId);
    }

    /// <summary>Vrátí všechny potřebné namespaces pro seznam operací.</summary>
    public IReadOnlyList<string> GetRequiredNamespaces(IEnumerable<string> operationIds)
    {
        var namespaces = new HashSet<string>();
        foreach (var opId in operationIds)
        {
            var req = Resolve(opId);
            if (req?.RequiredNamespaces is not null)
                foreach (var ns in req.RequiredNamespaces)
                    namespaces.Add(ns);
        }
        return namespaces.ToList().AsReadOnly();
    }
}
