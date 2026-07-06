using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MetaForge.Core.Integration.Tests;

/// <summary>
/// Validuje syntaxi vygenerovaného C# kódu pomocí Roslyn.
/// </summary>
public static class SyntaxValidator
{
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
            .Where(d => d.Severity == DiagnosticSeverity.Error)
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
