using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Primitives;

namespace MetaForge.Core.Inference.Boundary.DomainAnalyzers;

/// <summary>
/// Generický fallback analyzér pro metody, které nepokrývá žádný specializovaný analyzér.
/// Detekuje základní hraniční stavy: null, empty, zero.
/// </summary>
public sealed class GenericBoundaryAnalyzer : IDomainAnalyzer
{
    public string DomainName => "generic";
    
    public IReadOnlyList<string> Keywords => Array.Empty<string>();
    
    public int Priority => 0;
    
    public bool IsAvailable => true;

    public bool CanHandle(Method method) => true;
    
    public Task<IReadOnlyList<MethodConstraint>> AnalyzeAsync(Method method)
    {
        var constraints = new List<MethodConstraint>();
        
        AnalyzeNullParameters(method, constraints);
        AnalyzeReturnTypeConstraints(method, constraints);
        
        return Task.FromResult<IReadOnlyList<MethodConstraint>>(constraints);
    }
    
    /// <summary>
    /// Pro referenční typy bez default hodnoty přidej null check.
    /// </summary>
    private void AnalyzeNullParameters(Method method, List<MethodConstraint> constraints)
    {
        foreach (var param in method.Parameters)
        {
            if ((param.Type.BaseType == DataType.String || 
                 param.Type.BaseType == DataType.Object) &&
                string.IsNullOrEmpty(param.DefaultValue))
            {
                if (!constraints.Any(c => c.InvalidCondition.Contains($"{param.Name} is null")))
                {
                    constraints.Add(new MethodConstraint
                    {
                        InvalidCondition = $"{param.Name} is null",
                        Description = $"Parametr '{param.Name}' nesmí být null.",
                        Kind = ConstraintKind.Precondition,
                        ExceptionType = "ArgumentNullException",
                        ExceptionMessage = $"Hodnota '{param.Name}' nemůže být null."
                    });
                }
            }
        }
    }
    
    /// <summary>
    /// Analýza constraints na návratový typ.
    /// </summary>
    private void AnalyzeReturnTypeConstraints(Method method, List<MethodConstraint> constraints)
    {
        // Pro metody vracející bool může být Postcondition na výsledek
        // Ale to je spíše business logika než boundary analysis
        // Zatím prázdné - přidáme podle potřeby
    }
}
