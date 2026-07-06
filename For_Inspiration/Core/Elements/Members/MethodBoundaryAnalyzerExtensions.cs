using System.ComponentModel;
using System.Reactive.Linq;
using MetaForge.Core.Configuration;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Internal.Testing;
using MetaForge.Core.Inference;
using MetaForge.Core.Inference.Boundary;
using MetaForge.Core.Inference.Boundary.Models;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Extension methods for Method that integrate boundary analysis with reactive change detection.
/// Tests are automatically triggered when the method's behavior-relevant properties change.
/// </summary>
public static class MethodBoundaryAnalyzerExtensions
{
    private static readonly Dictionary<Method, IDisposable> _subscriptions = new();

    // Factory pro vytvoření AI inferenceru — registruje se z MetaForge.Ai assembly.
    // Core záměrně neodkazuje MetaForge.Ai (AI je volitelná vrstva).
    // Pokud factory není zaregistrována, AI analýza gracefully selže.
    private static Func<AIInferenceSettings, IConstraintInferencer>? _aiInferencerFactory;

    /// <summary>
    /// Registruje factory pro vytvoření AI inferenceru.
    /// Voláno z MetaForge.Ai nebo MetaForge.Builders assembly — Core nezávisí na konkrétní implementaci.
    /// <code>
    /// MethodBoundaryAnalyzerExtensions.RegisterAiInferencerFactory(
    ///     settings => new MetaForge.Ai.AiConstraintInferencer(settings));
    /// </code>
    /// </summary>
    public static void RegisterAiInferencerFactory(Func<AIInferenceSettings, IConstraintInferencer> factory)
        => _aiInferencerFactory = factory ?? throw new ArgumentNullException(nameof(factory));

    // Lazy-loaded analyzers
    private static MethodBoundaryAnalyzer? _ruleBasedAnalyzer;
    private static MethodBoundaryAnalyzer? _aiAnalyzer;
    private static AIInferenceSettings? _aiSettings;
    
    /// <summary>
    /// Timeout for debouncing rapid changes (ms).
    /// </summary>
    public const int DebounceMilliseconds = 500;

    /// <summary>
    /// Timeout for overall analysis operation (ms).
    /// </summary>
    public const int AnalysisTimeoutMilliseconds = 30000;

    /// <summary>
    /// Registers a method for automatic boundary analysis when its behavior-relevant properties change.
    /// Uses AI if configured and available.
    /// </summary>
    /// <param name="method">The method to monitor.</param>
    /// <param name="aiSettings">AI settings (optional). If null, uses rule-based only.</param>
    /// <param name="autoAnalyzeConstraints">If true, automatically adds detected constraints to the method.</param>
    /// <param name="onAnalysisComplete">Optional callback when analysis completes.</param>
    /// <returns>IDisposable to stop monitoring.</returns>
    public static IDisposable RegisterForBoundaryAnalysis(
        this Method method,
        AIInferenceSettings? aiSettings = null,
        bool autoAnalyzeConstraints = true,
        Action<BoundaryAnalysisResult>? onAnalysisComplete = null)
    {
        if (_subscriptions.ContainsKey(method))
        {
            _subscriptions[method].Dispose();
        }

        // Use AI analyzer if settings provided and AI is enabled
        var analyzer = GetAnalyzer(aiSettings);
        var disposables = new CompositeDisposable();

        // Subscribe to relevant property changes
        var propertyChanges = Observable
            .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => method.PropertyChanged += h,
                h => method.PropertyChanged -= h)
            .Where(e => IsBehaviorRelevantProperty(e.EventArgs.PropertyName))
            .Select(_ => method);

        // Subscribe to collection changes (Parameters, BodyExpressions, Constraints)
        var parameterChanges = Observable
            .FromEventPattern<System.Collections.Specialized.NotifyCollectionChangedEventHandler, 
                System.Collections.Specialized.NotifyCollectionChangedEventArgs>(
                h => method.Parameters.CollectionChanged += h,
                h => method.Parameters.CollectionChanged -= h)
            .Select(_ => method);

        var bodyExpressionChanges = Observable
            .FromEventPattern<System.Collections.Specialized.NotifyCollectionChangedEventHandler, 
                System.Collections.Specialized.NotifyCollectionChangedEventArgs>(
                h => method.BodyExpressions.CollectionChanged += h,
                h => method.BodyExpressions.CollectionChanged -= h)
            .Select(_ => method);

        var combinedChanges = propertyChanges
            .Merge(parameterChanges)
            .Merge(bodyExpressionChanges)
            .Throttle(TimeSpan.FromMilliseconds(DebounceMilliseconds))
            .Select(m => m);

        var subscription = combinedChanges.Subscribe(async m =>
        {
            try
            {
                var result = await AnalyzeMethodAsync(m, analyzer, autoAnalyzeConstraints);
                onAnalysisComplete?.Invoke(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Boundary analysis error: {ex.Message}");
            }
        });

        disposables.Add(subscription);
        _subscriptions[method] = disposables;

        return disposables;
    }

