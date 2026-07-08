using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Builders;

/// <summary>
/// Fluent builder pro StructElement.
/// </summary>
public sealed class StructBuilder
{
    private readonly StructElement _struct = new();

    public StructBuilder(string name)
    {
        _struct.Name = name;
    }

    public StructBuilder ReadOnly() { _struct.IsReadOnly = true; return this; }
    public StructBuilder Record() { _struct.IsRecord = true; return this; }

    public StructBuilder Property(string name, TypeModel type, Action<PropertyBuilder>? configure = null)
    {
        var builder = new PropertyBuilder(name, type);
        configure?.Invoke(builder);
        _struct.Properties.Add(builder.Build());
        return this;
    }

    public StructBuilder Method(string name, Action<MethodBuilder>? configure = null)
    {
        var builder = new MethodBuilder(name);
        configure?.Invoke(builder);
        _struct.Methods.Add(builder.Build());
        return this;
    }

    public StructBuilder Metadata(Action<MetadataBag> configure)
    {
        configure(_struct.Metadata);
        return this;
    }

    public StructElement Build() => _struct;
}
