using System.Diagnostics;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Inference.Boundary.Models;

namespace MetaForge.Core.Inference.Boundary;

/// <summary>
/// Hlavní orchestrátor pro boundary analýzu metod.
/// Registruje a spouští doménově-specifické analyzéry.
/// </summary>
public sealed class MethodBoundaryAnalyzer
{
    private readonly List<IDomainAnalyzer> _analyzers;
    // Přímý přístup k RulesBoundaryAnalyzer pro AddRule() — vyhne se nutnosti castovat z _analyzers.
    private readonly DomainAnalyzers.RulesBoundaryAnalyzer _rulesAnalyzer;
    
    /// <summary>
    /// Singleton instance pro snadné použití (pouze rule-based).
    /// Pro AI podporu použijte CreateWithAI().
    /// </summary>
    public static MethodBoundaryAnalyzer Default { get; } = new();

    /// <summary>
    /// Vytvoří instanci s AI podporou.
    /// </summary>
    /// <param name="aiInferencer">AI inferencer (Ollama, OpenAI, MiniMax, ...)</param>
    /// <returns>MethodBoundaryAnalyzer s registrovaným AI analyzérem.</returns>
    public static MethodBoundaryAnalyzer CreateWithAI(IConstraintInferencer aiInferencer)
    {
        var analyzer = new MethodBoundaryAnalyzer();
        
        if (aiInferencer.IsAvailable)
        {
            analyzer.Register(new DomainAnalyzers.AIBoundaryAnalyzer(aiInferencer));
        }
        
        return analyzer;
    }
    
    public MethodBoundaryAnalyzer()
    {
        _analyzers = new List<IDomainAnalyzer>();
        _rulesAnalyzer = new DomainAnalyzers.RulesBoundaryAnalyzer();
        RegisterDefaultAnalyzers();
    }
    
    /// <summary>
    /// Zaregistruje výchozí doménové analyzéry (bez AI).
    /// </summary>
    private void RegisterDefaultAnalyzers()
    {
        Register(_rulesAnalyzer);
        Register(new DomainAnalyzers.MathBoundaryAnalyzer());
        Register(new DomainAnalyzers.FinanceBoundaryAnalyzer());
        Register(new DomainAnalyzers.StringBoundaryAnalyzer());
        Register(new DomainAnalyzers.CollectionBoundaryAnalyzer());
        Register(new DomainAnalyzers.GenericBoundaryAnalyzer());
    }
    
    /// <summary>
    /// Zaregistruje vlastní doménový analyzér (plugin).
    /// </summary>
    public MethodBoundaryAnalyzer Register(IDomainAnalyzer analyzer)
    {
        _analyzers.Add(analyzer);
        _analyzers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        return this;
    }

    /// <summary>
    /// Přidá konfigurační pravidlo do vestavěného <see cref="DomainAnalyzers.RulesBoundaryAnalyzer"/>.
    /// Vhodné pro jednoduché podmínky bez nutnosti psát vlastní IDomainAnalyzer plugin.
    /// </summary>
    /// <example>
    /// <code>
    /// MethodBoundaryAnalyzer.Default.AddRule(new BoundaryRule
    /// {
    ///     ParamNamePattern = "timeout|delay",
    ///     Condition        = "{param} &lt; 0",
    ///     ExceptionType    = "ArgumentOutOfRangeException",
    ///     ExceptionMessage = "Parametr '{param}' nesmí být záporný."
    /// });
    /// </code>
    /// </example>
    public MethodBoundaryAnalyzer AddRule(BoundaryRule rule)
    {
        _rulesAnalyzer.AddRule(rule);
        return this;
    }

    /// <summary>
    /// Přidá více konfiguračních pravidel najednou.
    /// </summary>
    public MethodBoundaryAnalyzer AddRules(IEnumerable<BoundaryRule> rules)
    {
        _rulesAnalyzer.AddRules(rules);
        return this;
    }
    