    /// <summary>
    /// Performs boundary analysis on the method synchronously.
    /// Uses rule-based only.
    /// </summary>
    public static async Task<BoundaryAnalysisResult> AnalyzeBoundaryAsync(
        this Method method,
        bool addConstraints = true)
    {
        return await AnalyzeMethodAsync(method, GetRuleBasedAnalyzer(), addConstraints);
    }

    /// <summary>
    /// Performs boundary analysis with AI support.
    /// </summary>
    public static async Task<BoundaryAnalysisResult> AnalyzeBoundaryWithAIAsync(
        this Method method,
        AIInferenceSettings aiSettings,
        bool addConstraints = true)
    {
        var analyzer = GetAnalyzer(aiSettings);
        return await AnalyzeMethodAsync(method, analyzer, addConstraints);
    }

    /// <summary>
    /// Unregisters a method from automatic boundary analysis.
    /// </summary>
    public static void UnregisterFromBoundaryAnalysis(this Method method)
    {
        if (_subscriptions.TryGetValue(method, out var subscription))
        {
            subscription.Dispose();
            _subscriptions.Remove(method);
        }
    }

    /// <summary>
    /// Gets the current AI settings.
    /// </summary>
    public static AIInferenceSettings? GetAISettings() => _aiSettings;

    /// <summary>
    /// Checks if AI is currently enabled and available.
    /// </summary>
    public static bool IsAIAvailable()
    {
        return _aiSettings?.Enabled == true && _aiAnalyzer != null;
    }

    /// <summary>
    /// Gets the rule-based analyzer (always available).
    /// </summary>
    private static MethodBoundaryAnalyzer GetRuleBasedAnalyzer()
    {
        return _ruleBasedAnalyzer ??= MethodBoundaryAnalyzer.Default;
    }

    /// <summary>
    /// Gets the analyzer based on AI settings.
    /// </summary>
    private static MethodBoundaryAnalyzer GetAnalyzer(AIInferenceSettings? aiSettings)
    {
        if (aiSettings?.Enabled != true)
        {
            return GetRuleBasedAnalyzer();
        }

        // Check if we need to create a new AI analyzer
        if (_aiAnalyzer == null || !Equals(_aiSettings, aiSettings))
        {
            _aiSettings = aiSettings;
            var aiInferencer = _aiInferencerFactory?.Invoke(aiSettings)
                ?? throw new InvalidOperationException(
                    "AI inferencer factory není zaregistrována. " +
                    "Zavolejte MethodBoundaryAnalyzerExtensions.RegisterAiInferencerFactory() " +
                    "před použitím AI boundary analýzy.");
            _aiAnalyzer = MethodBoundaryAnalyzer.CreateWithAI(aiInferencer);
        }

        return _aiAnalyzer;
    }

    /// <summary>
    /// Performs boundary analysis and adds detected constraints to the method.
    /// </summary>
    private static async Task<BoundaryAnalysisResult> AnalyzeMethodAsync(
        Method method,
        MethodBoundaryAnalyzer analyzer,
        bool addConstraints)
    {
        var result = await analyzer.AnalyzeAsync(method);

        if (addConstraints)
        {
            foreach (var constraint in result.Constraints)
            {
                if (!method.Constraints.Any(c => c.InvalidCondition == constraint.InvalidCondition))
                {
                    method.Constraints.Add(constraint);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Determines if a property change is relevant to method behavior.
    /// </summary>
    private static bool IsBehaviorRelevantProperty(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return false;

        return propertyName switch
        {
            nameof(Method.Name) => true,
            nameof(Method.ReturnType) => true,
            nameof(Method.Body) => true,
            nameof(Method.IsStatic) => true,
            nameof(Method.IsAsync) => true,
            nameof(Method.AccessModifier) => false,
            nameof(Method.IsVirtual) => false,
            nameof(Method.IsOverride) => false,
            nameof(Method.IsAbstract) => false,
            nameof(Method.Documentation) => false,
            nameof(Method.ResolvedBody) => false,
            _ => false
        };
    }

    /// <summary>
    /// Checks if a method is currently registered for boundary analysis.
    /// </summary>
    public static bool IsRegisteredForBoundaryAnalysis(this Method method)
    {
        return _subscriptions.ContainsKey(method);
    }

    /// <summary>
    /// Gets the count of methods currently registered for boundary analysis.
    /// </summary>
    public static int GetRegisteredMethodCount()
    {
        return _subscriptions.Count;
    }

    /// <summary>
    /// Disposes all active subscriptions. Call during application shutdown.
    /// </summary>
    public static void DisposeAllAnalysisSubscriptions()
    {
        foreach (var subscription in _subscriptions.Values)
        {
            subscription.Dispose();
        }
        _subscriptions.Clear();
        _ruleBasedAnalyzer = null;
        _aiAnalyzer = null;
    }
}

/// <summary>
/// Composite disposable for managing multiple subscriptions.
/// </summary>
internal class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    public void Add(IDisposable disposable)
    {
        if (!_disposed)
        {
            _disposables.Add(disposable);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            foreach (var d in _disposables)
            {
                d.Dispose();
            }
            _disposables.Clear();
        }
    }
}
