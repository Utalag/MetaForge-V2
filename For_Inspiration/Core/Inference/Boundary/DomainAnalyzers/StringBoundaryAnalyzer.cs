using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Primitives;

namespace MetaForge.Core.Inference.Boundary.DomainAnalyzers;

/// <summary>
/// Specializovaný analyzér pro metody pracující se stringy.
/// Detekuje hraniční stavy jako prázdný string, whitespace, max length.
/// </summary>
public sealed class StringBoundaryAnalyzer : IDomainAnalyzer
{
    public string DomainName => "string";
    
    public IReadOnlyList<string> Keywords => new[]
    {
        "parse", "format", "split", "join", "replace", "trim",
        "contains", "indexof", "substring", "validate", "sanitize",
        "encode", "decode", "convert", "capitalize", "lower", "upper"
    };
    
    public int Priority => 90;
    
    public bool IsAvailable => true;

    public bool CanHandle(Method method)
    {
        var name = method.Name.ToLowerInvariant();
        var hasStringParams = method.Parameters.Any(p => p.Type.BaseType == DataType.String);
        var hasStringKeyword = Keywords.Any(k => name.Contains(k));
        
        return hasStringParams && (hasStringKeyword || hasStringParams);
    }
    
    public Task<IReadOnlyList<MethodConstraint>> AnalyzeAsync(Method method)
    {
        var constraints = new List<MethodConstraint>();
        
        AnalyzeEmptyString(method, constraints);
        AnalyzeWhitespace(method, constraints);
        AnalyzeIndexBounds(method, constraints);
        AnalyzeEncoding(method, constraints);
        
        return Task.FromResult<IReadOnlyList<MethodConstraint>>(constraints);
    }
    
    private void AnalyzeEmptyString(Method method, List<MethodConstraint> constraints)
    {
        foreach (var param in method.Parameters.Where(p => p.Type.BaseType == DataType.String))
        {
            if (string.IsNullOrEmpty(param.DefaultValue))
            {
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"string.IsNullOrEmpty({param.Name})",
                    Description = $"Parametr '{param.Name}' nesmí být prázdný string.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "ArgumentException",
                    ExceptionMessage = $"Hodnota '{param.Name}' nesmí být prázdný řetězec."
                });
            }
        }
    }
    
    private void AnalyzeWhitespace(Method method, List<MethodConstraint> constraints)
    {
        foreach (var param in method.Parameters.Where(p => p.Type.BaseType == DataType.String))
        {
            var name = method.Name.ToLowerInvariant();
            
            if (name.Contains("parse") || name.Contains("validate") || name.Contains("format"))
            {
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"string.IsNullOrWhiteSpace({param.Name})",
                    Description = $"Parametr '{param.Name}' nesmí být whitespace-only.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "ArgumentException",
                    ExceptionMessage = $"Hodnota '{param.Name}' nesmí být prázdný nebo obsahovat pouze whitespace."
                });
            }
        }
    }
    
    private void AnalyzeIndexBounds(Method method, List<MethodConstraint> constraints)
    {
        var name = method.Name.ToLowerInvariant();
        
        if (name.Contains("index") || name.Contains("char") || name.Contains("substring"))
        {
            var stringParam = method.Parameters.FirstOrDefault(p => p.Type.BaseType == DataType.String);
            var indexParam = method.Parameters.FirstOrDefault(p => 
                p.Name.Contains("index", StringComparison.OrdinalIgnoreCase) ||
                p.Name == "i" || p.Name == "pos");
                
            if (stringParam != null && indexParam != null)
            {
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"{indexParam.Name} < 0",
                    Description = $"Index '{indexParam.Name}' nesmí být záporný.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "ArgumentOutOfRangeException",
                    ExceptionMessage = $"Index '{indexParam.Name}' nemůže být záporný."
                });
                
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"{indexParam.Name} >= {stringParam.Name}.Length",
                    Description = $"Index '{indexParam.Name}' nesmí přesáhnout délku řetězce.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "ArgumentOutOfRangeException",
                    ExceptionMessage = $"Index '{indexParam.Name}' je mimo rozsah řetězce."
                });
            }
        }
    }
    
    private void AnalyzeEncoding(Method method, List<MethodConstraint> constraints)
    {
        var name = method.Name.ToLowerInvariant();
        
        if (name.Contains("encode") || name.Contains("decode") || name.Contains("convert"))
        {
            foreach (var param in method.Parameters.Where(p => p.Type.BaseType == DataType.String))
            {
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"{param.Name} is null",
                    Description = $"Parametr '{param.Name}' nesmí být null pro kódování.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "ArgumentNullException",
                    ExceptionMessage = $"Hodnota '{param.Name}' nemůže být null."
                });
            }
        }
    }
}
