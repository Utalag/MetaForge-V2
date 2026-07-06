using MetaForge.Core.Common;
using MetaForge.Core.StandardLibraries;
using System.Text.RegularExpressions;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Interní renderer pro překlad kanonických <c>mf.math.*</c> volání na výrazy cílového jazyka.
/// Veřejné builder API (Sqrt, Abs, Pow, …) žije v <c>MetaForge.ForgeBlocks.Math.SemanticMath</c>.
/// </summary>
public static class MathSemanticRenderer
{
    private const string LibraryName = "math";
    private static readonly Regex AnyMathCallRegex = new(
        @"(?<![\w.])(?:mf\.math|math|Math|System\.Math)\.",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex CanonicalMathCallRegex = new(
        @"(?<![\w.])mf\.math\.[A-Za-z_]\w*\s*\(",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string RenderInExpression(string? expression, ProgramLanguage language)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return expression ?? string.Empty;

        if (!AnyMathCallRegex.IsMatch(expression))
            return expression;

        if (!StandardLibraryTranslatorRegistry.Instance.TryGetFunctionMappings(LibraryName, language, out var functionMappings) || functionMappings.Count == 0)
        {
            if (CanonicalMathCallRegex.IsMatch(expression))
                throw new InvalidOperationException($"No translator is registered for standard library '{LibraryName}' and language '{language}'.");

            return expression;
        }

        var rendered = expression;

        foreach (var (functionName, targetCall) in functionMappings)
        {
            rendered = ReplaceFunctionCall(rendered, functionName, targetCall);
        }

        if (CanonicalMathCallRegex.IsMatch(rendered))
            throw new InvalidOperationException($"Translator for standard library '{LibraryName}' does not cover all semantic math calls for language '{language}'.");

        return rendered;
    }

    private static string ReplaceFunctionCall(string input, string functionName, string targetCall)
    {
        var pattern = $@"(?<![\w.])(?:mf\.math|math|Math|System\.Math)\.{Regex.Escape(functionName)}\s*\(";
        return Regex.Replace(input, pattern, targetCall + "(", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}