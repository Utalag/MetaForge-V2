using System.Text.RegularExpressions;
using MetaForge.Core.Common;
using MetaForge.Core.StandardLibraries;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Obecny renderer kanonickych `mf.<library>.<function>(...)` wrapper volani.
/// </summary>
public static class SemanticStandardLibrary
{
    private static readonly Regex CanonicalLibraryCallRegex = new(
        @"(?<![\w.])mf\.(?<library>[A-Za-z_]\w*)\.(?<function>[A-Za-z_]\w*)\s*\(",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string RenderInExpression(string? expression, ProgramLanguage language)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return expression ?? string.Empty;

        var matches = CanonicalLibraryCallRegex.Matches(expression);
        if (matches.Count == 0)
            return expression;

        var rendered = expression;
        var libraries = matches
            .Select(match => match.Groups["library"].Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var libraryName in libraries)
        {
            if (!StandardLibraryTranslatorRegistry.Instance.TryGetFunctionMappings(libraryName, language, out var functionMappings)
                || functionMappings.Count == 0)
            {
                if (ContainsCanonicalLibraryCall(rendered, libraryName))
                    throw new InvalidOperationException($"No translator is registered for standard library '{libraryName}' and language '{language}'.");

                continue;
            }

            foreach (var (functionName, targetCall) in functionMappings)
            {
                var pattern = $@"(?<![\w.])mf\.{Regex.Escape(libraryName)}\.{Regex.Escape(functionName)}\s*\(";
                rendered = Regex.Replace(rendered, pattern, targetCall + "(", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }

            if (ContainsCanonicalLibraryCall(rendered, libraryName))
                throw new InvalidOperationException($"Translator for standard library '{libraryName}' does not cover all semantic calls for language '{language}'.");
        }

        return rendered;
    }

    private static bool ContainsCanonicalLibraryCall(string expression, string libraryName)
    {
        var pattern = $@"(?<![\w.])mf\.{Regex.Escape(libraryName)}\.[A-Za-z_]\w*\s*\(";
        return Regex.IsMatch(expression, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}