    /// <summary>
    /// Provede kompletní boundary analýzu metody.
    /// Spustí nejrelevantnější analyzér (AI má prioritu 200, rule-based max 100).
    /// </summary>
    /// <param name="method">Metoda k analýze.</param>
    /// <returns>Výsledek analýzy se všemi hraničními stavy.</returns>
    public async Task<BoundaryAnalysisResult> AnalyzeAsync(Method method)
    {
        var stopwatch = Stopwatch.StartNew();
        var allConstraints = new List<MethodConstraint>();
        
        // Nejprve spustíme rule-based analyzéry pro rychlé výsledky
        var ruleBasedAnalyzers = _analyzers
            .Where(a => a is not DomainAnalyzers.AIBoundaryAnalyzer && a.IsAvailable && a.CanHandle(method))
            .OrderByDescending(a => a.Priority)
            .ToList();

        foreach (var analyzer in ruleBasedAnalyzers)
        {
            var constraints = await analyzer.AnalyzeAsync(method);
            allConstraints.AddRange(constraints);
        }

        // Pak spustíme AI analyzér (pokud je dostupný) pro komplexní případy
        var aiAnalyzer = _analyzers
            .OfType<DomainAnalyzers.AIBoundaryAnalyzer>()
            .FirstOrDefault(a => a.IsAvailable);

        if (aiAnalyzer != null)
        {
            var aiConstraints = await aiAnalyzer.AnalyzeAsync(method);
            allConstraints.AddRange(aiConstraints);
        }

        var uniqueConstraints = DeduplicateConstraints(allConstraints);
        
        // Urči detekovanou doménu
        var detectedDomain = aiAnalyzer?.DomainName ?? 
            ruleBasedAnalyzers.FirstOrDefault()?.DomainName ?? "unknown";

        stopwatch.Stop();
        
        var summary = BuildAnalysisSummary(method, uniqueConstraints, detectedDomain, aiAnalyzer?.IsAvailable == true);
        
        return new BoundaryAnalysisResult(
            MethodName: method.Name,
            Constraints: uniqueConstraints,
            DetectedDomain: detectedDomain,
            AnalysisSummary: summary,
            AnalysisDuration: stopwatch.Elapsed
        );
    }
    
    /// <summary>
    /// Provede boundary analýzu a přidá výsledky přímo do metody.
    /// </summary>
    public async Task<BoundaryAnalysisResult> AnalyzeAndAddConstraintsAsync(Method method)
    {
        var result = await AnalyzeAsync(method);
        
        foreach (var constraint in result.Constraints)
        {
            if (!method.Constraints.Any(c => c.InvalidCondition == constraint.InvalidCondition))
            {
                method.Constraints.Add(constraint);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Získá seznam registrovaných analyzérů.
    /// </summary>
    public IReadOnlyList<IDomainAnalyzer> GetAnalyzers() => _analyzers.AsReadOnly();

    /// <summary>
    /// Odstraní duplicitní constrainty na základě InvalidCondition.
    /// </summary>
    private static List<MethodConstraint> DeduplicateConstraints(List<MethodConstraint> constraints)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unique = new List<MethodConstraint>();
        
        foreach (var constraint in constraints)
        {
            if (seen.Add(constraint.InvalidCondition))
            {
                unique.Add(constraint);
            }
        }
        
        return unique;
    }
    
    /// <summary>
    /// Sestaví čitelnou zprávu o analýze.
    /// </summary>
    private static string BuildAnalysisSummary(Method method, 
        IReadOnlyList<MethodConstraint> constraints, string domain, bool aiEnabled)
    {
        var aiNote = aiEnabled ? " [AI enhanced]" : "";
        
        if (constraints.Count == 0)
            return $"✓ Metoda '{method.Name}' — žádné hraniční stavy detekovány.{aiNote}";
            
        return $"Analýza '{method.Name}' (doména: {domain}): " +
               $"{constraints.Count} hraniční stav/y detekován.{aiNote}";
    }
}
