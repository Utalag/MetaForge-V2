namespace MetaForge.Translator;

/// <summary>
/// Sdileny kontrakt pro AI klienty, ktere umi prijmout libovolny system/user prompt.
/// Node assist ho pouziva pro reuse existujici translation AI vrstvy bez separatniho bootstrapu.
/// </summary>
internal interface IPromptCompletionAiClient
{
    bool IsAvailable { get; }

    Task<string?> CompletePromptAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}