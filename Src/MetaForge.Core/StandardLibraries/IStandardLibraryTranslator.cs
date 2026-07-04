namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Překládá sémantickou operaci na požadavky standardní knihovny.
/// </summary>
public interface IStandardLibraryTranslator
{
    /// <summary>Identifikátor operace, kterou překladač obsluhuje.</summary>
    string OperationId { get; }

    /// <summary>Přeloží operaci na požadavky. Vrací null pokud operaci nerozumí.</summary>
    StandardLibraryRequirements? Translate(string operationId);
}
