using System.Text.RegularExpressions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Primitives;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Detekuje použití kanonických wrapper knihoven a převádí je na importy a balíčkové požadavky cílového jazyka.
/// </summary>
public static class StandardLibraryRequirementResolver
{
    private static readonly Regex CanonicalLibraryCallRegex = new(
        @"(?<![\w.])mf\.(?<library>[A-Za-z_]\w*)\.[A-Za-z_]\w*\s*\(",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static IReadOnlyCollection<string> GetUsedLibraries(ILanguageElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        var libraries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CollectFromElement(element, libraries);

        return [.. libraries.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)];
    }

    public static StandardLibraryRequirements Resolve(ILanguageElement element, ProgramLanguage language)
    {
        ArgumentNullException.ThrowIfNull(element);

        var imports = new HashSet<string>(StringComparer.Ordinal);
        var packages = new Dictionary<string, CodePackageDependency>(StringComparer.OrdinalIgnoreCase);

        foreach (var libraryName in GetUsedLibraries(element))
        {
            if (!StandardLibraryTranslatorRegistry.Instance.TryGetTranslator(libraryName, out var translator))
                throw new InvalidOperationException($"No standard library translator is registered for library '{libraryName}'.");

            foreach (var importName in translator.GetRequiredImports(language))
            {
                if (!string.IsNullOrWhiteSpace(importName))
                    imports.Add(importName);
            }

            foreach (var package in translator.GetRequiredPackages(language))
            {
                if (string.IsNullOrWhiteSpace(package.PackageId))
                    continue;

                var key = $"{package.PackageManager}|{package.PackageId}";
                if (!packages.TryGetValue(key, out var existing)
                    || string.Compare(package.Version, existing.Version, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    packages[key] = package;
                }
            }
        }

        return new StandardLibraryRequirements
        {
            Imports = [.. imports.OrderBy(name => name, StringComparer.Ordinal)],
            Packages = [.. packages.Values
                .OrderBy(package => package.PackageManager, StringComparer.OrdinalIgnoreCase)
                .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)]
        };
    }

    private static void CollectFromElement(ILanguageElement element, ISet<string> libraries)
    {
        switch (element)
        {
            case Class cls:
                foreach (var field in cls.Fields)
                    CollectFromField(field, libraries);
                foreach (var property in cls.Properties)
                    CollectFromProperty(property, libraries);
                foreach (var constructor in cls.Constructors)
                    CollectFromConstructor(constructor, libraries);
                foreach (var method in cls.Methods)
                    CollectFromMethod(method, libraries);
                break;

            case Struct str:
                foreach (var field in str.Fields)
                    CollectFromField(field, libraries);
                foreach (var property in str.Properties)
                    CollectFromProperty(property, libraries);
                foreach (var constructor in str.Constructors)
                    CollectFromConstructor(constructor, libraries);
                foreach (var method in str.Methods)
                    CollectFromMethod(method, libraries);
                break;

            case Interface iface:
                foreach (var method in iface.Methods)
                    CollectFromMethod(method, libraries);
                break;

            case Method method:
                CollectFromMethod(method, libraries);
                break;

            case Constructor constructor:
                CollectFromConstructor(constructor, libraries);
                break;

            case Property property:
                CollectFromProperty(property, libraries);
                break;

            case Field field:
                CollectFromField(field, libraries);
                break;
        }
    }

    private static void CollectFromMethod(Method method, ISet<string> libraries)
    {
        CollectFromText(method.Body, libraries);

        foreach (var expression in method.BodyExpressions)
            CollectFromComputedExpression(expression, libraries);

        foreach (var constraint in method.Constraints)
        {
            CollectFromText(constraint.InvalidCondition, libraries);
            CollectFromText(constraint.ExceptionMessage, libraries);
        }
    }

    private static void CollectFromConstructor(Constructor constructor, ISet<string> libraries)
    {
        CollectFromText(constructor.Body, libraries);
        CollectFromText(constructor.BaseCall, libraries);

        foreach (var expression in constructor.BodyExpressions)
            CollectFromComputedExpression(expression, libraries);
    }

    private static void CollectFromProperty(Property property, ISet<string> libraries)
    {
        CollectFromText(property.DefaultValue, libraries);
        CollectFromText(property.GetterBody, libraries);
        CollectFromText(property.SetterBody, libraries);
        CollectFromComputedExpression(property.GetterExpression, libraries);

        foreach (var expression in property.SetterExpressions)
            CollectFromComputedExpression(expression, libraries);
    }

    private static void CollectFromField(Field field, ISet<string> libraries)
        => CollectFromText(field.DefaultValue, libraries);

    private static void CollectFromComputedExpression(ComputedExpression? expression, ISet<string> libraries)
    {
        if (expression == null)
            return;

        CollectFromText(expression.LeftOperand, libraries);
        CollectFromText(expression.RightOperand, libraries);
        CollectFromText(expression.MinValue, libraries);
        CollectFromText(expression.MaxValue, libraries);
        CollectFromText(expression.Message, libraries);
        CollectFromText(expression.FormatTemplate, libraries);
        CollectFromText(expression.RawCode, libraries);

        foreach (var argument in expression.Arguments)
            CollectFromText(argument, libraries);

        foreach (var condition in expression.Conditions)
            CollectFromText(condition, libraries);

        foreach (var branchCode in expression.BranchCodes)
            CollectFromText(branchCode, libraries);

        CollectFromText(expression.ElseCode, libraries);
        CollectFromComputedExpression(expression.Condition, libraries);
        CollectFromComputedExpression(expression.ThenBranch, libraries);
        CollectFromComputedExpression(expression.ElseBranch, libraries);

        foreach (var branch in expression.BranchExpressions)
            foreach (var branchExpression in branch)
                CollectFromComputedExpression(branchExpression, libraries);

        foreach (var elseExpression in expression.ElseExpressions)
            CollectFromComputedExpression(elseExpression, libraries);
    }

    private static void CollectFromText(string? text, ISet<string> libraries)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        foreach (Match match in CanonicalLibraryCallRegex.Matches(text))
        {
            var libraryName = match.Groups["library"].Value;
            if (!string.IsNullOrWhiteSpace(libraryName))
                libraries.Add(libraryName);
        }
    }
}