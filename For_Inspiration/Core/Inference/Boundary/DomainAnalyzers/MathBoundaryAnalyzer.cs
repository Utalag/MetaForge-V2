using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Primitives;

namespace MetaForge.Core.Inference.Boundary.DomainAnalyzers;

/// <summary>
/// Specializovaný analyzér pro matematické metody.
/// Detekuje hraniční stavy jako dělení nulou, overflow, NaN, Infinity.
/// </summary>
public sealed class MathBoundaryAnalyzer : IDomainAnalyzer
{
    public string DomainName => "math";
    
    public IReadOnlyList<string> Keywords => new[] 
    { 
        "calculate", "compute", "solve", "evaluate", "sqrt", "square", 
        "root", "quadratic", "cubic", "polynomial", "factor", "div", "mod",
        "sin", "cos", "tan", "log", "ln", "exp", "pow", "gcd", "lcm",
        "discriminant", "slope", "intercept", "magnitude", "normalize"
    };
    
    public int Priority => 100;
    
    public bool IsAvailable => true;
    
    /// <summary>
    /// Matematické funkce, které vyžadují nezáporné argumenty.
    /// </summary>
    private static readonly HashSet<string> NonNegativeFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "sqrt", "log", "ln", "square", "root"
    };

    public bool CanHandle(Method method)
    {
        var name = method.Name.ToLowerInvariant();
        var hasNumericParams = method.Parameters.Any(p => IsNumericType(p.Type.BaseType));
        var hasMathKeyword = Keywords.Any(k => name.Contains(k));
        
        return hasNumericParams && (hasMathKeyword || IsMathOperation(method));
    }
    
    public Task<IReadOnlyList<MethodConstraint>> AnalyzeAsync(Method method)
    {
        var constraints = new List<MethodConstraint>();
        
        AnalyzeDivisionByZero(method, constraints);
        AnalyzeOverflowPotential(method, constraints);
        AnalyzeDomainErrors(method, constraints);
        AnalyzeQuadraticEquation(method, constraints);
        AnalyzeLogarithmDomain(method, constraints);
        AnalyzeTrigonometricDomain(method, constraints);
        AnalyzePowerFunction(method, constraints);
        
        return Task.FromResult<IReadOnlyList<MethodConstraint>>(constraints);
    }
    
    /// <summary>
    /// Detekuje dělení nulou z ComputedExpressions.
    /// </summary>
    private void AnalyzeDivisionByZero(Method method, List<MethodConstraint> constraints)
    {
        foreach (var param in method.Parameters.Where(p => IsNumericType(p.Type.BaseType)))
        {
            if (IsUsedAsDivisor(param.Name, method.BodyExpressions))
            {
                var condition = $"{param.Name} == 0";
                if (!constraints.Any(c => c.InvalidCondition == condition))
                {
                    constraints.Add(new MethodConstraint
                    {
                        InvalidCondition = condition,
                        Description = $"Parametr '{param.Name}' je použit jako dělitel — nesmí být nula.",
                        Kind = ConstraintKind.Precondition,
                        ExceptionType = "DivideByZeroException",
                        ExceptionMessage = $"Parametr '{param.Name}' nemůže být nula (dělení)."
                    });
                }
            }
        }
    }
    
    /// <summary>
    /// Detekuje potenciální overflow pro velmi velké/smallé hodnoty.
    /// </summary>
    private void AnalyzeOverflowPotential(Method method, List<MethodConstraint> constraints)
    {
        foreach (var param in method.Parameters.Where(p => IsNumericType(p.Type.BaseType)))
        {
            if (param.Type.BaseType is DataType.Float or DataType.Double or 
                DataType.Int or DataType.Long)
            {
                if (IsSquaredOrDoubled(param.Name, method.BodyExpressions))
                {
                    constraints.Add(new MethodConstraint
                    {
                        InvalidCondition = $"Math.Abs({param.Name}) > 1e9",
                        Description = $"Hodnota '{param.Name}' může způsobit overflow při násobení.",
                        Kind = ConstraintKind.Precondition,
                        ExceptionType = "OverflowException",
                        ExceptionMessage = $"Hodnota '{param.Name}' je příliš velká pro bezpečný výpočet."
                    });
                }
            }
        }
    }
    
    /// <summary>
    /// Detekuje domain errors (log záporného čísla, atd.).
    /// </summary>
    private void AnalyzeDomainErrors(Method method, List<MethodConstraint> constraints)
    {
        var name = method.Name.ToLowerInvariant();
        
        if (name.Contains("log") || name.Contains("ln"))
        {
            foreach (var param in method.Parameters.Where(p => p.Type.BaseType == DataType.Double))
            {
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"{param.Name} <= 0",
                    Description = $"Logaritmus '{param.Name}' musí být > 0.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "ArgumentOutOfRangeException",
                    ExceptionMessage = $"Logaritmus: hodnota '{param.Name}' musí být větší než nula."
                });
            }
        }
        
        if (name.Contains("sqrt") || name.Contains("root"))
        {
            foreach (var param in method.Parameters.Where(p => IsNumericType(p.Type.BaseType)))
            {
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"{param.Name} < 0",
                    Description = $"Odmocnina '{param.Name}' musí být >= 0.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "ArgumentOutOfRangeException",
                    ExceptionMessage = $"Odmocnina: hodnota '{param.Name}' nesmí být záporná."
                });
            }
        }
    }
    
    /// <summary>
    /// Specializovaná analýza pro kvadratickou rovnici ax² + bx + c = 0.
    /// </summary>
    private void AnalyzeQuadraticEquation(Method method, List<MethodConstraint> constraints)
    {
        if (!method.Name.Contains("quadratic", StringComparison.OrdinalIgnoreCase))
            return;
            
        var a = method.Parameters.FirstOrDefault(p => 
            p.Name.Equals("a", StringComparison.OrdinalIgnoreCase));
        var b = method.Parameters.FirstOrDefault(p => 
            p.Name.Equals("b", StringComparison.OrdinalIgnoreCase));
        var c = method.Parameters.FirstOrDefault(p => 
            p.Name.Equals("c", StringComparison.OrdinalIgnoreCase));
            
        if (a == null || b == null || c == null)
            return;
            
        constraints.Add(new MethodConstraint
        {
            InvalidCondition = $"{a.Name} == 0",
            Description = "Koeficient 'a' nemůže být nula (dělení nulou ve vzorci).",
            Kind = ConstraintKind.Precondition,
            ExceptionType = "ArgumentException",
            ExceptionMessage = "Koeficient 'a' kvadratické rovnice nemůže být nula."
        });
        
        constraints.Add(new MethodConstraint
        {
            InvalidCondition = $"({b.Name} * {b.Name} - 4 * {a.Name} * {c.Name}) < 0",
            Description = "Záporný diskriminant → žádné reálné kořeny.",
            Kind = ConstraintKind.Precondition,
            ExceptionType = "InvalidOperationException",
            ExceptionMessage = "Rovnice nemá reálné řešení (záporný diskriminant)."
        });
        
        constraints.Add(new MethodConstraint
        {
            InvalidCondition = $"2 * {a.Name} == 0",
            Description = "Jmenovatel (2a) by byl nula.",
            Kind = ConstraintKind.Precondition,
            ExceptionType = "DivideByZeroException",
            ExceptionMessage = "Jmenovatel (2a) nemůže být nula."
        });
    }
    
    /// <summary>
    /// Analýza logaritmické funkce.
    /// </summary>
    private void AnalyzeLogarithmDomain(Method method, List<MethodConstraint> constraints)
    {
        if (!method.Parameters.Any(p => p.Name.Contains("arg", StringComparison.OrdinalIgnoreCase) ||
                                        p.Name.Contains("value", StringComparison.OrdinalIgnoreCase)))
            return;
            
        var valueParam = method.Parameters.FirstOrDefault(p => 
            p.Name.Contains("arg", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains("value", StringComparison.OrdinalIgnoreCase));
            
        if (valueParam != null && IsNumericType(valueParam.Type.BaseType))
        {
            constraints.Add(new MethodConstraint
            {
                InvalidCondition = $"{valueParam.Name} <= 0",
                Description = "Logaritmus vyžaduje kladný argument.",
                Kind = ConstraintKind.Precondition,
                ExceptionType = "ArgumentOutOfRangeException",
                ExceptionMessage = $"Hodnota '{valueParam.Name}' musí být větší než nula."
            });
        }
    }
    
    /// <summary>
    /// Analýza trigonometrických funkcí.
    /// </summary>
    private void AnalyzeTrigonometricDomain(Method method, List<MethodConstraint> constraints)
    {
        var name = method.Name.ToLowerInvariant();
        
        if (name.Contains("asin") || name.Contains("acos"))
        {
            foreach (var param in method.Parameters.Where(p => IsNumericType(p.Type.BaseType)))
            {
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"Math.Abs({param.Name}) > 1",
                    Description = "asin/acos akceptují pouze hodnoty v rozsahu [-1, 1].",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "ArgumentOutOfRangeException",
                    ExceptionMessage = $"Hodnota '{param.Name}' musí být v rozsahu [-1, 1]."
                });
            }
        }
    }
    
    /// <summary>
    /// Analýza mocninné funkce.
    /// </summary>
    private void AnalyzePowerFunction(Method method, List<MethodConstraint> constraints)
    {
        if (!method.Name.Contains("pow", StringComparison.OrdinalIgnoreCase))
            return;
            
        if (method.Parameters.Count >= 2)
        {
            var baseParam = method.Parameters.First();
            var expParam = method.Parameters.Skip(1).FirstOrDefault();
            
            if (expParam != null)
            {
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"{baseParam.Name} < 0",
                    Description = "Záporný základ s neceločíselným exponentem produkuje komplexní číslo.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "InvalidOperationException",
                    ExceptionMessage = "Záporný základ s neceločíselným exponentem není podporován."
                });
            }
        }
    }
    
    private static bool IsNumericType(DataType type) => type switch
    {
        DataType.Int or DataType.Long or DataType.Short or DataType.Byte
            or DataType.Float or DataType.Double or DataType.Decimal => true,
        _ => false
    };
    
    private static bool IsUsedAsDivisor(string paramName, IEnumerable<ComputedExpression> expressions)
    {
        foreach (var expr in expressions)
        {
            var code = expr.GenerateCode();
            if (string.IsNullOrWhiteSpace(code))
                continue;
                
            var patterns = new[] { $"/ {paramName}", $"/{paramName}", $"/({paramName}" };
            if (patterns.Any(p => code.Contains(p, StringComparison.Ordinal)))
                return true;
        }
        return false;
    }
    
    private static bool IsSquaredOrDoubled(string paramName, IEnumerable<ComputedExpression> expressions)
    {
        foreach (var expr in expressions)
        {
            var code = expr.GenerateCode();
            if (code.Contains($"{paramName} * {paramName}", StringComparison.Ordinal) ||
                code.Contains($"{paramName}*{paramName}", StringComparison.Ordinal) ||
                code.Contains($"{paramName} + {paramName}", StringComparison.Ordinal))
                return true;
        }
        return false;
    }
    
    private static bool IsMathOperation(Method method)
    {
        var paramTypes = method.Parameters.Select(p => p.Type.BaseType).ToList();
        
        if (paramTypes.Count(p => IsNumericType(p)) >= 3)
            return true;
            
        if ((method.ReturnType.BaseType == DataType.Double || 
             method.ReturnType.BaseType == DataType.Float) &&
            paramTypes.Any(IsNumericType))
            return true;
            
        return false;
    }
}
