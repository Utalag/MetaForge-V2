using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;

namespace MetaForge.Core.Builders;

/// <summary>
/// Fluent builder pro MethodElement.
/// </summary>
public sealed class MethodBuilder
{
    private readonly MethodElement _method;

    public MethodBuilder(string name)
    {
        _method = new MethodElement { Name = name, ReturnType = TypeModel.Void };
    }

    public MethodBuilder Returns(TypeModel type) { _method.ReturnType = type; return this; }
    public MethodBuilder Async() { _method.IsAsync = true; return this; }
    public MethodBuilder Static() { _method.IsStatic = true; return this; }
    public MethodBuilder Abstract() { _method.IsAbstract = true; _method.Body = null; return this; }
    public MethodBuilder Virtual() { _method.IsVirtual = true; return this; }
    public MethodBuilder Override() { _method.IsOverride = true; return this; }
    public MethodBuilder Extension() { _method.IsExtension = true; _method.IsStatic = true; return this; }

    public MethodBuilder Param(string name, TypeModel type, Action<ParameterBuilder>? configure = null)
    {
        var builder = new ParameterBuilder(name, type);
        configure?.Invoke(builder);
        _method.Parameters.Add(builder.Build());
        return this;
    }

    public MethodBuilder Body(BlockStatement body) { _method.Body = body; return this; }
    public MethodBuilder ExpressionBody(Expression expr) { _method.ExpressionBody = expr; return this; }

    public MethodBuilder Metadata(Action<MetadataBag> configure)
    {
        configure(_method.Metadata);
        return this;
    }

    public MethodElement Build() => _method;
}
