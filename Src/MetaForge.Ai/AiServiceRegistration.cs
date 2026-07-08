using Microsoft.Extensions.DependencyInjection;
using MetaForge.Ai.Abstractions;
using MetaForge.Ai.Adapters;
using MetaForge.Ai.Inference;
using MetaForge.Ai.Prompts;
using MetaForge.Ai.Translation;
using MetaForge.Core.Inference;
using MetaForge.Translator.Translation;

namespace MetaForge.Ai;

/// <summary>
/// Extension metody pro DI registraci AI služeb.
/// </summary>
public static class AiServiceRegistration
{
    /// <summary>
    /// Zaregistruje AI služby do DI containeru.
    /// Volitelné — pokud se nezavolá, použijí se deterministické fallbacky.
    /// </summary>
    public static IServiceCollection AddMetaForgeAi(
        this IServiceCollection services,
        string ollamaUrl = "http://localhost:11434",
        string model = "gemma3:12b")
    {
        // Transportní adapter
        services.AddSingleton<IAiBackendAdapter>(_ =>
            new OllamaAdapter(ollamaUrl, model));

        // AI implementace — nahradí deterministické fallbacky
        services.AddSingleton<IConstraintInferencer, AiConstraintInferencer>();
        services.AddSingleton<ITranslationService, AiTranslationService>();

        // Prompt registry a evaluace
        services.AddSingleton<PromptRegistry>();
        services.AddSingleton<PromptEvaluationService>();

        return services;
    }
}
