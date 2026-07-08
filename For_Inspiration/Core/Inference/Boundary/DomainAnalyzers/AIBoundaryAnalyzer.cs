using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Inference.Boundary.DomainAnalyzers;

/// <summary>
/// AI-based boundary analyzer.
/// Využívá AI model (Ollama, OpenAI, MiniMax) pro detekci komplexních hraničních stavů.
/// </summary>
public sealed class AIBoundaryAnalyzer : IDomainAnalyzer
{
    private readonly IConstraintInferencer _aiInferencer;

    public string DomainName => "ai";

    public IReadOnlyList<string> Keywords => Array.Empty<string>();

    public int Priority => 200; // Nejvyšší priorita - AI je přesnější

    public bool IsAvailable => _aiInferencer?.IsAvailable ?? false;

    public AIBoundaryAnalyzer(IConstraintInferencer aiInferencer)
    {
        _aiInferencer = aiInferencer ?? throw new ArgumentNullException(nameof(aiInferencer));
    }

    public bool CanHandle(Method method)
    {
        // AI analyzer handles everything when available
        return IsAvailable;
    }

    public async Task<IReadOnlyList<MethodConstraint>> AnalyzeAsync(Method method)
    {
        if (!IsAvailable)
        {
            return Array.Empty<MethodConstraint>();
        }

        try
        {
            // Získej existující constrainty z rule-based analyzérů
            var existingConstraints = method.Constraints
                .Where(c => c.Source == ConstraintSource.FromRuleBased)
                .ToList();

            // Nech AI navrhně další na základě těchto
            var aiConstraints = await _aiInferencer.InferConstraintsAsync(
                method, 
                existingConstraints);

            return aiConstraints;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AI Analysis failed: {ex.Message}");
            return Array.Empty<MethodConstraint>();
        }
    }
}
