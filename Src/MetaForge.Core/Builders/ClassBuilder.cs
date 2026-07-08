using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Builders;

/// <summary>
/// Fluent builder pro ClassElement.
/// </summary>
public sealed class ClassBuilder
{
    private readonly ClassElement _class = new();

    public ClassBuilder(string name)
    {
        _class.Name = name;
    }

    public ClassBuilder Sealed() { _class.IsSealed = true; return this; }
    public ClassBuilder Abstract() { _class.IsAbstract = true; return this; }
    public ClassBuilder Static() { _class.IsStatic = true; return this; }
    public ClassBuilder Partial() { _class.IsPartial = true; return this; }
    public ClassBuilder Record() { _class.IsRecord = true; return this; }

    public ClassBuilder BaseClass(string name) { _class.BaseClassName = name; return this; }
    public ClassBuilder Implements(string name) { _class.ImplementedInterfaces.Add(name); return this; }

    public ClassBuilder Property(string name, TypeModel type, Action<PropertyBuilder>? configure = null)
    {
        var builder = new PropertyBuilder(name, type);
        configure?.Invoke(builder);
        _class.Properties.Add(builder.Build());
        return this;
    }

    public ClassBuilder Method(string name, Action<MethodBuilder>? configure = null)
    {
        var builder = new MethodBuilder(name);
        configure?.Invoke(builder);
        _class.Methods.Add(builder.Build());
        return this;
    }

    public ClassBuilder Attribute(string name, params object?[] args)
    {
        var attr = new AttributeElement { Name = name };
        attr.Arguments.AddRange(args);
        _class.Attributes.Add(attr);
        return this;
    }

    public ClassBuilder Metadata(Action<MetadataBag> configure)
    {
        configure(_class.Metadata);
        return this;
    }

    public ClassElement Build() => _class;
}
