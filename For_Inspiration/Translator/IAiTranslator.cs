using MetaForge.Core.Common;

namespace MetaForge.Translator;

/// <summary>
/// AI-assisted translator interface.
/// Implementations may use local LLMs (Ollama), Semantic Kernel, or cloud APIs.
/// If AI is not available, methods return null and the system falls back to deterministic translation.
/// </summary>
public interface IAiTranslator
{
    /// <summary>
    /// Runs a generic completion with explicit system and user prompts.
    /// Used by higher orchestration layers such as business conversations.
    /// </summary>
    Task<string?> CompletePromptAsync(string systemPrompt, string userPrompt);

    /// <summary>
    /// Returns true if the AI backend is available and responding.
    /// </summary>
    bool IsAvailable { get; }
}
