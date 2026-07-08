using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Primitives;
using MetaForge.Core.ValueObjects;

namespace MetaForge.Core.Internal.Testing;

/// <summary>
/// Sleduje změny v Method a spouští boundary testy pouze když je to potřeba.
/// Interní komponenta - NIKDY neexponuje C# kód uživateli.
/// </summary>
public sealed class ReactiveBoundaryTester
{
    private readonly RoslynTestRunner _roslynRunner;
    private readonly ConcurrentDictionary<string, MethodSnapshot> _snapshots = new();
    private readonly TimeSpan _debounceDelay;
    
    /// <summary>
    /// Singleton instance pro snadné použití.
    /// </summary>
    public static ReactiveBoundaryTester Default { get; } = new();
    
    public ReactiveBoundaryTester(TimeSpan? debounceDelay = null)
    {
        _roslynRunner = new RoslynTestRunner();
        _debounceDelay = debounceDelay ?? TimeSpan.FromMilliseconds(500);
    }
    
    /// <summary>
    /// Spustí testy pouze pokud se něco podstatného změnilo.
    /// Vrací null pokud se nic podstatného nezměnilo.
    /// </summary>
    public async Task<BoundaryTestResult?> TestIfChangedAsync(Method method)
    {
        var methodId = GetMethodId(method);
        
        var currentHash = ComputeMethodHash(method);
        
        if (_snapshots.TryGetValue(methodId, out var snapshot))
        {
            if (snapshot.Hash == currentHash)
                return null;
                
            if (!RequiresRetest(method, snapshot))
                return null;
        }
        
        var result = await _roslynRunner.RunAsync(method);
        
        _snapshots[methodId] = new MethodSnapshot
        {
            Hash = currentHash,
            TestedAt = DateTime.UtcNow,
            ResultHash = result.ResultHash,
            ParameterHashes = ComputeParameterHashes(method),
            ConstraintHash = ComputeConstraintHash(method)
        };
        
        return result;
    }
    
    /// <summary>
    /// Provede boundary analýzu a přidá výsledky do metody.
    /// </summary>
    public async Task<BoundaryTestResult?> AnalyzeAndUpdateConstraintsAsync(Method method)
    {
        var result = await TestIfChangedAsync(method);
        
        if (result == null)
            return null;
        
        foreach (var boundary in result.Boundaries)
        {
            if (!method.Constraints.Any(c => c.InvalidCondition == boundary.Condition))
            {
                method.Constraints.Add(new MethodConstraint
                {
                    InvalidCondition = boundary.Condition,
                    ExceptionType = boundary.ExceptionType,
                    Description = boundary.Description,
                    Kind = ConstraintKind.Precondition,
                    Source = ConstraintSource.FromRoslynTest
                });
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Určuje, zda změna vyžaduje retest.
    /// </summary>
    private bool RequiresRetest(Method currentMethod, MethodSnapshot snapshot)
    {
        var currentParamHashes = ComputeParameterHashes(currentMethod);
        var currentConstraintHash = ComputeConstraintHash(currentMethod);
        
        if (!Equals(snapshot.ParameterHashes, currentParamHashes))
            return true;
            
        if (!Equals(snapshot.ConstraintHash, currentConstraintHash))
            return true;
            
        return false;
    }
    
    /// <summary>
    /// Vypočte hash metody pro změnovou detekci.
    /// </summary>
    private string ComputeMethodHash(Method method)
    {
        var sb = new StringBuilder();
        
        foreach (var expr in method.BodyExpressions)
        {
            sb.Append(expr.Operation.GetHashCode());
            sb.Append(expr.LeftOperand ?? "");
            sb.Append(expr.RightOperand ?? "");
            sb.Append(expr.MinValue ?? "");
            sb.Append(expr.MaxValue ?? "");
        }
        
        foreach (var param in method.Parameters)
        {
            sb.Append(param.Type.BaseType.GetHashCode());
            sb.Append(param.Type.CustomTypeName ?? "");
            sb.Append(param.Type.IsCollection);
        }
        
        foreach (var prop in method.AffectedProperties)
        {
            sb.Append(ComputeStrongTypeHash(prop.StrongType));
        }
        
        foreach (var field in method.AffectedFields)
        {
            sb.Append(ComputeStrongTypeHash(field.StrongType));
        }
        
        sb.Append(method.ReturnType.BaseType.GetHashCode());
        
        return ComputeHash(sb.ToString());
    }
    
    private string ComputeStrongTypeHash(StrongType? strongType)
    {
        if (strongType == null) return "";
        
        var sb = new StringBuilder();
        sb.Append(strongType.Name ?? "");
        
        if (strongType.ValidationRules != null)
        {
            foreach (var rule in strongType.ValidationRules)
            {
                sb.Append(rule.RuleType);
                sb.Append(rule.Parameter ?? "");
            }
        }
        
        return ComputeHash(sb.ToString());
    }
    
    private string ComputeParameterHashes(Method method)
    {
        var sb = new StringBuilder();
        foreach (var param in method.Parameters)
        {
            sb.Append(param.Type.BaseType);
            sb.Append(param.Type.CustomTypeName ?? "");
        }
        return ComputeHash(sb.ToString());
    }
    
    private string ComputeConstraintHash(Method method)
    {
        var sb = new StringBuilder();
        foreach (var prop in method.AffectedProperties)
        {
            if (prop.StrongType?.ValidationRules != null)
            {
                foreach (var rule in prop.StrongType.ValidationRules)
                {
                    sb.Append(rule.RuleType);
                    sb.Append(rule.Parameter ?? "");
                }
            }
        }
        return ComputeHash(sb.ToString());
    }
    
    private static string ComputeHash(string content)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }
    
    private static string GetMethodId(Method method)
    {
        return $"{method.GetHashCode()}_{method.Name}";
    }
}

/// <summary>
/// Uložený stav metody pro cache invalidaci.
/// </summary>
internal sealed class MethodSnapshot
{
    public string Hash { get; init; } = "";
    public DateTime TestedAt { get; init; }
    public string ResultHash { get; init; } = "";
    public string ParameterHashes { get; init; } = "";
    public string ConstraintHash { get; init; } = "";
}
