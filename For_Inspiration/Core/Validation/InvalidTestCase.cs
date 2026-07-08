namespace MetaForge.Core.Validation;

/// <summary>
/// Jeden odvozený testovací případ pro neplatnou hodnotu.
/// Generován metodou InvalidValueProfile.ResolveTestCases().
/// Scriban šablona iteruje přes kolekci těchto záznamů.
/// </summary>
/// <param name="Label">Identifikátor test case (např. "EmptyString", "BelowMin").</param>
/// <param name="CSharpLiteral">C# literál pro použití v testu (např. "\"\"", "-1", "null").</param>
/// <param name="Reason">Důvod proč je hodnota neplatná (pro assertion message).</param>
public sealed record InvalidTestCase(
    string Label,
    string CSharpLiteral,
    string Reason);
