namespace MetaForge.Core.Diagnostics;

/// <summary>
/// Implementace IDiagnosticCollector — sběr diagnostických zpráv.
/// </summary>
public sealed class DiagnosticBag : IDiagnosticCollector
{
    private readonly List<Diagnostic> _items = [];

    /// <summary>Počet zpráv.</summary>
    public int Count => _items.Count;

    /// <summary>Počet chyb (Severity.Error).</summary>
    public int ErrorCount => _items.Count(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>Počet varování (Severity.Warning).</summary>
    public int WarningCount => _items.Count(d => d.Severity == DiagnosticSeverity.Warning);

    /// <inheritdoc/>
    public bool HasErrors => _items.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <inheritdoc/>
    public void Report(Diagnostic diagnostic) => _items.Add(diagnostic);

    /// <inheritdoc/>
    public IReadOnlyList<Diagnostic> ToReadOnly() => _items.AsReadOnly();

    /// <summary>Vrátí pouze chyby.</summary>
    public IReadOnlyList<Diagnostic> Errors() =>
        _items.Where(d => d.Severity == DiagnosticSeverity.Error).ToList().AsReadOnly();

    /// <summary>Vrátí pouze varování.</summary>
    public IReadOnlyList<Diagnostic> Warnings() =>
        _items.Where(d => d.Severity == DiagnosticSeverity.Warning).ToList().AsReadOnly();

    /// <summary>Vytvoří kopii bagu.</summary>
    public DiagnosticBag Clone()
    {
        var clone = new DiagnosticBag();
        clone._items.AddRange(_items);
        return clone;
    }

    /// <summary>Přidá všechny zprávy z jiného bagu.</summary>
    public void Merge(DiagnosticBag other) => _items.AddRange(other._items);
}
