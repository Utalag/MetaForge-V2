using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Builders;

/// <summary>
/// Fluent builder pro InterfaceElement.
/// </summary>
public sealed class InterfaceBuilder
{
    private readonly InterfaceElement _iface = new();

    public InterfaceBuilder(string name)
    {
        _iface.Name = name;
    }

    public InterfaceBuilder Property(string name, TypeModel type, Action<PropertyBuilder>? configure = null)
    {
        var builder = new PropertyBuilder(name, type);
        configure?.Invoke(builder);
        _iface.Properties.Add(builder.Build());
        return this;
    }

    public InterfaceBuilder Method(string name, Action<MethodBuilder>? configure = null)
    {
        var builder = new MethodBuilder(name);
        configure?.Invoke(builder);
        _iface.Methods.Add(builder.Build());
        return this;
    }

    public InterfaceBuilder Metadata(Action<MetadataBag> configure)
    {
        configure(_iface.Metadata);
        return this;
    }

    public InterfaceElement Build() => _iface;
}
