using MetaForge.Ai.Abstractions;

namespace MetaForge.Ai.Prompts;

/// <summary>
/// Automatické vyhodnocení kvality promptů na testovacích datech.
/// Spouští prompt proti AI backendu a validuje výstupy.
/// </summary>
public sealed class PromptEvaluationService
{
    private readonly IAiBackendAdapter _backend;

    /// <summary>
    /// Vytvoří evaluační službu s daným AI backendem.
    /// </summary>
    public PromptEvaluationService(IAiBackendAdapter backend)
    {
        _backend = backend;
    }

    /// <summary>
    /// Vyhodnotí prompt na sadě testovacích případů.
    /// </summary>
    /// <param name="prompt">Šablona promptu k vyhodnocení.</param>
    /// <param name="testCases">Sada testovacích případů.</param>
    /// <returns>Výsledek vyhodnocení s úspěšností.</returns>
    public async Task<PromptEvalResult> EvaluateAsync(
        PromptTemplate prompt,
        IReadOnlyList<PromptTestCase> testCases)
    {
        var results = new List<TestCaseResult>();

        foreach (var testCase in testCases)
        {
            try
            {
                // Sestavit prompt dosazením placeholderů
                var placeholders = new Dictionary<string, string>
                {
                    ["input"] = testCase.Input
                };
                var userPrompt = prompt.BuildPrompt(placeholders);

                // Poslat do AI backendu
                var output = await _backend.SendAsync(
                    $"{prompt.SystemPrompt}\n\n{userPrompt}");

                // Validovat výstup
                var passed = testCase.Validator(output);
                results.Add(new TestCaseResult
                {
                    TestName = testCase.Name,
                    Passed = passed,
                    Output = output,
                });
            }
            catch (Exception ex)
            {
                results.Add(new TestCaseResult
                {
                    TestName = testCase.Name,
                    Passed = false,
                    Output = null,
                    Error = ex.Message,
                });
            }
        }

        var passRate = results.Count > 0
            ? (double)results.Count(r => r.Passed) / results.Count
            : 0.0;

        return new PromptEvalResult
        {
            PromptName = prompt.Name,
            PassRate = passRate,
            Results = results.AsReadOnly(),
        };
    }

    /// <summary>
    /// Porovná dvě verze promptu a vrátí tu s lepší úspěšností.
    /// </summary>
    public async Task<PromptTemplate?> SelectBestAsync(
        PromptTemplate version1,
        PromptTemplate version2,
        IReadOnlyList<PromptTestCase> testCases)
    {
        var result1 = await EvaluateAsync(version1, testCases);
        var result2 = await EvaluateAsync(version2, testCases);

        return result1.PassRate >= result2.PassRate ? version1 : version2;
    }
}
