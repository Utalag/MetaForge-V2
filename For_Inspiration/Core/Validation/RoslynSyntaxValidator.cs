using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MetaForge.Core.Validation;

/// <summary>
/// Validátor generovaného kódu pomocí Roslyn.
/// Ověřuje syntaktickou správnost C# kódu bez kompilace — pouze parse fáze.
/// </summary>
public static class RoslynSyntaxValidator
{
    /// <summary>
    /// Validuje jeden C# člen (property, field, metoda) zabalením do pomocné třídy.
    /// Vhodné pro výstup z Property.GenerateCode() nebo Field.GenerateCode().
    /// </summary>
    public static RoslynValidationResult ValidateMember(string memberCode)
    {
        var source = "class __Wrapper__\n{\n    " + memberCode.Replace("\n", "\n    ") + "\n}";
        return ValidateSource(source);
    }

    /// <summary>
    /// Validuje kompletní C# zdrojový kód (namespace, třída, atd.).
    /// </summary>
    public static RoslynValidationResult ValidateSource(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);

        var errors = tree.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => new RoslynSyntaxError(
                d.Id,
                d.GetMessage(),
                d.Location.GetLineSpan().StartLinePosition.Line,
                d.Location.GetLineSpan().StartLinePosition.Character))
            .ToList();

        return new RoslynValidationResult(errors);
    }
}

/// <summary>
/// Výsledek Roslyn syntaktické validace.
/// </summary>
public sealed class RoslynValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public IReadOnlyList<RoslynSyntaxError> Errors { get; }

    internal RoslynValidationResult(IEnumerable<RoslynSyntaxError> errors)
    {
        Errors = errors.ToList();
    }

    public override string ToString() =>
        IsValid ? "OK" : string.Join(Environment.NewLine, Errors);
}

/// <summary>
/// Jeden syntaktický error z Roslyn diagnostiky.
/// </summary>
public sealed record RoslynSyntaxError(string Id, string Message, int Line, int Column)
{
    public override string ToString() => $"[{Id}] ({Line},{Column}): {Message}";
}
