using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MetaForge.Generators.Tests;

/// <summary>
/// Validuje syntaxi vygenerovaného C# kódu pomocí Roslyn.
/// Používá se v testech pro ověření, že generátor produkuje syntakticky korektní C#.
/// Nepotřebuje assembly reference — pouze parsuje syntaxi.
/// </summary>
public static class SyntaxValidator
{
    /// <summary>
    /// Validuje C# syntaxi. Vrací true pokud je kód syntakticky korektní,
    /// jinak false + výpis chyb do <paramref name="diagnostics"/>.
    /// </summary>
    /// <param name="sourceCode">C# zdrojový kód k validaci.</param>
    /// <param name="diagnostics">Výpis syntaktických chyb (prázdný pokud validní).</param>
    /// <returns>True pokud je kód syntakticky korektní.</returns>
    public static bool IsValid(string sourceCode, out string diagnostics)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            diagnostics = "Source code is empty.";
            return false;
        }

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode,
            CSharpParseOptions.Default.WithKind(SourceCodeKind.Regular));

        var errors = syntaxTree.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToList();

        if (errors.Count == 0)
        {
            diagnostics = string.Empty;
            return true;
        }

        diagnostics = string.Join(Environment.NewLine,
            errors.Select(e =>
            {
                var span = e.Location.GetLineSpan();
                return $"  Line {span.StartLinePosition.Line + 1,4}, Col {span.StartLinePosition.Character + 1,3}: {e.GetMessage()}";
            }));

        return false;
    }
}
