using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Primitives;

namespace MetaForge.Core.Inference.Boundary.DomainAnalyzers;

/// <summary>
/// Specializovaný analyzér pro metody pracující s kolekcemi.
/// Detekuje hraniční stavy jako index out of bounds, empty collection.
/// </summary>
public sealed class CollectionBoundaryAnalyzer : IDomainAnalyzer
{
    public string DomainName => "collection";
    
    public IReadOnlyList<string> Keywords => new[]
    {
        "get", "add", "remove", "insert", "delete", "at",
        "index", "element", "item", "first", "last", "nth",
        "head", "tail", "push", "pop", "enqueue", "dequeue"
    };
    
    public int Priority => 80;
    
    public bool IsAvailable => true;

    public bool CanHandle(Method method)
    {
        var name = method.Name.ToLowerInvariant();
        var hasCollectionParams = method.Parameters.Any(p => 
            p.Type.IsCollection || 
            p.Type.BaseType == DataType.Object);
        
        return hasCollectionParams || Keywords.Any(k => name.Contains(k));
    }
    
    public Task<IReadOnlyList<MethodConstraint>> AnalyzeAsync(Method method)
    {
        var constraints = new List<MethodConstraint>();
        
        AnalyzeIndexAccess(method, constraints);
        AnalyzeEmptyCollection(method, constraints);
        AnalyzeCapacity(method, constraints);
        
        return Task.FromResult<IReadOnlyList<MethodConstraint>>(constraints);
    }
    
    private void AnalyzeIndexAccess(Method method, List<MethodConstraint> constraints)
    {
        var name = method.Name.ToLowerInvariant();
        
        if (name.Contains("get") && (name.Contains("index") || name.Contains("element") || name.Contains("item")))
        {
            var collectionParam = method.Parameters.FirstOrDefault(p => p.Type.IsCollection);
            var indexParam = method.Parameters.FirstOrDefault(p => 
                p.Name.Contains("index", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains("i", StringComparison.OrdinalIgnoreCase) ||
                p.Name == "n" || p.Name == "pos");
                
            if (collectionParam != null && indexParam != null)
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
                    InvalidCondition = $"{indexParam.Name} >= {collectionParam.Name}.Count",
                    Description = $"Index '{indexParam.Name}' nesmí přesáhnout počet prvků.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "ArgumentOutOfRangeException",
                    ExceptionMessage = $"Index '{indexParam.Name}' je mimo rozsah kolekce."
                });
            }
        }
    }
    
    private void AnalyzeEmptyCollection(Method method, List<MethodConstraint> constraints)
    {
        var name = method.Name.ToLowerInvariant();
        
        if (name.Contains("first") || name.Contains("last") || name.Contains("head") || name.Contains("tail"))
        {
            var collectionParam = method.Parameters.FirstOrDefault(p => p.Type.IsCollection);
            
            if (collectionParam != null)
            {
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"{collectionParam.Name}.Count == 0",
                    Description = "Kolekce nesmí být prázdná pro tuto operaci.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "InvalidOperationException",
                    ExceptionMessage = "Kolekce je prázdná — operace není možná."
                });
            }
        }
    }
    
    private void AnalyzeCapacity(Method method, List<MethodConstraint> constraints)
    {
        var name = method.Name.ToLowerInvariant();
        
        if (name.Contains("insert") || name.Contains("add") || name.Contains("resize"))
        {
            var collectionParam = method.Parameters.FirstOrDefault(p => p.Type.IsCollection);
            var indexParam = method.Parameters.FirstOrDefault(p => 
                p.Name.Contains("index", StringComparison.OrdinalIgnoreCase));
                
            if (collectionParam != null && indexParam != null)
            {
                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = $"{indexParam.Name} > {collectionParam.Name}.Count",
                    Description = $"Index '{indexParam.Name}' nesmí být větší než počet prvků.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "ArgumentOutOfRangeException",
                    ExceptionMessage = $"Index '{indexParam.Name}' je mimo povolený rozsah."
                });
            }
        }
    }
}
