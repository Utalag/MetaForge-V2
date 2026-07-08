using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Builders;

/// <summary>
/// Fluent builder pro PropertyElement.
/// </summary>
public sealed class PropertyBuilder
{
    private readonly PropertyElement _prop;

    public PropertyBuilder(string name, TypeModel type)
    {
        _prop = new PropertyElement { Name = name, Type = type };
    }

    public PropertyBuilder Get() { _prop.HasGetter = true; return this; }
    public PropertyBuilder Set() { _prop.HasSetter = true; return this; }
    public PropertyBuilder Init() { _prop.IsInitOnly = true; _prop.HasSetter = false; return this; }
    public PropertyBuilder GetSet() { _prop.HasGetter = true; _prop.HasSetter = true; return this; }
    public PropertyBuilder Required() { _prop.IsRequired = true; return this; }
    public PropertyBuilder Static() { _prop.IsStatic = true; return this; }
    public PropertyBuilder Default(string value) { _prop.DefaultValue = value; return this; }

    public PropertyBuilder Metadata(Action<MetadataBag> configure)
    {
        configure(_prop.Metadata);
        return this;
    }

    public PropertyElement Build() => _prop;
}
